List<Number> list = new();
while (Console.ReadLine() is { } s)
{
    var n = s[0] == '-' ? -long.Parse(s[1..]) : long.Parse(s);
    list.Add(new (list.Count, n));
}

Ring ring = new(list);
foreach (var number in list)
{
    ring.Mix(number);
}

Console.WriteLine(ring.Coordinates());

var keys = list.Select(n => n with {N = n.N * 811589153}).ToList();
ring = new(keys);
for (var i = 0; i < 10; ++i)
{
    foreach (var number in keys)
    {
        ring.Mix(number);
    }
}

Console.WriteLine(ring.Coordinates());

internal sealed class Ring
{
    private readonly List<Number> _list;

    public Ring(IEnumerable<Number> list) => _list = new List<Number>(list);

    public void Mix(Number n)
    {
        var i = _list.FindIndex(x => x == n);
        var delta = n.N % (_list.Count - 1);
        var direction = Math.Sign(delta);
        for (var count = Math.Abs(delta); count > 0; --count)
        {
            var j = i + direction;
            if (j == -1)
            {
                j = _list.Count - 1;
            }
            else if (j == _list.Count)
            {
                j = 0;
            }

            (_list[i], _list[j]) = (_list[j], _list[i]);
            i = j;
        }
    }

    public long Coordinates()
    {
        var z = _list.FindIndex(n => n.N == 0);

        int i = (z + 1000) % _list.Count,
            j = (z + 2000) % _list.Count,
            k = (z + 3000) % _list.Count;

        return _list[i].N + _list[j].N + _list[k].N;
    }
}

internal readonly record struct Number(int Id, long N);
