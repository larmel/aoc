#include <stdio.h>
#include <string.h>

static int cycle(int c, int x)
{
    int pos = (c - 1) % 40;
    if (pos == 0)
        putchar('\n');
    putchar(pos >= x - 1 && pos <= x + 1 ? '#' : '.');
    return c == 20 || ((c + 20) % 40 == 0) ? x * c : 0;
}

int main(void)
{
    char op[5];
    int c, d, x, signal;
    for (c = 1, x = 1, signal = 0; scanf("%s ", op) != EOF; ++c)
    {
        signal += cycle(c, x);
        if (!strcmp("addx", op) && scanf("%d ", &d) != EOF)
        {
            c += 1;
            signal += cycle(c, x);
            x += d;
        }
    }

    return printf("\n\nSignal strength: %d\n", signal), 0;
}
