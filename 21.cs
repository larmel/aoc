List<long?> values = new();
List<Operation?> operations = new();
int root = 0, humn = 0;
{
    Dictionary<string, int> keys = new();
    int ResolveKey(string name)
    {
        if (keys.TryGetValue(name, out var key)) return key;
        key = keys.Count;
        keys.Add(name, key);
        values.Add(null);
        operations.Add(null);
        return key;
    }

    while (Console.ReadLine()?.Split() is { } line)
    {
        var name = line[0][..^1];
        var key = ResolveKey(name);
        root = name == "root" ? key : root;
        humn = name == "humn" ? key : humn;
        switch (line)
        {
            case [_, var c]:
                values[key] = long.Parse(c);
                break;
            case [_, var ls, var op, var rs]:
            {
                var l = ResolveKey(ls);
                var r = ResolveKey(rs);
                operations[key] = new Operation(l, r, op[0]);
                break;
            }
        }
    }
}

var (rootLeft, rootRight, _) = operations[root]!;

var input = values.ToList();
Shout(root, input);
Console.WriteLine($"Root: {input[root]} (left: {input[rootLeft]}, right: {input[rootRight]})");

// Very ad-hoc process to find the solution for part 2, could be generalized more.
Console.WriteLine($"{values.Count(s => !s.HasValue)} unsolved initially");
var originals = values.ToList();
values[humn] = null;
operations[root] = operations[root]! with {Op = '='};

Shout(root, values);

Console.WriteLine($"{values.Count(s => !s.HasValue)} unsolved with humn unknown");

{
    HashSet<int> rootLeftDeps = new(), rootRightDeps = new();
    DiscoverDependencies(rootLeft, rootLeftDeps);
    DiscoverDependencies(rootRight, rootRightDeps);

    long TryEvaluate(long x)
    {
        foreach (var dep in rootLeftDeps)
        {
            values[dep] = null;
        }

        values[humn] = x;
        return Shout(rootLeft, values)!.Value;
    }

    // Turns out right side is fully evaluated.
    long target = values[rootRight]!.Value;
    long zero = TryEvaluate(0),
        positive = TryEvaluate(10);
    
    Console.WriteLine($"Results around zero: {zero}, {positive}");
    var inputDirection = zero < positive ? 1 : -1;
    int searchDirection = zero > target ? -inputDirection : inputDirection;

    Console.WriteLine($"Input search direction: {searchDirection}");
    long attempt = 0, result;
    do
    {
        result = TryEvaluate(attempt);
        attempt += 1000000;
    }
    while (Math.Sign(result - target) == searchDirection) ;

    searchDirection *= -1;
    while (result != target)
    {
        attempt += searchDirection;
        result = TryEvaluate(attempt);
    }

    // There can be multiple solutions, find the lowest one.
    for (var i = 0; i < 1000; ++i)
    {
        attempt = attempt - i;
        originals[humn] = attempt;
        var check = Shout(root, originals.ToList());
        if (check == 0)
        {
            Console.WriteLine($"Found input: {attempt}");
        }
    }
}

long? Shout(int key, List<long?> vals)
{
    var n = vals[key];
    if (n.HasValue || operations[key] == null) return n;
    if (operations[key] is var (left, right, op))
    {
        var l = Shout(left, vals);
        var r = Shout(right, vals);
        if (!l.HasValue || !r.HasValue) n = null;
        else n = op switch
        {
            '+' => checked(l.Value + r.Value),
            '-' => checked(l.Value - r.Value),
            '*' => checked(l.Value * r.Value),
            '/' => l.Value / r.Value,
            _ => l.Value.CompareTo(r.Value)
        };
    }

    vals[key] = n;
    return n;
}

void DiscoverDependencies(int key, HashSet<int> dependencies)
{
    if (values[key].HasValue) return;
    dependencies.Add(key);
    if (operations[key] is var (left, right, _))
    {
        DiscoverDependencies(left, dependencies);
        DiscoverDependencies(right, dependencies);
    }
}

internal sealed record Operation(int Left, int Right, char Op);
