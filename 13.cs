List<Node> packets = new();
while (Console.ReadLine() is { } line)
{
    if (line.Length > 1)
    {
        packets.Add(ParseLine(line));
    }
}

var sum = 0;
for (var i = 0; i < packets.Count / 2; ++i)
{
    var c = NodeComparer.Instance.Compare(packets[i * 2], packets[i * 2 + 1]);
    sum += c <= 0 ? i + 1 : 0;
}

Console.WriteLine($"Sum of indices of correctly ordered pairs: {sum}");

var dividers = new[]
{
    ParseLine("[[2]]"),
    ParseLine("[[6]]")
};

packets.AddRange(dividers);
packets.Sort(NodeComparer.Instance);

var key = (packets.IndexOf(dividers[0]) + 1) * (packets.IndexOf(dividers[1]) + 1);

Console.WriteLine($"Decoder key: {key}");

Node ParseLine(string line)
{
    var i = 0;
    return Parse(line, ref i);
}

Node Parse(string line, ref int i)
{
    switch (line[i])
    {
        case '[':
            i++;
            List<Node> elements = new();
            while (line[i] != ']')
            {
                elements.Add(Parse(line, ref i));
                if (line[i] == ',')
                    i++;
            }

            i++;
            return new List(elements);
        case var c:
            int d;
            for (d = 0; char.IsDigit(line[i]); i++)
            {
                d = d * 10 + (c - '0');
            }

            return new Number(d);
    }
}

internal class NodeComparer : IComparer<Node>
{
    public static NodeComparer Instance { get; } = new();

    private int CompareLists(List a, List b)
    {
        var n = Math.Min(a.Values.Count, b.Values.Count);
        for (var i = 0; i < n; ++i)
        {
            var c = Compare(a.Values[i], b.Values[i]);
            if (c != 0)
                return c;
        }

        return a.Values.Count.CompareTo(b.Values.Count);
    }

    public int Compare(Node? x, Node? y) => (x, y) switch
    {
        (Number a, Number b) => a.Value.CompareTo(b.Value),
        (List a, List b) => CompareLists(a, b),
        (Number a, List b) => Compare(new List(new List<Node> {a}), b),
        (List a, Number b) => Compare(a, new List(new List<Node> {b})),
        _ => throw new NotImplementedException()
    };
}

internal abstract record Node;

internal sealed record Number(int Value) : Node
{
    public override string ToString() => Value.ToString();
}

internal sealed record List(List<Node> Values) : Node
{
    public bool Equals(List? other)
        => other != null && other.Values.Count == Values.Count &&
           Values.Zip(other.Values).All(p => p.First.Equals(p.Second));

    public override int GetHashCode() => Values.Count;

    public override string ToString() => $"[{string.Join(",", Values)}]";
}
