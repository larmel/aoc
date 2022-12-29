var sum = 0L;
while (Console.ReadLine() is { } line)
{
    var d = FromSnafu(line);
    sum += d;
}

var conv = ToSnafu(sum);
Console.WriteLine($"Sum: {sum} => {conv}");

long FromSnafu(string snafu)
{
    long d = 0L, scale = 1;
    for (var i = 0; i < snafu.Length; ++i)
    {
        d += scale * snafu[^(i + 1)] switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '-' => -1,
            _ => -2
        };

        scale *= 5;
    }

    return d;
}

//
// Example: 4890
//
// 5^5   5^4   5^3   5^2   5^1   5^0
//   1     2     4     0     3     0
//   1     2     4     1    -2     0
//   1     3    -1     1    -2     0
//   2    -2    -1     1    -2     0   :   2=-1=0
//
string ToSnafu(long d)
{
    List<long> rem = new();
    do
    {
        var r = d % 5;
        d /= 5;
        rem.Add(r);
    } while (d > 0);

    rem.Add(0);
    for (var i = 0; i < rem.Count - 1; ++i)
    {
        while (rem[i] < -2)
        {
            rem[i] += 5;
            rem[i + 1] -= 1;
        }

        while (rem[i] > 2)
        {
            rem[i] -= 5;
            rem[i + 1] += 1;
        }
    }

    return string.Join("", rem.Select(r => r switch
    {
        -2 => "=",
        -1 => "-",
        _ => r.ToString()
    }).Reverse()).TrimStart('0');
}
