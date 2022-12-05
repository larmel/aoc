/*
    [D]    
[N] [C]    
[Z] [M] [P]
 1   2   3 

move 1 from 2 to 1
move 3 from 1 to 3
move 2 from 2 to 1
move 1 from 1 to 2
 */

string line = Console.ReadLine()!;
int i, n = (line.Length + 1) / 4;

List<char>[] stack = new List<char>[n];
List<char>[] list = new List<char>[n];
for (i = 0; i < n; ++i)
{
    stack[i] = new List<char>();
    list[i] = new List<char>();
}
do
{
    for (i = 0; i < n; ++i)
    {
        char c = line[i * 4 + 1];
        if (char.IsLetter(c))
        {
            stack[i].Insert(0, c);
            list[i].Insert(0, c);
        }
    }

    line = Console.ReadLine()!;
} while (line.Length != 0);

while (true)
{
    string? str = Console.ReadLine();
    string[]? op = str?.Split(' ');
    if (op is ["move", var countStr, "from", var fromStr, "to", var toStr])
    {
        int count = int.Parse(countStr), from = int.Parse(fromStr) - 1, to = int.Parse(toStr) - 1;
        for (; count > 0; --count)
        {
            stack[to].Add(stack[from][^1]);
            stack[from].RemoveAt(stack[from].Count - 1);
            
            list[to].Add(list[from][^count]);
            list[from].RemoveAt(list[from].Count - count);
        }
    }
    else break;
}

Console.WriteLine("P1: " + string.Join("", stack.Select(s => s.Count != 0 ? $"{s[^1]}" : "")));
Console.WriteLine("P2: " + string.Join("", list.Select(s => s.Count != 0 ? $"{s[^1]}" : "")));
