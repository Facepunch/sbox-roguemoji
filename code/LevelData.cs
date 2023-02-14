using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public class LevelData
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string WalkSound { get; set; }
    public string BgColorEven { get; set; }
    public string BgColorOdd { get; set; }
    public Dictionary<string, List<IntVector>> Things { get; set; }
    public Dictionary<string, int> RandomThings { get; set; }
}
