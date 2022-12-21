using System.Collections.Concurrent;
using System.Diagnostics;

List<Blueprint> blueprints = new();
while (Console.ReadLine()?.Split() is { } line)
{
    State
        oreRobotCost = new (int.Parse(line[6])),
        clayRobotCost = new (int.Parse(line[12])),
        obsidianRobotCost = new (int.Parse(line[18]), int.Parse(line[21])),
        geodeRobotCost = new (int.Parse(line[27]), 0, int.Parse(line[30]));

    Blueprint blueprint = new (blueprints.Count + 1, oreRobotCost, clayRobotCost, obsidianRobotCost, geodeRobotCost);
    blueprints.Add(blueprint);
}

Console.WriteLine($"Part 1: {Part1()}");
Console.WriteLine($"Part 2: {Part2()}");

int Part1()
{
    var quality = 0;
    Parallel.ForEach(blueprints, blueprint =>
    {
        var n = Solve(blueprint, 24);
        Interlocked.Add(ref quality, blueprint.Id * n);
    });

    return quality;
}

int Part2()
{
    ConcurrentBag<int> results = new();
    Parallel.ForEach(blueprints.Take(Math.Min(3, blueprints.Count)), blueprint =>
    {
        var n = Solve(blueprint, 32);
        results.Add(n);
    });

    return results.Aggregate(1, (a, b) => a * b);
}

int Solve(Blueprint blueprint, int steps)
{
    var sw = Stopwatch.StartNew();
    var n = 0;
    FindMaxGeodeCount(blueprint, steps, new State(), new State(1), ref n);
    Console.WriteLine($"[{blueprint.Id}] Max number of geodes: {n} (Elapsed: {sw.Elapsed})");
    return n;
}

void FindMaxGeodeCount(Blueprint blueprint, int remainingSteps, State count, State delta, ref int currentBest)
{
    if (remainingSteps == 0)
    {
        if (count.Geode > currentBest)
        {
            //Console.WriteLine($"Current best: {currentBest}");
            currentBest = count.Geode;
        }

        return;
    }

    var heuristic = count.Geode + remainingSteps * delta.Geode;
    if (remainingSteps > 1)
    {
        // Absolute best case, we can create a new geode robot each step.
        var possible = count.CanCreate(blueprint.GeodeRobotCost) ? remainingSteps : remainingSteps - 1;
        var extra = (possible) * (possible - 1) / 2;
        heuristic += extra;
    }

    if (heuristic < currentBest)
    {
        return;
    }

    // If we can create a geode robot, then that is always the correct choice.
    if (count.CanCreate(blueprint.GeodeRobotCost))
    {
        var d = delta with {Geode = delta.Geode + 1};
        var c = count - blueprint.GeodeRobotCost;
        FindMaxGeodeCount(blueprint, remainingSteps - 1, c + delta, d, ref currentBest);
        return;
    }

    if (count.CanCreate(blueprint.ObsidianRobotCost))
    {
        var d = delta with {Obsidian = delta.Obsidian + 1};
        var c = count - blueprint.ObsidianRobotCost;
        FindMaxGeodeCount(blueprint, remainingSteps - 1, c + delta, d, ref currentBest);
    }

    if (count.CanCreate(blueprint.ClayRobotCost))
    {
        var d = delta with {Clay = delta.Clay + 1};
        var c = count - blueprint.ClayRobotCost;
        FindMaxGeodeCount(blueprint, remainingSteps - 1, c + delta, d, ref currentBest);
    }

    if (count.CanCreate(blueprint.OreRobotCost))
    {
        var d = delta with {Ore = delta.Ore + 1};
        var c = count - blueprint.OreRobotCost;
        FindMaxGeodeCount(blueprint, remainingSteps - 1, c + delta, d, ref currentBest);
    }

    // Also an option to not create any robot.
    count += delta;
    FindMaxGeodeCount(blueprint, remainingSteps - 1, count, delta, ref currentBest);
}

internal readonly record struct State(int Ore = 0, int Clay = 0, int Obsidian = 0, int Geode = 0)
{
    public bool CanCreate(State cost) => Ore >= cost.Ore && Clay >= cost.Clay && Obsidian >= cost.Obsidian;

    public static State operator +(State a, State b)
        => new(a.Ore + b.Ore, a.Clay + b.Clay, a.Obsidian + b.Obsidian, a.Geode + b.Geode);

    public static State operator -(State a, State b)
        => new(a.Ore - b.Ore, a.Clay - b.Clay, a.Obsidian - b.Obsidian, a.Geode - b.Geode);
}

internal readonly record struct Blueprint(int Id, State OreRobotCost, State ClayRobotCost, State ObsidianRobotCost, State GeodeRobotCost);
