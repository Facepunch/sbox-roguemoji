﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public enum EasingType
{
    None = -1,
    Linear = 0,
    SineIn, SineOut, SineInOut,
    QuadIn, QuadOut, QuadInOut,
    CubicIn, CubicOut, CubicInOut,
    QuartIn, QuartOut, QuartInOut,
    QuintIn, QuintOut, QuintInOut,
    ExpoIn, ExpoOut, ExpoInOut,
    ExtremeIn, ExtremeOut, ExtremeInOut,
    ElasticIn, ElasticOut, ElasticInOut,
    ElasticSoftIn, ElasticSoftOut, ElasticSoftInOut,
    BackIn, BackOut, BackInOut,
    BounceIn, BounceOut, BounceInOut
};

public struct AStarEdge<T>
{
    /// <summary>
    /// The node this connection leads to.
    /// </summary>
    public readonly T Dest;

    /// <summary>
    /// The cost of using this connection.
    /// </summary>
    public readonly float Cost;

    internal AStarEdge(T dest, float cost)
    {
        Dest = dest;
        Cost = cost;
    }
}

public static class Utils
{
    public static IEnumerable<Thing> WithAll(this IEnumerable<Thing> list, ThingFlags flags)
    {
        return list.Where(x => (x.Flags & flags) == flags);
    }

    public static IEnumerable<Thing> WithAny(this IEnumerable<Thing> list, ThingFlags flags)
    {
        return list.Where(x => (x.Flags & flags) != 0);
    }

    public static IEnumerable<Thing> WithNone(this IEnumerable<Thing> list, ThingFlags flags)
    {
        return list.Where(x => (x.Flags & flags) == 0);
    }

    public static void DrawCircle(Vector2 pos, float radius, int num_segments, float starting_angle, Color color)
    {
        var step = 2f * MathF.PI / (float)num_segments;
        for(int i = 1; i < num_segments + 1; i++)
        {
            var to_angle = i * step + starting_angle;
            var to_point = new Vector2(pos.x + radius * MathF.Cos(to_angle), pos.y + radius * MathF.Sin(to_angle));

            var from_angle = (i - 1) * step + starting_angle;
            var from_point = new Vector2(pos.x + radius * MathF.Cos(from_angle), pos.y + radius * MathF.Sin(from_angle));

            DebugOverlay.Line(from_point, to_point, color, 0f, false);
        }
    }

