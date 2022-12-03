#include <stdio.h>
#include <string.h>

static int score(const char *sack, int len)
{
  int c, i, j;
  for (i = len / 2; i < len; ++i)
  {
    c = sack[i];
    for (j = 0; j < len / 2; ++j)
    {
      if (sack[j] == c)
      {
        int wat = c <= 'Z' ? (c - 'A' + 27) : c - 'a' + 1;
        return wat;
      }
    }
  }

  return 0;
}

int main(void)
{
  static char sack[26 * 2 + 1];

  int s1 = 0, s2 = 0;
  while (!feof(stdin))
  {
    unsigned long flag[3] = {0};
    int i, j, l;
    for (i = 0; i < 3; ++i)
    {
      fscanf(stdin, "%s ", sack);
      l = strlen(sack);
      s1 += score(sack, l);
      for (j = 0; j < l; ++j)
      {
        int c = sack[j];
        c = c <= 'Z' ? c - 'A' + 26 : c - 'a';
        flag[i] |= 1ul << c;
      }
    }
    flag[0] = flag[0] & flag[1] & flag[2];
    for (i = 0; i < 2 * 26; ++i)
    {
      if (flag[0] & 1)
      {
        s2 += i + 1;
        break;
      }

      flag[0] >>= 1;
    }
  }

  printf("part1: %d\n", s1);
  printf("part2: %d\n", s2);
  return 0;
}
