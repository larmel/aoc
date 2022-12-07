/*
$ cd /
$ ls
dir a
14848514 b.txt
8504156 c.dat
dir d
$ cd a
$ ls
dir e
29116 f
2557 g
62596 h.lst
$ cd e
$ ls
584 i
$ cd ..
$ cd ..
$ cd d
$ ls
4060174 j
8033020 d.log
5626152 d.ext
7214296 k
 */

Node root = new(null, "/"), current = root;
List<Node> directories = new() {root};
while (Console.ReadLine() is { } line)
{
    switch (line.Split())
    {
        case ["$", "ls"]:
            break;
        case ["$", "cd", ".."]:
            current = current.Parent!;
            break;
        case ["$", "cd", "/"]:
            current = root;
            break;
        case ["$", "cd", var dir]:
            current = current.Children[dir];
            break;
        case ["dir", var dir]:
            current.Children.Add(dir, new Node(current, dir));
            directories.Add(current.Children[dir]);
            break;
        case [var size, var name]:
            current.Children.Add(name, new Node(current, name));
            current.Children[name].AddSize(int.Parse(size));
            break;
    }
}

Console.WriteLine("P1: " + directories.Select(n => n.Size).Where(s => s < 100000).Sum());
Console.WriteLine("P2: " + directories.Select(n => n.Size).Where(s => s >= 30000000 - (70000000 - root.Size)).Min());

internal record Node(Node? Parent, string Name)
{
    public int Size { get; private set; }

    public Dictionary<string, Node> Children { get; } = new();

    public void AddSize(int size)
    {
        Size += size;
        Parent?.AddSize(size);
    }
}
