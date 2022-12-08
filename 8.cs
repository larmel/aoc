/*
30373
25512
65332
33549
35390
 */

char[] row = Console.ReadLine()!.ToArray();
int n = row.Length;
char[][] forest = new char[n][];
forest[0] = row;
for (int r = 1; r < n; ++r)
{
    forest[r] = Console.ReadLine()!.ToArray();
}

int scenic = 0;
int visible = 4 * n - 4;
for (int r = 1; r < n - 1; ++r)
{
    for (int c = 1; c < n - 1; ++c)
    {
        char tree = forest[r][c];
        int up = Enumerable.Range(0, r).Reverse().TakeWhile(x => forest[x][c] < tree).Count(),
            down = Enumerable.Range(r + 1, n - r - 1).TakeWhile(x => forest[x][c] < tree).Count(),
            left = Enumerable.Range(0, c).Reverse().TakeWhile(x => forest[r][x] < tree).Count(),
            right = Enumerable.Range(c + 1, n - c - 1).TakeWhile(x => forest[r][x] < tree).Count();

        bool isVisible = up == r || down == n - r - 1 || left == c || right == n - c - 1;
        visible += isVisible ? 1 : 0;

        int Cap(int x, int l) => x < l ? x + 1 : x;
        int score = Cap(up, r) * Cap(down, n - r - 1) * Cap(left, c) * Cap(right, n - c - 1);
        scenic = Math.Max(scenic, score);
    }
}

Console.WriteLine($"Visible trees: {visible}");
Console.WriteLine($"Scenic score: {scenic}");