    public const float Deg2Rad = MathF.PI / 180f;
    public const float Rad2Deg = 360f / (MathF.PI * 2f);

    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        var rads = Deg2Rad * degrees;
        var ca = MathF.Cos(rads);
        var sa = MathF.Sin(rads);

        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }

    public static Vector2 DegreesToVector(float degrees)
    {
        var rads = Deg2Rad * degrees;
        return new Vector2(MathF.Cos(rads), MathF.Sin(rads));
    }

    public static int GetDistance(int x, int y)
    {
        return (int)Math.Round(Math.Sqrt(x * x + y * y));
    }

    public static int GetDistance(IntVector a, IntVector b)
    {
        var diff = b - a;
        return (int)Math.Round(Math.Sqrt(diff.x * diff.x + diff.y * diff.y));
    }

    public static bool IsOdd(IntVector gridPos)
    {
        return gridPos.x % 2 == gridPos.y % 2;
    }

    public static float FastSin(float input)
    {
        // wrap input angle to -PI..PI
        while (input < -3.14159265f)
            input += 6.28318531f;
        while (input > 3.14159265f)
            input -= 6.28318531f;

        return 1.27323954f * input + 0.405284735f * (input < 0f ? 1f : -1f) * input * input;
    }

    public static float DynamicEaseTo(float current, float goal, float factorPercent, float dt, float referenceFrameRate = 60f)
    {
        if (float.IsPositiveInfinity(dt)) 
            return goal;

        return current + (goal - current) * (1f - MathF.Pow(1f - MathX.Clamp(factorPercent, 0f, 1f), dt * referenceFrameRate));
    }

    public static Vector2 DynamicEaseTo(Vector2 current, Vector2 goal, float factorPercent, float dt, float referenceFrameRate = 60f)
    {
        if (float.IsPositiveInfinity(dt))
            return goal;
        var diff = goal - current;

        return current + diff * (1f - MathF.Pow(1f - MathX.Clamp(factorPercent, 0f, 1f), dt * referenceFrameRate));
    }

    public static float EaseUnclamped(float value, EasingType easingType)
    {
        switch (easingType)
        {
            case EasingType.SineIn: return SineIn(value);
            case EasingType.SineOut: return SineOut(value);
            case EasingType.SineInOut: return SineInOut(value);

            case EasingType.QuadIn: return QuadIn(value);
            case EasingType.QuadOut: return QuadOut(value);
            case EasingType.QuadInOut: return QuadInOut(value);

            case EasingType.CubicIn: return CubicIn(value);
            case EasingType.CubicOut: return CubicOut(value);
            case EasingType.CubicInOut: return CubicInOut(value);

            case EasingType.QuartIn: return QuartIn(value);
            case EasingType.QuartOut: return QuartOut(value);
            case EasingType.QuartInOut: return QuartInOut(value);

            case EasingType.QuintIn: return QuintIn(value);
            case EasingType.QuintOut: return QuintOut(value);
            case EasingType.QuintInOut: return QuintInOut(value);

            case EasingType.ExpoIn: return ExpoIn(value);
            case EasingType.ExpoOut: return ExpoOut(value);
            case EasingType.ExpoInOut: return ExpoInOut(value);

            case EasingType.ExtremeIn: return ExtremeIn(value);
            case EasingType.ExtremeOut: return ExtremeOut(value);
            case EasingType.ExtremeInOut: return ExtremeInOut(value);

            case EasingType.ElasticIn: return ElasticIn(value);
            case EasingType.ElasticOut: return ElasticOut(value);
            case EasingType.ElasticInOut: return ElasticInOut(value);

            case EasingType.ElasticSoftIn: return ElasticSoftIn(value);
            case EasingType.ElasticSoftOut: return ElasticSoftOut(value);
            case EasingType.ElasticSoftInOut: return ElasticSoftInOut(value);

            case EasingType.BackIn: return BackIn(value);
            case EasingType.BackOut: return BackOut(value);
            case EasingType.BackInOut: return BackInOut(value);

            case EasingType.BounceIn: return BounceIn(value);
            case EasingType.BounceOut: return BounceOut(value);
            case EasingType.BounceInOut: return BounceInOut(value);
            default: return value;
        }
    }

    public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax, EasingType easingType = EasingType.Linear, bool clamp = true)
    {
        if (inputMin.Equals(inputMax) || outputMin.Equals(outputMax))
            return outputMin;

        //            if (inputMin.Equals(inputMax) || outputMin.Equals(outputMax))
        //                return outputMax;

        if (clamp)
        {
            // clamp input
            if (inputMax > inputMin)
            {
                if (value < inputMin) value = inputMin;
                else if (value > inputMax) value = inputMax;
            }
            else if (inputMax < inputMin)
            {
                if (value > inputMin) value = inputMin;
                else if (value < inputMax) value = inputMax;
            }
        }

        var ratio = EaseUnclamped((value - inputMin) / (inputMax - inputMin), easingType);

        var outVal = outputMin + ratio * (outputMax - outputMin);

        //            // clamp output
        //            if (outputMax < outputMin) {
        //                if (outVal < outputMax) outVal = outputMax;
        //                else if (outVal > outputMin) outVal = outputMin;
        //            } else {
        //                if (outVal > outputMax) outVal = outputMax;
        //                else if (outVal < outputMin) outVal = outputMin;
        //            }

        return outVal;
    }

    public static float MapReturn(float value, float inputMin, float inputMax, float outputMin, float outputMax, EasingType easingType = EasingType.Linear)
    {
        var halfway = inputMin + (inputMax - inputMin) * 0.5f;
        if (value < halfway) return Map(value, inputMin, halfway, outputMin, outputMax, easingType);
        else return Map(value, halfway, inputMax, outputMax, outputMin, GetOppositeEasingType(easingType));
    }

    public static float EasePercent(float percent, EasingType easingType)
    {
        return Map(percent, 0f, 1f, 0f, 1f, easingType);
    }

    public static EasingType GetOppositeEasingType(EasingType easingType)
    {
        var opposite = EasingType.Linear;
        switch (easingType)
        {
            case EasingType.SineIn: opposite = EasingType.SineOut; break;
            case EasingType.SineOut: opposite = EasingType.SineIn; break;
            case EasingType.SineInOut: opposite = EasingType.SineInOut; break;

            case EasingType.QuadIn: opposite = EasingType.QuadOut; break;
            case EasingType.QuadOut: opposite = EasingType.QuadIn; break;
            case EasingType.QuadInOut: opposite = EasingType.QuadInOut; break;

            case EasingType.CubicIn: opposite = EasingType.CubicOut; break;
            case EasingType.CubicOut: opposite = EasingType.CubicIn; break;
            case EasingType.CubicInOut: opposite = EasingType.CubicInOut; break;

            case EasingType.QuartIn: opposite = EasingType.QuartOut; break;
            case EasingType.QuartOut: opposite = EasingType.QuartIn; break;
            case EasingType.QuartInOut: opposite = EasingType.QuartInOut; break;

            case EasingType.QuintIn: opposite = EasingType.QuintOut; break;
            case EasingType.QuintOut: opposite = EasingType.QuintIn; break;
            case EasingType.QuintInOut: opposite = EasingType.QuintInOut; break;

            case EasingType.ExpoIn: opposite = EasingType.ExpoOut; break;
            case EasingType.ExpoOut: opposite = EasingType.ExpoIn; break;
            case EasingType.ExpoInOut: opposite = EasingType.ExpoInOut; break;

            case EasingType.ExtremeIn: opposite = EasingType.ExtremeOut; break;
            case EasingType.ExtremeOut: opposite = EasingType.ExtremeIn; break;
            case EasingType.ExtremeInOut: opposite = EasingType.ExtremeInOut; break;

            case EasingType.ElasticIn: opposite = EasingType.ElasticOut; break;
            case EasingType.ElasticOut: opposite = EasingType.ElasticIn; break;
            case EasingType.ElasticInOut: opposite = EasingType.ElasticInOut; break;

            case EasingType.ElasticSoftIn: opposite = EasingType.ElasticSoftOut; break;
            case EasingType.ElasticSoftOut: opposite = EasingType.ElasticSoftIn; break;
            case EasingType.ElasticSoftInOut: opposite = EasingType.ElasticSoftInOut; break;

            case EasingType.BackIn: opposite = EasingType.BackOut; break;
            case EasingType.BackOut: opposite = EasingType.BackIn; break;
            case EasingType.BackInOut: opposite = EasingType.BackInOut; break;

            case EasingType.BounceIn: opposite = EasingType.BounceOut; break;
            case EasingType.BounceOut: opposite = EasingType.BounceIn; break;
            case EasingType.BounceInOut: opposite = EasingType.BounceInOut; break;
        }

        return opposite;
    }

    public static float SineIn(float t) { return 1f - MathF.Cos(t * MathF.PI * 0.5f); }
    public static float SineOut(float t) { return MathF.Sin(t * (MathF.PI * 0.5f)); }
    public static float SineInOut(float t) { return -0.5f * (MathF.Cos(MathF.PI * t) - 1f); }

    public static float QuadIn(float t) { return t * t; }
    public static float QuadOut(float t) { return t * (2f - t); }
    public static float QuadInOut(float t) { return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t; }

    public static float CubicIn(float t) { return t * t * t; }
    public static float CubicOut(float t) { var t1 = t - 1f; return t1 * t1 * t1 + 1f; }
    public static float CubicInOut(float t) { return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f; }

    public static float QuartIn(float t) { return t * t * t * t; }
    public static float QuartOut(float t) { var t1 = t - 1f; return 1f - t1 * t1 * t1 * t1; }
    public static float QuartInOut(float t) { return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f; }

    public static float QuintIn(float t) { return t * t * t * t * t; }
    public static float QuintOut(float t) { var t1 = t - 1f; return 1f + t1 * t1 * t1 * t1 * t1; }
    public static float QuintInOut(float t) { var t1 = t - 1f; return t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * t1 * t1 * t1 * t1 * t1; }

    public static float ExpoIn(float t) { return MathF.Pow(2f, 10f * (t - 1f)); }
    public static float ExpoOut(float t) { return 1f - MathF.Pow(2f, -10f * t); }
    public static float ExpoInOut(float t) { return t < 0.5f ? ExpoIn(t * 2f) * 0.5f : 1f - ExpoIn(2f - t * 2f) * 0.5f; }

    public static float ExtremeIn(float t) { return MathF.Pow(10f, 10f * (t - 1f)); }
    public static float ExtremeOut(float t) { return 1f - MathF.Pow(10f, -10f * t); }
    public static float ExtremeInOut(float t) { return t < 0.5f ? ExtremeIn(t * 2f) * 0.5f : 1f - ExtremeIn(2f - t * 2f) * 0.5f; }

    public static float ElasticIn(float t) { return 1f - ElasticOut(1f - t); }
    public static float ElasticOut(float t) { var p = 0.3f; return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * (float)Math.PI) / p) + 1f; }
    public static float ElasticInOut(float t) { return t < 0.5f ? ElasticIn(t * 2f) * 0.5f : 1f - ElasticIn(2f - t * 2f) * 0.5f; }

    public static float ElasticSoftIn(float t) { return 1f - ElasticSoftOut(1f - t); }
    public static float ElasticSoftOut(float t) { var p = 0.5f; return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * (float)Math.PI) / p) + 1f; }
    public static float ElasticSoftInOut(float t) { return t < 0.5f ? ElasticSoftIn(t * 2f) * 0.5f : 1f - ElasticSoftIn(2f - t * 2f) * 0.5f; }

    public static float BackIn(float t) { var p = 1f; return t * t * ((p + 1f) * t - p); }
    public static float BackOut(float t) { var p = 1f; var scaledTime = t / 1f - 1f; return scaledTime * scaledTime * ((p + 1f) * scaledTime + p) + 1f; }
    public static float BackInOut(float t)
    {
        var p = 1f;
        var scaledTime = t * 2f;
        var scaledTime2 = scaledTime - 2f;
        var s = p * 1.525f;

        if (scaledTime < 1f) return 0.5f * scaledTime * scaledTime * ((s + 1f) * scaledTime - s);
        else return 0.5f * (scaledTime2 * scaledTime2 * ((s + 1f) * scaledTime2 + s) + 2f);
    }

    public static float BounceIn(float t) { return 1f - BounceOut(1f - t); }
    public static float BounceOut(float t)
    {
        var scaledTime = t / 1f;

        if (scaledTime < 1 / 2.75f)
        {
            return 7.5625f * scaledTime * scaledTime;
        }
        else if (scaledTime < 2 / 2.75)
        {
            var scaledTime2 = scaledTime - 1.5f / 2.75f;
            return 7.5625f * scaledTime2 * scaledTime2 + 0.75f;
        }
        else if (scaledTime < 2.5 / 2.75)
        {
            var scaledTime2 = scaledTime - 2.25f / 2.75f;
            return 7.5625f * scaledTime2 * scaledTime2 + 0.9375f;
        }
        else
        {
            var scaledTime2 = scaledTime - 2.625f / 2.75f;
            return 7.5625f * scaledTime2 * scaledTime2 + 0.984375f;
        }
    }
    public static float BounceInOut(float t)
    {
        if (t < 0.5) return BounceIn(t * 2f) * 0.5f;
        else return BounceOut(t * 2f - 1f) * 0.5f + 0.5f;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };

    public static string MakeConfused(string input, int seed)
    {
        char[] characters = input.ToCharArray();

        int mixedCounter = 1;

        Random rng = new Random(seed);
        for (int i = 0; i < characters.Length - 1; i++)
        {
            if (rng.Next(0, mixedCounter) != 0)
                continue;

            int randomIndex = rng.Next(i, characters.Length);
            char temp = characters[i];
            characters[i] = characters[randomIndex];
            characters[randomIndex] = temp;
            mixedCounter += mixedCounter + 1;
        }

        return new string(characters);
    }

    public static string GetRandomIcon(string i0, string i1) { int rand = Game.Random.Int(0, 1); switch(rand) { case 0: default: return i0; case 1: return i1; } }
    public static string GetRandomIcon(string i0, string i1, string i2) { int rand = Game.Random.Int(0, 2); switch (rand) { case 0: default: return i0; case 1: return i1; case 2: return i2; } }
    public static string GetRandomIcon(string i0, string i1, string i2, string i3) { int rand = Game.Random.Int(0, 3); switch (rand) { case 0: default: return i0; case 1: return i1; case 2: return i2; case 3: return i3; } }
    public static string GetRandomIcon(string i0, string i1, string i2, string i3, string i4) { int rand = Game.Random.Int(0, 4); switch (rand) { case 0: default: return i0; case 1: return i1; case 2: return i2; case 3: return i3; case 4: return i4; } }
    public static string GetRandomIcon(string i0, string i1, string i2, string i3, string i4, string i5) { int rand = Game.Random.Int(0, 5); switch (rand) { case 0: default: return i0; case 1: return i1; case 2: return i2; case 3: return i3; case 4: return i4; case 5: return i5; } }
    public static string GetRandomIcon(string i0, string i1, string i2, string i3, string i4, string i5, string i6) { int rand = Game.Random.Int(0, 6); switch (rand) { case 0: default: return i0; case 1: return i1; case 2: return i2; case 3: return i3; case 4: return i4; case 5: return i5; case 6: return i6; } }
    public static string GetRandomIcon(string i0, string i1, string i2, string i3, string i4, string i5, string i6, string i7) { int rand = Game.Random.Int(0, 7); switch (rand) { case 0: default: return i0; case 1: return i1; case 2: return i2; case 3: return i3; case 4: return i4; case 5: return i5; case 6: return i6; case 7: return i7; } }


    private class NodeInfo<T>
    {
        private const int MaxPoolSize = 8192;

        private static List<NodeInfo<T>> _sPool;

        internal static NodeInfo<T> Create(T node, NodeInfo<T> prev = null, float costAdd = 0f)
        {
            NodeInfo<T> nodeInfo;

            if (_sPool == null || _sPool.Count == 0)
            {
                nodeInfo = new NodeInfo<T>();
            }
            else
            {
                nodeInfo = _sPool[_sPool.Count - 1];
                _sPool.RemoveAt(_sPool.Count - 1);
            }

            nodeInfo.Node = node;
            nodeInfo.Prev = prev;

            if (prev == null)
            {
                nodeInfo.Depth = 0;
                nodeInfo.Cost = 0f;
            }
            else
            {
                nodeInfo.Depth = prev.Depth + 1;
                nodeInfo.Cost = prev.Cost + costAdd;
            }

            return nodeInfo;
        }

        internal static void Pool(NodeInfo<T> nodeInfo)
        {
            if (_sPool == null) _sPool = new List<NodeInfo<T>>(MaxPoolSize);
            if (_sPool.Count >= MaxPoolSize) return;

            _sPool.Add(nodeInfo);
        }

        private float _heuristic;

        public T Node { get; private set; }
        public NodeInfo<T> Prev { get; private set; }
        public int Depth { get; private set; }
        public float Cost { get; private set; }
        public float Total { get; private set; }

        public float Heuristic
        {
            get { return _heuristic; }
            set
            {
                _heuristic = value;
                Total = Cost + value;
            }
        }
    }

    /// <summary>
    /// Convenience method to produce a graph connection for use when calling AStar().
    /// </summary>
    /// <typeparam name="T">Graph node type.</typeparam>
    /// <param name="dest">Destination node of the connection.</param>
    /// <param name="cost">Cost of taking the connection.</param>
    public static AStarEdge<T> Edge<T>(T dest, float cost)
    {
        return new AStarEdge<T>(dest, cost);
    }

    private static class AStarWrapper<T>
        where T : IEquatable<T>
    {
        public static NodeInfo<T> FirstMatchOrDefault(List<NodeInfo<T>> list, T toCompare)
        {
            var count = list.Count;

            for (var i = count - 1; i >= 0; --i)
            {
                var item = list[i];
                if (item.Node.Equals(toCompare))
                    return item;
            }

            return null;
        }

        private static List<NodeInfo<T>> _sOpen;
        private static List<NodeInfo<T>> _sClosed;

        public static bool AStar(T origin, T target, List<T> destList,
            Func<T, IEnumerable<AStarEdge<T>>> adjFunc, Func<T, T, float> heuristicFunc)
        {
            var open = _sOpen ?? (_sOpen = new List<NodeInfo<T>>());
            var clsd = _sClosed ?? (_sClosed = new List<NodeInfo<T>>());

            open.Clear();
            clsd.Clear();

            var first = NodeInfo<T>.Create(origin);
            first.Heuristic = heuristicFunc(origin, target);

            open.Add(first);

            try
            {
                while (open.Count > 0)
                {
                    NodeInfo<T> cur = null;
                    foreach (var node in open)
                    {
                        if (cur == null || node.Total < cur.Total) cur = node;
                    }

                    if (cur.Node.Equals(target))
                    {
                        for (var i = cur.Depth; i >= 0; --i)
                        {
                            destList.Add(cur.Node);
                            cur = cur.Prev;
                        }
                        destList.Reverse();
                        return true;
                    }

                    open.Remove(cur);
                    clsd.Add(cur);

                    foreach (var adj in adjFunc(cur.Node))
                    {
                        var node = NodeInfo<T>.Create(adj.Dest, cur, adj.Cost);
                        var existing = FirstMatchOrDefault(clsd, adj.Dest);

                        if (existing != null)
                        {
                            if (existing.Cost <= node.Cost) continue;

                            clsd.Remove(existing);
                            node.Heuristic = existing.Heuristic;

                            NodeInfo<T>.Pool(existing);
                        }

                        existing = FirstMatchOrDefault(open, adj.Dest);

                        if (existing != null)
                        {
                            if (existing.Cost <= node.Cost) continue;

                            open.Remove(existing);
                            node.Heuristic = existing.Heuristic;

                            NodeInfo<T>.Pool(existing);
                        }
                        else
                        {
                            node.Heuristic = heuristicFunc(node.Node, target);
                        }

                        open.Add(node);
                    }
                }
                return false;

            }
            finally
            {
                foreach (var nodeInfo in open)
                {
                    NodeInfo<T>.Pool(nodeInfo);
                }

                foreach (var nodeInfo in clsd)
                {
                    NodeInfo<T>.Pool(nodeInfo);
                }
            }
        }
    }

    /// <summary>
    /// An implementation of the AStar path finding algorithm.
    /// </summary>
    /// <typeparam name="T">Graph node type.</typeparam>
    /// <param name="origin">Node to start path finding from.</param>
    /// <param name="target">The goal node to reach.</param>
    /// <param name="adjFunc">Function returning the neighbouring connections for a node.</param>
    /// <param name="heuristicFunc">Function returning the estimated cost of travelling between two nodes.</param>
    /// <returns>A sequence of nodes representing a path if one is found, otherwise an empty array.</returns>
    public static bool AStar<T>(T origin, T target, List<T> destPath,
        Func<T, IEnumerable<AStarEdge<T>>> adjFunc, Func<T, T, float> heuristicFunc)
        where T : IEquatable<T>
    {
        return AStarWrapper<T>.AStar(origin, target, destPath, adjFunc, heuristicFunc);
    }
}
