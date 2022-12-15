List<Sensor> sensors = new();
HashSet<Point> beacons = new();
while (Console.ReadLine()?.Split()
       is ["Sensor", "at", var sxs, var sys, "closest", "beacon", "is", "at", var bxs, var bys])
{
    Point
        sensor = new (int.Parse(sxs[2..^1]), int.Parse(sys[2..^1])),
        beacon = new (int.Parse(bxs[2..^1]), int.Parse(bys[2..]));

    var radius = Math.Abs(sensor.X - beacon.X) + Math.Abs(sensor.Y - beacon.Y);
    sensors.Add(new Sensor(sensor, radius));
    beacons.Add(beacon);
}

int BeaconFreeSquaresInRow(int y)
{
    HashSet<Point> covered = new();
    foreach (var sensor in sensors)
    {
        var distance = Math.Abs(sensor.Position.Y - y);
        if (distance > sensor.Radius)
        {
            continue;
        }

        for (var i = 0; i <= sensor.Radius - distance; ++i)
        {
            covered.Add(new (sensor.Position.X - i, y));
            covered.Add(new (sensor.Position.X + i, y));
        }
    }

    covered.ExceptWith(beacons);
    return covered.Count;
}

Console.WriteLine($"Beacon free squares in row: {BeaconFreeSquaresInRow(2000000)}");

Point FindUndetectedPoint(int bound)
{
    bool Undetected(Sensor s, Point offset, out Point border)
    {
        border = new(s.Position.X + offset.X, s.Position.Y + offset.Y);
        if (border.X < 0 || border.Y < 0 || border.X > bound || border.Y > bound)
        {
            return false;
        }

        for (var i = 0; i < sensors.Count; ++i)
        {
            if (sensors[i].Position == s.Position) continue;
            if (sensors[i].Covers(border)) return false;
        }

        return true;
    }

    foreach (var sensor in sensors)
    {
        for (var i = 0; i <= sensor.Radius + 1; ++i)
        {
            Point
                a = new(-sensor.Radius - 1 + i, -i),
                b = new(-sensor.Radius - 1 + i, i),
                c = new(sensor.Radius + 1 - i, -i),
                d = new(sensor.Radius + 1 - i, i);

            if (Undetected(sensor, a, out var p1)) return p1;
            if (Undetected(sensor, b, out var p2)) return p2;
            if (Undetected(sensor, c, out var p3)) return p3;
            if (Undetected(sensor, d, out var p4)) return p4;
        }
    }

    throw new Exception("No undetected points!");
}

var u = FindUndetectedPoint(4000000);
Console.WriteLine($"Undetected point {u}, frequency: {u.X * 4000000L + u.Y}");

internal readonly record struct Point(int X, int Y)
{
    public override string ToString() => $"({X}, {Y})";
}

internal readonly record struct Sensor(Point Position, int Radius)
{
    public bool Covers(Point p)
    {
        int dx = Math.Abs(Position.X - p.X),
            dy = Math.Abs(Position.Y - p.Y);

        return dx + dy <= Radius;
    }
}
