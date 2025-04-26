using System.Diagnostics;
using System.Reflection;

namespace QuickDirTree;

public static class MyApp
{
    public static string AppVersion
    {
        get {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            return versionInfo?.FileVersion ?? "0.0.0.0";
        }
    }
}