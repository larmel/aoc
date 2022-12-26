using System.Text;

HashSet<Point> elves = new();
for (var i = 0; Console.ReadLine() is { } line; ++i)
{
    for (var j = 0; j < line.Length; ++j)
    {
        if (line[j] == '#')
        {
            elves.Add(new(j, i));
        }
    }
}

Dictionary<Point, List<Point>> intention = new();
List<Point[]> offsets = new()
{
    new Point[] {new(-1, -1), new(0, -1), new(1, -1)},
    new Point[] {new(-1, 1), new(0, 1), new(1, 1)},
    new Point[] {new(-1, -1), new(-1, 0), new(-1, 1)},
    new Point[] {new(1, -1), new(1, 0), new(1, 1)}
};

for (var round = 0; ; ++round)
{
    if (round == 10)
    {
        Console.WriteLine($"Empty tiles after round 10: {EmptyTiles()}");
    }

    intention.Clear();
    foreach (var elf in elves)
    {
        if (offsets.SelectMany(a => a.Select(p => p)).All(p => !elves.Contains(elf + p)))
        {
            continue;
        }

        for (var dir = 0; dir < 4; ++dir)
        {
            var searchPoints = offsets[dir];
            var free = true;
            for (var i = 0; i < 3; ++i)
            {
                var offset = searchPoints[i];
                var target = elf + offset;
                if (elves.Contains(target))
                {
                    free = false;
                }
            }

            if (free)
            {
                var offset = searchPoints[1];
                var target = elf + offset;
                if (!intention.TryGetValue(target, out var list))
                {
                    list = new List<Point>();
                    intention[target] = list;
                }

                list.Add(elf);
                break;
            }
        }
    }

    var first = offsets[0];
    offsets.RemoveAt(0);
    offsets.Add(first);
    var moved = false;
    foreach (var (target, list) in intention)
    {
        if (list.Count == 1)
        {
            elves.Remove(list[0]);
            elves.Add(target);
            moved = true;
        }
    }

    if (!moved)
    {
        Console.WriteLine($"No elf moved in round {round + 1}");
        break;
    }
}

int EmptyTiles()
{
    int xMin = elves.Min(p => p.X),
        xMax = elves.Max(p => p.X),
        yMin = elves.Min(p => p.Y),
        yMax = elves.Max(p => p.Y);

    var sb = new StringBuilder();
    for (var y = yMin; y <= yMax; ++y)
    {
        for (var x = xMin; x <= xMax; ++x)
        {
            Point p = new(x, y);
            sb.Append(elves.Contains(p) ? '#' : '.');
        }

        sb.AppendLine();
    }

    var area = (xMax - xMin + 1) * (yMax - yMin + 1);
    Console.WriteLine($"{elves.Count} elves enclosed in area {area}");
    Console.WriteLine(sb.ToString());

    return area - elves.Count;
}

internal sealed record Point(int X, int Y)
{
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
}
