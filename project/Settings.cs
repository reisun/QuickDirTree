using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuickDirTree;

public class Settings: BaseResourcesReader<Settings> 
{
    public string AppVersion { get; set; }
    public string TargetDirectry { get; set; }
    public int LaunchCount { get; set; }
}