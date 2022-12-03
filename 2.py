
def solve():
  # A = Rock, B = Paper, C = Scissor
  # X = Rock (1p), Y = Paper (2p), Z = Scissor (3p)
  # Loss = 0p, Draw = 3p {'A X', 'B Y', 'C Z'}, Win = 6p {'A Y', 'B Z', 'C X'}
  points = 0
  with open('2.txt') as fp:
    for line in fp:
      round = line.strip()
      a = ord(round[0]) - ord('A')
      b = ord(round[2]) - ord('X')
      points += b + 1
      points += 3 if (a == b) else 0
      points += 6 if (b == ((a + 1) % 3)) else 0
  return points

def solve2():
  # A = Rock (1p), B = Paper (2p), C = Scissor (3p)
  # X = Loss (0p), Y = Draw (3p), Z = Win (6p)
  points = 0
  with open('2.txt') as fp:
    for line in fp:
      round = line.strip()
      a = ord(round[0]) - ord('A')
      b = ord(round[2]) - ord('X')
      b = (a + 2) % 3 if b == 0 else a if b == 1 else (a + 1) % 3
      points += b + 1
      points += 3 if (a == b) else 0
      points += 6 if (b == ((a + 1) % 3)) else 0
  return points

if __name__ == '__main__':
    print("points = {}".format(solve()))
    print("points = {}".format(solve2()))
