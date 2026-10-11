// Лабораторная работа 1. Вариант 7.
// Задание 1 — факториал числа n;
// Задание 2 — числа Фибоначчи от 0 до n;
// Задание 3 — функция A = cth(cbrt(x^2)) * exp(sinh(x^5 + 13));
// Задание 4 — ряд Тейлора для cos x.

using System.Globalization;

const double Eps = 1e-6;    // точность суммирования ряда (задание 4)

bool running = true;

while (running)
{
    Console.Write("Выберите задание (1-4, 0 - выход): ");

    if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 4)
    {
        Console.WriteLine("Ошибка: введите целое число от 0 до 4.");
        continue;
    }

    switch (choice)
    {
        case 1: Task1(); break;
        case 2: Task2(); break;
        case 3: Task3(); break;
        case 4: Task4(); break;
        case 0: running = false; break;
    }

    Console.WriteLine();
}

// ------------------------- Задание 1. Факториал -------------------------

static void Task1()
{
    Console.Write("Задание 1. Введите n (0..20): ");

    // 21! уже не помещается в long, поэтому n ограничено сверху.
    if (!int.TryParse(Console.ReadLine(), out int n) || n < 0 || n > 20)
    {
        Console.WriteLine("Ошибка: нужно целое число от 0 до 20.");
        return;
    }

    Console.WriteLine($"{n}! = {Factorial(n)}");
}

// Факториал: произведение 1 * 2 * ... * n. При n = 0 результат равен 1.
static long Factorial(int n)
{
    long result = 1;

    for (int i = 2; i <= n; i++)
        result *= i;

    return result;
}

// --------------------- Задание 2. Числа Фибоначчи -----------------------

static void Task2()
{
    Console.Write("Задание 2. Введите n (0..92): ");

    // F(92) — наибольшее число Фибоначчи, помещающееся в long.
    if (!int.TryParse(Console.ReadLine(), out int n) || n < 0 || n > 92)
    {
        Console.WriteLine("Ошибка: нужно целое число от 0 до 92.");
        return;
    }

    List<long> fib = Fibonacci(n);
    Console.WriteLine(string.Join(", ", fib));
}

// Список чисел Фибоначчи от F(0) = 0 до F(n) включительно.
static List<long> Fibonacci(int n)
{
    List<long> numbers = new List<long>();
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

// ------------------------ Задание 3. Функция ----------------------------

static void Task3()
{
    Console.Write("Задание 3. Введите x: ");

    if (!double.TryParse(Console.ReadLine(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double x))
    {
        Console.WriteLine("Ошибка: нужно вещественное число (например, -1.5).");
        return;
    }

    double a = FunctionA(x, out string? error);

    if (error is not null)
        Console.WriteLine($"Ошибка: {error}");
    else
        Console.WriteLine($"A({x}) = {a:E6}");
}

// A = cth(cbrt(x^2)) * exp(sinh(x^5 + 13)).
// Если при данном x функция не определена, значение равно NaN,
// а в error записывается текст сообщения для пользователя.
static double FunctionA(double x, out string? error)
{
    error = null;
    double root = Math.Cbrt(x * x);     // кубический корень из x^2

    if (root == 0)                      // при x = 0 cth(0) не существует
    {
        error = "при x = 0 функция не определена (cth(0) — деление на ноль).";
        return double.NaN;
    }

    double cth = Math.Cosh(root) / Math.Sinh(root);     // cth z = ch z / sh z
    double sh = Math.Sinh(Math.Pow(x, 5) + 13);
    double a = cth * Math.Exp(sh);

    if (double.IsInfinity(a) || double.IsNaN(a))        // переполнение double
    {
        error = "при данном x функция не определена (переполнение exp(sh(x^5 + 13))).";
        return double.NaN;
    }

    return a;
}

// --------------------- Задание 4. Ряд Тейлора ---------------------------

static void Task4()
{
    Console.Write("Задание 4. Введите x: ");

    if (!double.TryParse(Console.ReadLine(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double x))
    {
        Console.WriteLine("Ошибка: нужно вещественное число (например, 2).");
        return;
    }

    double sum = TaylorCos(x, Eps, out int count);

    // Для сравнения выводим и сумму ряда, и значение библиотечной функции.
    Console.WriteLine($"Сумма ряда:            {Fmt(sum)}");
    Console.WriteLine($"Math.Cos(x):           {Fmt(Math.Cos(x))}");
    Console.WriteLine($"Просуммировано членов: {count}");
}

// Формат вещественного числа для вывода: 10 знаков после точки.
static string Fmt(double value) => value.ToString("F10", CultureInfo.InvariantCulture);

// Ряд Тейлора для cos x: cos x = 1 - x^2/2! + x^4/4! - x^6/6! + ...
// Члены суммируются, пока очередной член по модулю больше eps.
// Следующий член получается из предыдущего рекуррентно:
// t(n) = -t(n-1) * x^2 / ((2n - 1) * 2n) —
// факториал заново не вычисляется.
static double TaylorCos(double x, double eps, out int count)
{
    double term = 1.0;      // t(0) = 1
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
