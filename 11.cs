List<Monkey> monkeys = new();
List<List<long>> initialItems = new();

while (Console.ReadLine()?.Split() is ["Monkey", _])
{
    var line = Console.ReadLine()!
        .Replace(",", "")
        .Replace("Starting items:", "")
        .Trim();

    initialItems.Add(line.Split().Select(long.Parse).ToList());

    if (Console.ReadLine()!.Trim().Split() is ["Operation:", "new", "=", "old", var op, var r])
    {
        Func<long, long> operation = int.TryParse(r, out var d)
            ? x => op == "*" ? x * d : x + d
            : x => op == "*" ? x * x : x + x;

        var divisor = long.Parse(Console.ReadLine()!.Split()[^1]);
        var trueTarget = int.Parse(Console.ReadLine()!.Split()[^1]);
        var falseTarget = int.Parse(Console.ReadLine()!.Split()[^1]);
        monkeys.Add(new (operation, divisor, trueTarget, falseTarget));
    }

    Console.ReadLine();
}

long MonkeyBusiness(int rounds, bool relax)
{
    var inspections = new long[monkeys.Count];
    var items = new Queue<Item>[initialItems.Count];
    for (var i = 0; i < monkeys.Count; ++i)
    {
        items[i] = new Queue<Item>(initialItems[i].Select(n => new Item(n, monkeys, relax)));
    }

    for (var round = 0; round < rounds; ++round)
    {
        for (var i = 0; i < monkeys.Count; ++i)
        {
            var monkey = monkeys[i];
            while (items[i].TryDequeue(out var item))
            {
                inspections[i] += 1;
                item.Apply(c => monkey.Operation(c));
                var value = item.Values[i];
                var target = value.Remainder == 0 ? monkey.TrueTarget : monkey.FalseTarget;
                items[target].Enqueue(item);
            }
        }

        if (round is 0 or 19 || (round + 1) % 1000 == 0)
        {
            Console.WriteLine($"== After round {round + 1} ==");
            for (var i = 0; i < monkeys.Count; ++i)
            {
                Console.WriteLine($"Monkey {i} inspected items {inspections[i]} times");
            }
        }
    }

    Array.Sort(inspections);
    return inspections[^1] * inspections[^2];
}

Console.WriteLine($"Monkey business: {MonkeyBusiness(20, true)}");
Console.WriteLine($"Monkey business: {MonkeyBusiness(10000, false)}");

internal sealed record Monkey(Func<long, long> Operation, long Divisor, int TrueTarget, int FalseTarget);

internal sealed record Result(long Worry, long Remainder, long Mod);

internal sealed class Item
{
    private readonly bool _relax;

    // One worry value per monkey, mod its divisor.
    public readonly List<Result> Values;

    public Item(long value, List<Monkey> monkeys, bool relax)
    {
        _relax = relax;
        Values = monkeys.Select(m => new Result(value, value % m.Divisor, m.Divisor)).ToList();
    }

    public void Apply(Func<long, long> op)
    {
        for (var i = 0; i < Values.Count; ++i)
        {
            var v = Values[i];
            var worry = op(v.Worry);
            Values[i] = _relax
                ? v with
                {
                    Worry = worry / 3,
                    Remainder = (worry / 3) % v.Mod
                }
                : v with
                {
                    Worry = worry % v.Mod,
                    Remainder = op(v.Remainder) % v.Mod
                };
        }
    }
}
