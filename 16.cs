var graph = ReadInput(Console.In);

// There are multiple tricks for part 1:
// * Simplify the graph by deleting nodes with zero flow (except AA). They all have only one edge in/out.
// * Use a heuristic to discard search paths that cannot possibly lead to a higher than current highest total flow.
var minEdgeWeight = graph.SelectMany(n => n.Next).Select(e => e.Weight).Min();

Console.WriteLine($"Searching graph of {graph.Count} nodes, min edge weight {minEdgeWeight}");

var sw = System.Diagnostics.Stopwatch.StartNew();
var answer = FindMaxFlow(graph, 30);

Console.WriteLine($"Max flow: {answer} (elapsed: {sw.Elapsed})");

// The trick for part two is to simply try every single partition of nodes into two sub-sets, simulating the
// maximum flow possible for each set in 26 steps. There are only 2^14 possible combinations to calculate, as the
// input has 15 nodes with non-zero flow, and we can cut that in half because of symmetry.
List<Node> a = new(), b = new();
var combinations = 1u << graph.Count - 2;
Console.WriteLine($"Testing {combinations} work partitions.");
sw = System.Diagnostics.Stopwatch.StartNew();
for (uint k = 0; k < combinations; ++k)
{
    var mask = k << 2 | 2;
    a.Add(graph[0]);
    b.Add(graph[0]);
    for (var i = 1; i < graph.Count; ++i)
    {
        if (((mask >> i) & 1) == 1)
        {
            a.Add(graph[i]);
        }
        else
        {
            b.Add(graph[i]);
        }
    }

    var aFlow = FindMaxFlow(a, 26);
    var bFlow = FindMaxFlow(b, 26);
    answer = Math.Max(answer, aFlow + bFlow);
    a.Clear();
    b.Clear();
}

Console.WriteLine($"Max flow with help: {answer} (elapsed: {sw.Elapsed})");

List<Node> ReadInput(TextReader reader)
{
    Dictionary<string, Node> nodes = new();
    while (reader.ReadLine()?.Split() is { } line)
    {
        Node GetOrCreateNode(string name)
            => nodes.TryGetValue(name, out var n) ? n : nodes[name] = new(name);

        var flow = int.Parse(line[4].Substring(5, line[4].Length - 6));
        var node = GetOrCreateNode(line[1]);
        node.Flow = flow;

        var targets = line[9..].Select(l => l.TrimEnd(','));
        node.Next.AddRange(targets.Select(t => new Edge(GetOrCreateNode(t), 1)));
    }

    return DeleteZeroNodes(nodes["AA"]);
}

List<Node> DeleteZeroNodes(Node start)
{
    List<Node> nodes = new();
    bool TryFindZero(out Node zero)
    {
        nodes.Clear();
        Queue<Node> queue = new();
        queue.Enqueue(start);
        while (queue.TryDequeue(out var node))
        {
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
            }

            if (node.Flow == 0 && !node.Equals(start))
            {
                zero = node;
                return true;
            }

            foreach (var edge in node.Next.Where(edge => !nodes.Contains(edge.Target)))
            {
                queue.Enqueue(edge.Target);
            }
        }

        zero = start;
        return false;
    }

    while (TryFindZero(out var zero))
    {
        // All zero nodes (except start) has only two edges.
        var weight = zero.Next[0].Weight + zero.Next[1].Weight;
        Node p = zero.Next[0].Target, q = zero.Next[1].Target;

        var i = p.Next.FindIndex(e => e.Target.Equals(zero));
        var j = q.Next.FindIndex(e => e.Target.Equals(zero));
        p.Next[i] = new(q, weight);
        q.Next[j] = new(p, weight);
    }

    return nodes;
}

int FindMaxFlow(List<Node> nodes, int steps)
{
    var withFlow = nodes.Where(n => n.Flow != 0).ToHashSet();
    return Search(nodes[0], 0, new HashSet<Node>(), 0, 0);

    int Search(Node node, int step, HashSet<Node> opened, int currentFlow, int maxTotalFlow)
    {
        if (step >= steps || opened.Count == withFlow.Count)
        {
            return currentFlow;
        }

        // Early exit if we know an upper bound on remaining flow is still less that current max solution.
        int heuristic = 0, futureStep = step;
        foreach (var n in nodes.Where(n => !opened.Contains(n)).OrderByDescending(n => n.Flow))
        {
            if (futureStep >= steps) break;
            heuristic += (steps - futureStep - 1) * n.Flow;
            // Spend at least minEdgeWeight minute moving to next node, and 1 minute opening.
            futureStep += minEdgeWeight + 1; // Real input has no edge less than cost 2.
        }

        if (currentFlow + heuristic < maxTotalFlow)
        {
            return heuristic;
        }

        // Open valve and go to next.
        if (!opened.Contains(node) && withFlow.Contains(node))
        {
            opened.Add(node);
            var flow = (steps - step - 1) * node.Flow; // Total contribution from this valve until the end.

            foreach (var edge in node.Next
                         .OrderBy(n => opened.Contains(n.Target))
                         .ThenByDescending(n => n.Target.Flow))
            {
                var branchTotalFlow = Search(edge.Target, step + 1 + edge.Weight, opened, currentFlow + flow, maxTotalFlow);
                maxTotalFlow = Math.Max(branchTotalFlow, maxTotalFlow);
            }

            opened.Remove(node);
        }

        // Skip valve and go to next immediately.
        foreach (var edge in node.Next
                     .OrderBy(n => opened.Contains(n.Target))
                     .ThenByDescending(n => n.Target.Flow))
        {
            var branchTotalFlow = Search(edge.Target, step + edge.Weight, opened, currentFlow, maxTotalFlow);
            maxTotalFlow = Math.Max(branchTotalFlow, maxTotalFlow);
        }

        return maxTotalFlow;
    }
}

internal sealed record Edge(Node Target, int Weight);

internal sealed class Node : IEquatable<Node>
{
    public string Name { get; }
    
    public int Flow { get; set; }
    
    public List<Edge> Next { get; }

    public Node(string name)
    {
        Name = name;
        Next = new();
    }

    public bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj) =>  ReferenceEquals(this, obj) || obj is Node other && Equals(other);

    public override int GetHashCode() => Name.GetHashCode();
}
