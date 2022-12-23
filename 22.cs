List<string> inputLines = new();
while (Console.ReadLine() is { } line)
{
    inputLines.Add(line);
}

var directions = ParseInput(inputLines[^1]).ToList();

var flatEarth = FlatEarth.Parse(inputLines);
flatEarth.Move(directions);
Console.WriteLine(flatEarth.Score());

/*
    1 1 2 2
    1 1 2 2
    3 3
    3 3
4 4 5 5
4 4 5 5
6 6
6 6
 */

var cube = Cube.Parse(inputLines, 50, new Position[]
{
    new(50, 0), new(2 * 50, 0), new(50, 50),
    new(0, 50 * 2), new(50, 50 * 2), new(0, 50 * 3)
});
cube.Move(directions);
Console.WriteLine(cube.Score());

IEnumerable<int> ParseInput(string inputLine)
{
    var i = 0;
    while (i < inputLine.Length)
    {
        switch (inputLine[i])
        {
            case 'L':
                i++;
                yield return World.Left;
                break;
            case 'R':
                i++;
                yield return World.Right;
                break;
            default:
                var d = inputLine[i] - '0';
                i++;
                while (i < inputLine.Length && char.IsDigit(inputLine[i]))
                {
                    d *= 10;
                    d += inputLine[i] - '0';
                    i++;
                }

                yield return d;
                break;
        }
    }
}

internal readonly record struct Position(int X, int Y);

internal abstract class World
{
    public const int Right = -1, Down = -2, Left = -3, Up = -4;

    protected int Direction { get; set; } = Right;
    
    protected abstract Position Position { get; }

    protected abstract void Step();

    public void Move(List<int> directions)
    {
        foreach (var move in directions)
        {
            switch (move)
            {
                case Left:
                    Direction = Direction switch {Up => Left, Left => Down, Down => Right, _ => Up};
                    break;
                case Right:
                    Direction = Direction switch {Up => Right, Left => Up, Down => Left, _ => Down};
                    break;
                case var d:
                    while (d > 0)
                    {
                        Step();
                        --d;
                    }

                    break;
            }
        }
    }

    public int Score()
    {
        // Facing is 0 for right (>), 1 for down (v), 2 for left (<), and 3 for up (^)
        var facing = Direction switch {Right => 0, Down => 1, Left => 2, _ => 3};
        return 1000 * Position.Y + 4 * Position.X + facing;
    }
}

internal sealed class FlatEarth : World
{
    private readonly char[][] _map;
    private readonly int _width, _height;
    private Position _position;

    protected override Position Position => _position;

    private FlatEarth(char[][] map)
    {
        _map = map;
        _width = map[0].Length - 2;
        _height = map.Length - 2;
        _position = new(Array.IndexOf(map[1], '.'), 1);
    }

    public static FlatEarth Parse(List<string> inputLines)
    {
        var width = inputLines.Take(inputLines.Count - 2).Max(l => l.Length);
        var height = inputLines.Count - 2;

        var map = new char[height + 2][];
        for (var i = 0; i < height + 2; ++i)
        {
            map[i] = new char[width + 2];
            Array.Fill(map[i], ' ');
            if (i > 0 && i <= height)
            {
                var line = inputLines[i - 1].ToCharArray();
                Array.Copy(line, 0, map[i], 1, line.Length);
            }
        }

        return new FlatEarth(map);
    }

    protected override void Step()
    {
        var p = Direction switch
        {
            Left => _position with {X = _position.X - 1},
            Right => _position with {X = _position.X + 1},
            Up => _position with {Y = _position.Y - 1},
            _ => _position with {Y = _position.Y + 1}
        };

        if (_map[p.Y][p.X] is '#') return;
        if (_map[p.Y][p.X] is not ' ')
        {
            _position = p;
            return;
        }

        switch (Direction)
        {
            case Left:
                p = p with {X = _width + 1};
                while (_map[p.Y][p.X] == ' ') p = p with {X = p.X - 1};
                break;
            case Right:
                p = p with {X = 0};
                while (_map[p.Y][p.X] == ' ') p = p with {X = p.X + 1};
                break;
            case Up:
                p = p with {Y = _height + 1};
                while (_map[p.Y][p.X] == ' ') p = p with {Y = p.Y - 1};
                break;
            case Down:
                p = p with {Y = 0};
                while (_map[p.Y][p.X] == ' ') p = p with {Y = p.Y + 1};
                break;
        }

        if (_map[p.Y][p.X] is not '#')
        {
            _position = p;
        }
    }
}

internal sealed class Cube : World
{
    private readonly int[][][] _faces;
    private readonly Position[] _offsets;

    private readonly int _width;
    private int _face;
    private Position _position;

    protected override Position Position
    {
        get
        {
            var offset = _offsets[_face - 1];
            return new(offset.X + _position.X + 1, offset.Y + _position.Y + 1);
        }
    }

