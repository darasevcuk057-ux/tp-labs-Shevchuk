using System.Diagnostics;

namespace Lab5;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Лабораторная работа 5. Вариант 7.");
        Console.WriteLine("Умножение матриц: последовательная и параллельная версии.");

        Console.Write("Введите размер матрицы (по умолчанию 1000): ");
        int n = int.TryParse(Console.ReadLine(), out int parsed) && parsed > 0
            ? parsed
            : 1000;

        Console.Write("Введите через пробел числа потоков (1 2 4 8): ");
        var threadCounts = (Console.ReadLine() ?? "1 2 4 8")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out int v) && v > 0 ? v : 1)
            .Distinct()
            .OrderBy(v => v)
            .ToArray();

        // Одинаковые матрицы для всех замеров.
        var a = MatrixMultiplier.GenerateRandom(n, seed: 42);
        var b = MatrixMultiplier.GenerateRandom(n, seed: 137);

        Console.WriteLine();
        Console.WriteLine($"Матрицы {n}x{n}, ядер доступно: {Environment.ProcessorCount}");
        Console.WriteLine();

        // 1) Последовательная версия (один прогон).
        var sw = Stopwatch.StartNew();
        var seqResult = MatrixMultiplier.MultiplySequential(a, b);
        sw.Stop();
        long seqMs = sw.ElapsedMilliseconds;
        Console.WriteLine($"Последовательно:  {seqMs} мс");
        Console.WriteLine();

        // 2) Параллельная версия для каждого числа потоков.
        Console.WriteLine($"{"Потоков",-8} {"Время, мс",-12} {"Ускорение",-10}");
        Console.WriteLine(new string('-', 32));

        foreach (int threads in threadCounts)
        {
            sw.Restart();
            var parResult = MatrixMultiplier.MultiplyParallel(a, b, threads, CancellationToken.None);
            sw.Stop();
            long parMs = sw.ElapsedMilliseconds;
            double speedup = parMs > 0 ? (double)seqMs / parMs : 0;

            bool same = MatrixMultiplier.AreEqual(seqResult, parResult);
            Console.WriteLine($"{threads,-8} {parMs,-12} {speedup,-10:F2}" +
                             (same ? "" : "  РЕЗУЛЬТАТЫ НЕ СОВПАЛИ!"));

            if (!same)
                Console.WriteLine("ОШИБКА: результаты последовательной и параллельной версии различаются (гонка данных).");
        }

        // 3) Бонус: отмена вычисления по нажатию клавиши.
        Console.WriteLine();
        Console.WriteLine("Бонус: проверка отмены. Запускаю умножение, нажмите любую клавишу для отмены.");
        RunWithCancellation(a, b, threadCounts.Length > 0 ? threadCounts[^1] : 4);

        Console.WriteLine();
        Console.WriteLine("Готово. Нажмите Enter для выхода.");
        Console.ReadLine();
    }

    // Бонусное задание: отмена длительной операции через CancellationToken.
    private static void RunWithCancellation(int[,] a, int[,] b, int threads)
    {
        using var cts = new CancellationTokenSource();

        // Фоновый поток, который ждёт нажатия клавиши и отменяет задачу.
        var keyThread = new Thread(() =>
        {
            try
            {
                Console.ReadKey(true);
                cts.Cancel();
            }
            catch (InvalidOperationException)
            {
                // Входной поток недоступен (например, при запуске без консоли).
            }
        });
        keyThread.IsBackground = true;
        keyThread.Start();

        var sw = Stopwatch.StartNew();
        try
        {
            _ = MatrixMultiplier.MultiplyParallel(a, b, threads, cts.Token);
            sw.Stop();
            Console.WriteLine($"Вычисление завершено без отмены за {sw.ElapsedMilliseconds} мс.");
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            Console.WriteLine($"Операция отменена по нажатию клавиши через {sw.ElapsedMilliseconds} мс.");
        }
    }
}
