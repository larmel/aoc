Point start = default!;
List<Point> ground = new();
HashSet<Point> remaining = new();
List<Point> q = new();
Dictionary<Point, int> distance = new();
Dictionary<Point, int> height = new();
for (int row = 0; Console.ReadLine() is { } line; ++row)
{
    for (int col = 0; col < line.Length; ++col)
    {
        Point pos = new(row, col);
        int d = int.MaxValue;

        switch (line[col])
        {
            case 'S':
                height.Add(pos, 'a');
                ground.Add(pos);
                start = pos;
                break;
            case 'E':
                height.Add(pos, 'z');
                d = 0;
                q.Add(pos);
                break;
            case var c:
                if (c == 'a')
                    ground.Add(pos);
                height.Add(pos, c);
                break;
        }

        remaining.Add(pos);
        distance.Add(pos, d);
    }
}

bool Edge(Point a, Point b) => height[a] <= height[b] + 1;

void Relax(Point u, Point v)
{
    if (!remaining.Contains(v) || !Edge(u, v)) return;
    int d = distance[u] + 1;
    if (distance[v] > d)
    {
        distance[v] = d;
        q.Add(v);
    }
}

while (q.Count != 0)
{
    Point p = q.MinBy(x => distance[x]);
    q.Remove(p);
    remaining.Remove(p);
    Relax(p, p with {Col = p.Col + 1});
    Relax(p, p with {Col = p.Col - 1});
    Relax(p, p with {Row = p.Row + 1});
    Relax(p, p with {Row = p.Row - 1});
}

Console.WriteLine($"Distance from start: {distance[start]}");
Console.WriteLine($"Distance from ground: {ground.Where(c => !remaining.Contains(c)).Min(n => distance[n])}");

internal readonly record struct Point(int Row, int Col);
