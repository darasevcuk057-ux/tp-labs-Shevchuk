using Lab4.Core;

namespace Lab4.Tests;

public class MathAlgorithmsTests
{
    // --- Факториал ---

    [Fact]
    public void Factorial_OfZero_ReturnsOne()
    {
        long result = (long)MathAlgorithms.Factorial(0);
        Assert.Equal(1, result);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(10, 3628800)]
    [InlineData(20, 2432902008176640000)]
    public void Factorial_ValidN_ReturnsExpected(int n, long expected)
    {
        long result = (long)MathAlgorithms.Factorial(n);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Factorial_OutOfRange_Throws(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.Factorial(n));
    }

    // --- Числа Фибоначчи ---

    [Fact]
    public void Fibonacci_First6_AreCorrect()
    {
        var seq = MathAlgorithms.Fibonacci(5);
        Assert.Equal(new long[] { 0, 1, 1, 2, 3, 5 }, seq);
    }

    [Fact]
    public void Fibonacci_Zero_ReturnsSingleZero()
    {
        var seq = MathAlgorithms.Fibonacci(0);
        Assert.Single(seq);
        Assert.Equal(0, seq[0]);
    }

    [Fact]
    public void Fibonacci_92_LastIsMaxLong()
    {
        var seq = MathAlgorithms.Fibonacci(92);
        Assert.Equal(7540113804746346429, seq[92]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(93)]
    public void Fibonacci_OutOfRange_Throws(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.Fibonacci(n));
    }

    // --- Функция A ---

    [Fact]
    public void FunctionA_AtZero_ReturnsError()
    {
        double result = MathAlgorithms.FunctionA(0, out string? error);
        Assert.True(double.IsNaN(result));
        Assert.NotNull(error);
    }

    [Fact]
    public void FunctionA_AtValidX_ReturnsFiniteValue()
    {
        // x = -1.7: x^5 + 13 ≈ -1.2, exp(sinh(-1.2)) — конечное,
        // cth(cbrt(x^2)) — конечное. Функция определена.
        double result = MathAlgorithms.FunctionA(-1.7, out string? error);
        Assert.Null(error);
        Assert.False(double.IsNaN(result));
        Assert.False(double.IsInfinity(result));
    }

    // --- Ряд Тейлора для cos ---

    [Fact]
    public void TaylorCos_AtZero_ReturnsOne()
    {
        double result = MathAlgorithms.TaylorCos(0, 1e-6, out int count);
        Assert.Equal(1.0, result);
        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(Math.PI)]
    public void TaylorCos_ValidX_MatchesMathCos(double x)
    {
        double result = MathAlgorithms.TaylorCos(x, 1e-6, out _);
        Assert.Equal(Math.Cos(x), result, 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1e-6)]
    public void TaylorCos_NonPositiveEps_Throws(double eps)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.TaylorCos(1, eps, out _));
    }
}
