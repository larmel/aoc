using System.Diagnostics;
using System.Text;

var jets = Console.ReadLine()!.ToCharArray();

Tile[] tiles =
{
    new(0, new List<Point>{new(0, 0), new(1, 0), new(2, 0), new(3, 0)}),
    new(1, new List<Point>{new(0, 1), new(1, 0), new(1, 1), new(1, 2), new(2, 1)}),
    new(2, new List<Point>{new(0, 0), new(1, 0), new(2, 0), new(2, 1), new(2, 2)}),
    new(3, new List<Point>{new(0, 0), new(0, 1), new(0, 2), new(0, 3)}),
    new(4, new List<Point>{new(0, 0), new(0, 1), new(1, 0), new(1, 1)})
};

var targetTileCount = args.Length != 0 && long.TryParse(args[0], out var n) ? n : 1000000000000L;

Console.WriteLine($"Simulating {targetTileCount} rocks");

Simulation simulation = new(jets);
var sw = Stopwatch.StartNew();
for (var i = 0L; i < targetTileCount; )
{
    var tileIndex = i % tiles.Length;
    var tile = tiles[tileIndex].Copy();
    tile.Translate(new (2, simulation.MeshHeight + 3));
    i += simulation.PlaceTile(tile, i, targetTileCount);

    // Console.WriteLine();
    // Console.WriteLine($"Height after {i + 1} tiles is {simulation.Height}");
    // Console.WriteLine(simulation);
}

Console.WriteLine($"Height: {simulation.TotalHeight} ({sw.Elapsed})");

internal sealed class Simulation
{
    private readonly char[] _jets;

    private HashSet<Point> _mesh;
    private long _step;
    private long _skipHeight;

    private sealed record State(HashSet<Point> Mesh, long NextJet, int TileId)
    {
        public bool Equals(State? other)
            => other != null && Mesh.SetEquals(other.Mesh) && NextJet == other.NextJet && TileId == other.TileId;

        public override int GetHashCode() => HashCode.Combine(Mesh.Count, NextJet, TileId);
    }

    private readonly record struct Result(long Step, long TileCount, long Height);

    private readonly Dictionary<State, Result> _states = new();

    public Simulation(char[] jets)
    {
        _jets = jets;
        _mesh = Enumerable.Range(0, 7).Select(x => new Point(x, -1)).ToHashSet();
    }

    public int MeshHeight => _mesh.Max(p => p.Y) + 1;

    public long TotalHeight => _skipHeight + MeshHeight;

    public long PlaceTile(Tile tile, long currentTileCount, long targetTileCount)
    {
        do
        {
            var direction = _jets[_step % _jets.Length];
            tile.Blow(direction, _mesh);
            _step++;

            tile.Translate(new (0, -1));
        } while (!tile.CollidesWith(_mesh));

        currentTileCount += 1;
        tile.Translate(new (0, 1));
        tile.Points.ForEach(p => _mesh.Add(p));

        // Look for completed lines.
        foreach (var line in tile.Points.Select(p => p.Y).Distinct().OrderByDescending(y => y))
        {
            if (line == 0 || Enumerable.Range(0, 7).Any(x => !_mesh.Contains(new(x, line))))
                continue;

            // Push the whole mesh down towards new baseline.
            _skipHeight += line;
            _mesh.RemoveWhere(p => p.Y < line);
            _mesh = _mesh.Select(p => p with {Y = p.Y - line}).ToHashSet();

            // See if we have encountered this situation before, then we can skip towards the end.
            State state = new(new HashSet<Point>(_mesh), _step % _jets.Length, tile.Id);
            if (_states.TryGetValue(state, out var prev))
            {
                var cycleTime = _step - prev.Step;
                var cycleTiles = currentTileCount - prev.TileCount;
                var cycleHeight = TotalHeight - prev.Height;

                var remainingTiles = targetTileCount - currentTileCount;
                var skipCount = remainingTiles / cycleTiles;
                var skipTileCount = skipCount * cycleTiles;

                Console.WriteLine($"Found equivalent set at {currentTileCount}, previous {prev}");

                _step += skipCount * cycleTime;
                _skipHeight += skipCount * cycleHeight;
                return 1 + skipTileCount;
            }

            _states.Add(state, new (_step, currentTileCount, TotalHeight));
        }

        return 1;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var y = MeshHeight + 1; y >= 0; --y)
        {
            sb.Append('|');
            for (var x = 0; x < 7; ++x)
            {
                sb.Append(_mesh.Contains(new(x, y)) ? '#' : '.');
            }

            sb.AppendLine("|");
        }

        sb.AppendLine("+-------+");
        return sb.ToString();
    }
}

internal sealed record Tile(int Id, List<Point> Points)
{
    public Tile Copy() => new(Id, Points.ToList());

    public void Translate(Point offset)
    {
        for (var i = 0; i < Points.Count; ++i)
        {
            var p = Points[i];
            Points[i] = new(p.X + offset.X, p.Y + offset.Y);
        }
    }

    public bool Blow(char direction, HashSet<Point> mesh)
    {
        switch (direction)
        {
            case '<' when Points.Min(p => p.X) > 0:
                Translate(new (-1, 0));
                if (!CollidesWith(mesh)) return true;
                Translate(new (1, 0));
                return false;
            case '>' when Points.Max(p => p.X) < 6:
                Translate(new (1, 0));
                if (!CollidesWith(mesh)) return true;
                Translate(new (-1, 0));
                return true;
        }

        return false;
    }

    public bool CollidesWith(HashSet<Point> mesh) => Points.Any(mesh.Contains);
}

internal record struct Point(int X, int Y);
