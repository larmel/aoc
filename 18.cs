int surface = 0;
HashSet<Point> mesh = new();
while (Console.ReadLine()?.Split(",") is [var x, var y, var z])
{
    Point p = new(int.Parse(x), int.Parse(y), int.Parse(z));

    surface += 6;
    surface -= p.Adjacent().Count(q => mesh.Contains(q)) * 2;
    mesh.Add(p);
}

Console.WriteLine($"Surface: {surface}");

Point
    min = new (mesh.Min(p => p.X), mesh.Min(p => p.Y), mesh.Min(p => p.Z)),
    max = new (mesh.Max(p => p.X), mesh.Max(p => p.Y), mesh.Max(p => p.Z));

HashSet<Point> done = new();
List<Point> queue = new() {min with {X = min.X - 1}};
int outerSurface = 0;
while (queue.Count != 0)
{
    Point p = queue[^1];
    queue.RemoveAt(queue.Count - 1);
    done.Add(p);

    bool InBox(Point d)  =>
        (d.X >= min.X - 1 && d.X <= max.X + 1)
        && (d.Y >= min.Y - 1 && d.Y <= max.Y + 1)
        && (d.Z >= min.Z - 1 && d.Z <= max.Z + 1);

    foreach (var q in p.Adjacent().Where(n => !done.Contains(n) && !queue.Contains(n) && InBox(n)))
    {
        if (mesh.Contains(q))
        {
            outerSurface++;
        }
        else
        {
            queue.Add(q);
        }
    }
}

Console.WriteLine($"Outer surface: {outerSurface}");

internal readonly record struct Point(int X, int Y, int Z)
{
    public IEnumerable<Point> Adjacent()
    {
        yield return new(X - 1, Y, Z);
        yield return new(X + 1, Y, Z);
        yield return new(X, Y - 1, Z);
        yield return new(X, Y + 1, Z);
        yield return new(X, Y, Z - 1);
        yield return new(X, Y, Z + 1);
    }
}
