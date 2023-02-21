using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public class LevelData
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public SurfaceType SurfaceType { get; set; }
    public Dictionary<string, List<IntVector>> Things { get; set; }
    public Dictionary<string, int> RandomThings { get; set; }
}
