using System.Numerics;

namespace Lab4.Core;

// Алгоритмы из лабораторной работы 1 (вариант 7).
public static class MathAlgorithms
{
    // Факториал: 1 * 2 * ... * n. При n = 0 результат 1.
    // Бонус лаб 1: BigInteger — нет ограничения n <= 20.
    public static BigInteger Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "n должно быть неотрицательным.");
        if (n > 1000)
            throw new ArgumentOutOfRangeException(nameof(n), "n слишком большое (максимум 1000).");

        BigInteger result = BigInteger.One;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    // Числа Фибоначчи от F(0) = 0 до F(n) включительно.
    // F(92) — максимум, помещающийся в long.
    public static IReadOnlyList<long> Fibonacci(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "n должно быть неотрицательным.");
        if (n > 92)
            throw new ArgumentOutOfRangeException(nameof(n), "n слишком большое (максимум 92, дальше переполнение long).");

        var numbers = new List<long>();
        long a = 0, b = 1;
        for (int i = 0; i <= n; i++)
        {
            numbers.Add(a);
            long next = a + b;
            a = b;
            b = next;
        }
        return numbers;
    }

    // A = cth(cbrt(x^2)) * exp(sinh(x^5 + 13)).
    // Если функция не определена при данном x, возвращает NaN
    // и текст ошибки в error.
    public static double FunctionA(double x, out string? error)
    {
        error = null;
        double root = Math.Cbrt(x * x);

        if (root == 0)
        {
            error = "при x = 0 функция не определена (cth(0) — деление на ноль).";
            return double.NaN;
        }

        double cth = Math.Cosh(root) / Math.Sinh(root);
        double sh = Math.Sinh(Math.Pow(x, 5) + 13);
        double a = cth * Math.Exp(sh);

        if (double.IsInfinity(a) || double.IsNaN(a))
        {
            error = "при данном x функция не определена (переполнение).";
            return double.NaN;
        }

        return a;
    }

    // Ряд Тейлора для cos x.
    // Члены суммируются, пока очередной по модулю больше eps.
    public static double TaylorCos(double x, double eps, out int count)
    {
        if (eps <= 0)
            throw new ArgumentOutOfRangeException(nameof(eps), "eps должно быть положительным.");

        double term = 1.0;
        double sum = 0.0;
        count = 0;

        while (Math.Abs(term) > eps)
        {
            sum += term;
            count++;
            term *= -x * x / ((2 * count - 1) * 2 * count);
        }

        return sum;
    }
}
