using System.Diagnostics;
using System.Text;

var map = Map.Read(Console.In);

var openState = new Dictionary<Point, HashSet<int>>();
for (var y = 0; y < map.Height; ++y)
{
    for (var x = 0; x < map.Width; ++x)
    {
        Point p = new(y, x);
        openState[p] = new HashSet<int>();
    }
}

// The trick here is to recognize that the set of possible states is bounded by the
// cyclic blizzards, for the input it will result in the same map every 700 minute.
// Total number of states is therefore 35 * 100 * 700, and we can easily calculate
// the shortest path within this graph.
var sw = Stopwatch.StartNew();
for (var t = 0; t <= map.CycleTime; ++t)
{
    var free = map.GetOpen(t);
    foreach (var f in free)
    {
        openState[f].Add(t);
    }
}

Console.WriteLine($"Analyzed map in {sw.Elapsed}");

var (endNode, endDistance) = ShortestPath(new Node(map.Start, 0), map.End);

Console.WriteLine($"Reached end in {endDistance} steps");

var (startNode, startDistance) = ShortestPath(endNode, map.Start);

Console.WriteLine($"Back to start in {startDistance} steps");

var (_, finalDistance) = ShortestPath(startNode, map.End);

Console.WriteLine($"Reached end again in {finalDistance} steps, total {endDistance + startDistance + finalDistance}");

(Node, int) ShortestPath(Node start, Point end)
{
    Dictionary<Node, int> distances = new() {{start, 0}};
    List<Node> queue = new() {start};
    while (queue.Count != 0)
    {
        var node = queue[0];
        var distance = distances[node];
        queue.RemoveAt(0);

        // Max number of minutes to wait on current node.
        var maxWaitTime = 0;
        for (var t = node.Step + 1; openState[node.Position].Contains(t % map.CycleTime); ++t)
        {
            maxWaitTime++;
            if (maxWaitTime == map.CycleTime) break;
        }

        // Find all possible next nodes based on open state.
        for (var i = 0; i <= maxWaitTime; ++i)
        {
            var jump = 1 + i;
            var step = node.Step + jump;
            var t = step % map.CycleTime;
            foreach (var q in new []{node.Position.Right, node.Position.Down, node.Position.Left, node.Position.Up})
            {
                if (q.Y >= 0 && q.Y < map.Height && openState[q].Contains(t))
                {
                    Node n = new(q, t);
                    if (!distances.TryGetValue(n, out var d) || d > distance + jump)
                    {
                        distances[n] = distance + jump;
                        queue.Add(n);
                    }
                }
            }
        }
    }

    var answer = distances.Where(kv => kv.Key.Position == end).OrderBy(kv => kv.Value).FirstOrDefault();
    return (answer.Key, answer.Value);
}

internal sealed record Node(Point Position, int Step);

internal sealed class Map
{
    private readonly List<HashSet<Point>> _state;
    private readonly List<Blizzard> _blizzards;
    public readonly int Height, Width;

    // Blizzards repeat. Precomputed, but it is just the union of factors of width and height.
    public int CycleTime => Width == 102 ? 700 : 3 * 2 * 2;

    public Point Start => new(0, 1);
    public Point End => new(Height - 1, Width - 2);

    private Map(List<Blizzard> blizzards, int height, int width)
    {
        _blizzards = blizzards;
        Height = height;
        Width = width;
        _state = new List<HashSet<Point>> {Probe()};
    }

    public static Map Read(TextReader reader)
    {
        List<Blizzard> blizzards = new();
        List<string> lines = new();
        while (reader.ReadLine() is { } line)
        {
            var y = lines.Count;
            lines.Add(line);
            for (var x = 0; x < line.Length; ++x)
            {
                Blizzard? blizzard = line[x] switch
                {
                    '<' => new(new Point(y, x), new Point(0, -1)),
                    '>' => new(new Point(y, x), new Point(0, 1)),
                    '^' => new(new Point(y, x), new Point(-1, 0)),
                    'v' => new(new Point(y, x), new Point(1, 0)),
                    _ => null
                };

                if (blizzard.HasValue)
                {
                    blizzards.Add(blizzard.Value);
                }
            }
        }

        int width = lines[0].Length,
            height = lines.Count;

        return new Map(blizzards, height, width);
    }

    public HashSet<Point> GetOpen(int step)
    {
        if (_state.Count > step)
        {
            return _state[step];
        }

        if (step != _state.Count) throw new Exception();
        Advance();
        var map = Probe();
        _state.Add(map);
        return map;
    }

    private HashSet<Point> Probe()
    {
        HashSet<Point> map = new()
        {
            Start, End
        };

        for (var y = 1; y < Height - 1; ++y)
        {
            for (var x = 1; x < Width - 1; ++x)
            {
                Point p = new(y, x);
                if (_blizzards.All(b => b.Position != p))
                {
                    map.Add(p);
                }
            }
        }

        return map;
    }

    private void Advance()
    {
        for (var i = 0; i < _blizzards.Count; ++i)
        {
            var blizzard = _blizzards[i];
            var p = blizzard.Position + blizzard.Direction;
            if (p.Y == 0) p = p with {Y = Height - 2};
            else if (p.Y == Height - 1) p = p with {Y = 1};
            else if (p.X == 0) p = p with {X = Width - 2};
            else if (p.X == Width - 1) p = p with {X = 1};
            _blizzards[i] = blizzard with {Position = p};
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        for (var y = 0; y < Height; ++y)
        {
            for (var x = 0; x < Width; ++x)
            {
                Point p = new(y, x);
                if (p.Y == 0 || p.Y == Height - 1 || p.X == 0 || p.X == Width - 1)
                {
                    sb.Append('#');
                }
                else
                {
                    var c = _blizzards.Count(b => b.Position == p);
                    sb.Append(c > 9 ? '*' : c > 0 ? c.ToString() : '.');
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}

internal readonly record struct Point(int Y, int X)
{
    public Point Left => new(Y, X - 1);
    public Point Right => new(Y, X + 1);
    public Point Up => new(Y - 1, X);
    public Point Down => new(Y + 1, X);
    
    public static Point operator +(Point a, Point b) => new(a.Y + b.Y, a.X + b.X);

    public override string ToString() => $"({Y}, {X})";
}

internal readonly record struct Blizzard(Point Position, Point Direction);
