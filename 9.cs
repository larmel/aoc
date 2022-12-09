List<(string, int)> steps = new ();
while (Console.ReadLine()?.Split() is [var d, var c] && int.TryParse(c, out int count))
{
    steps.Add(new (d, count));
}

int Simulate(int length)
{
    HashSet<Point> trail = new();
    Point[] rope = new Point[length];
    for (int i = 0; i < length; ++i)
    {
        rope[i] = new Point(0, 0);
    }

    foreach ((string direction, int count) in steps)
    {
        for (int c = 0; c < count; ++c)
        {
            rope[0] = direction switch
            {
                "R" => rope[0] with {X = rope[0].X + 1},
                "L" => rope[0] with {X = rope[0].X - 1},
                "U" => rope[0] with {Y = rope[0].Y + 1},
                _ => rope[0] with {Y = rope[0].Y - 1},
            };

            for (int i = 1; i < rope.Length; ++i)
            {
                rope[i] = rope[i].Follow(rope[i - 1]);
            }

            trail.Add(rope[^1]);
        }
    }

    return trail.Count;
}

new List<int> {2, 10}.ForEach(n => Console.WriteLine($"Tail trail {n}: {Simulate(n)}"));

internal readonly record struct Point(int X, int Y)
{
    public Point Follow(Point head)
    {
        int dx = head.X - X, dy = head.Y - Y;
        if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) return this;
        return new Point(X + Math.Sign(dx), Y + Math.Sign(dy));
    }
}
