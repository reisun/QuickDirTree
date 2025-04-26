using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuickDirTree;

public class Texts: BaseResourcesReader<Texts> 
{
    public string TargetDirectryEmpty { get; }
    public string TargetDirectryNotFound { get; }
}