    private Cube(int size, Position[] offsets)
    {
        _width = size;
        _offsets = offsets;
        _faces = new int[7][][];
        for (var f = 1; f <= 6; ++f)
        {
            _faces[f] = new int[size][];
            for (var i = 0; i < size; ++i)
            {
                _faces[f][i] = new int[size];
                Array.Fill(_faces[f][i], ' ');
            }
        }
    }

    public static Cube Parse(List<string> inputLines, int width, Position[] offsets)
    {
        var cube = new Cube(width, offsets);
        for (var f = 1; f <= 6; ++f)
        {
            var offset = cube._offsets[f - 1];
            for (var y = 0; y < width; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    cube._faces[f][y][x] = inputLines[y + offset.Y][x + offset.X];
                }
            }
        }

        cube._position = new(Array.IndexOf(cube._faces[1][0], '.'), 0);
        cube._face = 1;
        return cube;
    }

    protected override void Step()
    {
        var p = Direction switch
        {
            Left => _position with {X = _position.X - 1},
            Right => _position with {X = _position.X + 1},
            Up => _position with {Y = _position.Y - 1},
            _ => _position with {Y = _position.Y + 1}
        };

        var max = _width - 1;
        if (p.X < 0)
        {
            switch (_face)
            {
                case 1 when _faces[4][max - p.Y][0] is not '#':
                    _position = new(0, max - p.Y);
                    _face = 4;
                    Direction = Right;
                    break;
                case 2 when _faces[1][p.Y][max] is not '#':
                    _position = new(max, p.Y);
                    _face = 1;
                    break;
                case 3 when _faces[4][0][p.Y] is not '#':
                    _position = new(p.Y, 0);
                    _face = 4;
                    Direction = Down;
                    break;
                case 4 when _faces[1][max - p.Y][0] is not '#':
                    _position = new(0, max - p.Y);
                    _face = 1;
                    Direction = Right;
                    break;
                case 5 when _faces[4][p.Y][max] is not '#':
                    _position = new(max, p.Y);
                    _face = 4;
                    break;
                case 6 when _faces[1][0][p.Y] is not '#':
                    _position = new(p.Y, 0);
                    _face = 1;
                    Direction = Down;
                    break;
            }
        }
        else if (p.X == _width)
        {
            switch (_face)
            {
                case 1 when _faces[2][p.Y][0] is not '#':
                    _position = new(0, p.Y);
                    _face = 2;
                    break;
                case 2 when _faces[5][max - p.Y][max] is not '#':
                    _position = new(max, max - p.Y);
                    _face = 5;
                    Direction = Left;
                    break;
                case 3 when _faces[2][max][p.Y] is not '#':
                    _position = new(p.Y, max);
                    _face = 2;
                    Direction = Up;
                    break;
                case 4 when _faces[5][p.Y][0] is not '#':
                    _position = new(0, p.Y);
                    _face = 5;
                    break;
                case 5 when _faces[2][max - p.Y][max] is not '#':
                    _position = new(max, max - p.Y);
                    _face = 2;
                    Direction = Left;
                    break;
                case 6 when _faces[5][max][p.Y] is not '#':
                    _position = new(p.Y, max);
                    _face = 5;
                    Direction = Up;
                    break;
            }
        }
        else if (p.Y < 0)
        {
            switch (_face)
            {
                case 1 when _faces[6][p.X][0] is not '#':
                    _position = new(0, p.X);
                    _face = 6;
                    Direction = Right;
                    break;
                case 2 when _faces[6][max][p.X] is not '#':
                    _position = new(p.X, max);
                    _face = 6;
                    break;
                case 3 when _faces[1][max][p.X] is not '#':
                    _position = new(p.X, max);
                    _face = 1;
                    break;
                case 4 when _faces[3][p.X][0] is not '#':
                    _position = new(0, p.X);
                    Direction = Right;
                    _face = 3;
                    break;
                case 5 when _faces[3][max][p.X] is not '#':
                    _position = new(p.X, max);
                    _face = 3;
                    break;
                case 6 when _faces[4][max][p.X] is not '#':
                    _position = new(p.X, max);
                    _face = 4;
                    break;
            }
        }
        else if (p.Y == _width)
        {
            switch (_face)
            {
                case 1 when _faces[3][0][p.X] is not '#':
                    _position = new(p.X, 0);
                    _face = 3;
                    break;
                case 2 when _faces[3][p.X][max] is not '#':
                    _position = new(max, p.X);
                    _face = 3;
                    Direction = Left;
                    break;
                case 3 when _faces[5][0][p.X] is not '#':
                    _position = new(p.X, 0);
                    _face = 5;
                    break;
                case 4 when _faces[6][0][p.X] is not '#':
                    _position = new(p.X, 0);
                    _face = 6;
                    break;
                case 5 when _faces[6][p.X][max] is not '#':
                    _position = new(max, p.X);
                    _face = 6;
                    Direction = Left;
                    break;
                case 6 when _faces[2][0][p.X] is not '#':
                    _position = new(p.X, 0);
                    _face = 2;
                    break;
            }
        }
        else
        {
            if (_faces[_face][p.Y][p.X] is not '#') 
            {
                _position = p;
            }
        }
    }
}
