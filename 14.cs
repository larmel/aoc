List<List<Coordinate>> paths = new();
while (Console.ReadLine() is { } line)
{
    paths.Add(line.Split(" -> ").Select(s =>
    {
        var p = s.Split(',');
        return new Coordinate(int.Parse(p[0]), int.Parse(p[1]));
    }).ToList());
}

Coordinate
    source = new(500, 0),
    min = source,
    max = source;

Dictionary<Coordinate, char> scan = new();
foreach (var path in paths)
{
    Coordinate Draw(Coordinate coordinate)
    {
        scan[coordinate] = '#';
        min = new Coordinate(X: Math.Min(min.X, coordinate.X), Y: Math.Min(min.Y, coordinate.Y));
        max = new Coordinate(X: Math.Max(max.X, coordinate.X), Y: Math.Max(max.Y, coordinate.Y));
        return coordinate;
    }
    
    for (var i = 0; i < path.Count; ++i)
    {
        var cursor = Draw(path[i]);
        if (i < path.Count - 1)
        {
            var next = path[i + 1];
            while (cursor.X != next.X)
            {
                cursor = Draw(cursor with {X = cursor.X + Math.Sign(next.X - cursor.X)});
            }
            
            while (cursor.Y != next.Y)
            {
                cursor = Draw(cursor with {Y = cursor.Y + Math.Sign(next.Y - cursor.Y)});
            }
        }
    }
}

for (int w = max.Y + 2, x = source.X - w; x <= source.X + w; ++x)
{
    scan[new(x, max.Y + 2)] = '#';
}

int Simulate(bool floor)
{
    for (var n = 0;; ++n)
    {
        var sand = source;
        while (true)
        {
            if (!floor && sand.Y > max.Y) return n;

            var fall = sand with {Y = sand.Y + 1};
            if (!scan.ContainsKey(fall))
            {
                sand = fall;
                continue;
            }

            fall = fall with {X = fall.X - 1};
            if (!scan.ContainsKey(fall))
            {
                sand = fall;
                continue;
            }

            fall = fall with {X = fall.X + 2};
            if (!scan.ContainsKey(fall))
            {
                sand = fall;
                continue;
            }

            break;
        }

        if (scan.ContainsKey(sand)) return n;
        scan[sand] = 'o';
    }
}

var initial = Simulate(false);
var floor = Simulate(true);

Console.WriteLine($"Units of sand: {initial}");
Console.WriteLine($"With floor: {initial + floor}");

internal readonly record struct Coordinate(int X, int Y);
