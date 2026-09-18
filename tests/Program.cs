using Newtonsoft.Json;
using QuickDirTree;

var root = Path.Combine(Path.GetTempPath(), "QuickDirTree-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var originalDirectory = Environment.CurrentDirectory;
var passed = 0;
try
{
    var errors = new List<string>();
    var path = Path.Combine(root, "appsettings.json");
    var settings = new Settings(path, errors.Add);
    Check(settings.TargetDirectries.Value.Count == 0 && !File.Exists(path), "Missing settings are not written on load");
    Check(settings.TrySetDirectories(["C:\\登録フォルダ", "D:\\Other"]), "First registration succeeds");
    Check(new Settings(path, errors.Add).TargetDirectries.Value.Count == 2, "Registration survives reload without application exit");
    Check(settings.TrySetDirectories(["D:\\Other"]), "Deletion succeeds");
    Check(new Settings(path, errors.Add).TargetDirectries.Value.SequenceEqual(["D:\\Other"]), "Deletion is immediately persisted");
    Check(settings.TrySetDirectories([]) && new Settings(path, errors.Add).TargetDirectries.Value.Count == 0, "Deleting the last folder persists an empty list");

    var otherDirectory = Directory.CreateDirectory(Path.Combine(root, "other")).FullName;
    Environment.CurrentDirectory = otherDirectory;
    Check(settings.TrySetDirectories(["C:\\Stable"]), "Save works after working directory changes");
    Check(new Settings(path, errors.Add).TargetDirectries.Value.SequenceEqual(["C:\\Stable"])
        && !File.Exists(Path.Combine(otherDirectory, "appsettings.json")), "Save and reload use the fixed location");
    Environment.CurrentDirectory = originalDirectory;

    var originalJson = File.ReadAllText(path);
    var cyclic = new List<object>();
    cyclic.Add(cyclic);
    try { JsonFile.Write(path, cyclic); throw new Exception("Expected serialization failure"); }
    catch (JsonException) { }
    Check(File.ReadAllText(path) == originalJson, "Serialization failure preserves the previous file");

    foreach (var invalid in new[] { "{", "", "null", "{}", "{\"TargetDirectries\":null}", "{\"TargetDirectries\":[null]}" })
    {
        var badPath = Path.Combine(root, "invalid.json");
        File.WriteAllText(badPath, invalid);
        try { _ = new Settings(badPath, errors.Add); throw new Exception("Expected invalid settings rejection"); }
        catch (JsonException) { }
        Check(File.ReadAllText(badPath) == invalid, "Invalid settings remain unchanged: " + invalid);
    }

    var blockedPath = Path.Combine(root, "blocked.json");
    var blocked = new Settings(blockedPath, errors.Add);
    Directory.CreateDirectory(blockedPath);
    Check(!blocked.TrySetDirectories(["C:\\Rejected"])
        && blocked.TargetDirectries.Value.Count == 0 && errors.Count == 1,
        "Failed commit reports the failure and does not update in-memory settings");
    Check(!Directory.EnumerateFiles(root, "*.tmp").Any(), "Temporary files are cleaned after failed commit");

    if (OperatingSystem.IsWindows())
    {
        using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Check(!settings.TrySetDirectories(["C:\\Rejected"]), "Locked destination rejects replacement");
            Check(settings.TargetDirectries.Value.SequenceEqual(["C:\\Stable"])
                && File.ReadAllText(path) == originalJson, "Failed replacement preserves disk and memory");
        }
        Check(!Directory.EnumerateFiles(root, "*.tmp").Any(), "Failed replacement cleans its temporary file");
    }
    else
    {
        Console.WriteLine("SKIP: Windows file-sharing replacement checks (run by Windows CI)");
    }
    Check(errors.Count == (OperatingSystem.IsWindows() ? 2 : 1), "Only expected save errors were reported");
    Console.WriteLine($"PASS: {passed} checks");
}
finally
{
    Environment.CurrentDirectory = originalDirectory;
    Directory.Delete(root, recursive: true);
}

void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed++;
    Console.WriteLine("PASS: " + name);
}
