using System.Diagnostics;

namespace Lab5;

// Умножение квадратных матриц: последовательно и параллельно.
// Параллельная версия делит строки результата между потоками через Parallel.For.
public static class MatrixMultiplier
{
    // Последовательное умножение матриц a и b размера n x n.
    public static int[,] MultiplySequential(int[,] a, int[,] b)
    {
        int n = a.GetLength(0);
        int m = a.GetLength(1);
        int k = b.GetLength(1);
        var result = new int[n, k];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < k; j++)
            {
                int sum = 0;
                for (int t = 0; t < m; t++)
                    sum += a[i, t] * b[t, j];
                result[i, j] = sum;
            }

        return result;
    }

    // Параллельное умножение через Parallel.For по строкам результата.
    // maxDegreeOfParallelism — число потоков (задаёт пользователь).
    public static int[,] MultiplyParallel(int[,] a, int[,] b,
        int maxDegreeOfParallelism, CancellationToken token)
    {
        int n = a.GetLength(0);
        int m = a.GetLength(1);
        int k = b.GetLength(1);
        var result = new int[n, k];

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = token
        };

        // Каждая строка i считается независимо, поэтому их можно
        // безопасно распределять между потоками.
        Parallel.For(0, n, options, i =>
        {
            for (int j = 0; j < k; j++)
            {
                int sum = 0;
                for (int t = 0; t < m; t++)
                    sum += a[i, t] * b[t, j];
                result[i, j] = sum;
            }
        });

        return result;
    }

    // Проверка, что две матрицы совпадают.
    public static bool AreEqual(int[,] a, int[,] b)
    {
        if (a.GetLength(0) != b.GetLength(0) ||
            a.GetLength(1) != b.GetLength(1))
            return false;

        for (int i = 0; i < a.GetLength(0); i++)
            for (int j = 0; j < a.GetLength(1); j++)
                if (a[i, j] != b[i, j])
                    return false;

        return true;
    }

    // Случайная матрица n x n со значениями от 0 до 9.
    public static int[,] GenerateRandom(int n, int seed)
    {
        var rand = new Random(seed);
        var matrix = new int[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                matrix[i, j] = rand.Next(0, 10);
        return matrix;
    }
}
