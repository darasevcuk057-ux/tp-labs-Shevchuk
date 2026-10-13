// Лабораторная работа №3. Потоки ввода-вывода и файловая система.
// Вариант №7: Слияние файлов. Объединить все .txt-файлы указанной папки
// в один итоговый файл; перед содержимым каждого файла вставлять
// строку-разделитель с его именем.
// Выполнила: Шевчук Дарья Александровна, группа ИИб-242.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab3;

internal class Program
{
    // Поле для хранения пути к последнему созданному итоговому файлу
    private static string? _lastOutputFilePath;

    private static async Task Main(string[] args)
    {
        // Регистрируем поддержку кодовых страниц Windows-1251 и CP866 (Бонусное задание №3)
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        bool running = true;

        while (running)
        {
            PrintMenu();
            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RunSyncMerge();
                    break;
                case "2":
                    await RunAsyncMerge();
                    break;
                case "3":
                    RunDemoWithSampleFiles();
                    break;
                case "4":
                    ShowMergedFileContent();
                    break;
                case "5":
                    RunSection2Example();
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("Выход из программы. До свидания!");
                    break;
                default:
                    Console.WriteLine("Ошибка: неизвестный пункт меню. Введите число от 0 до 5.");
                    break;
            }

            if (running)
            {
                Console.WriteLine();
            }
        }
    }

    // ======================= Методы консольного меню =======================

    private static void PrintMenu()
    {
        Console.WriteLine("==================================================================");
        Console.WriteLine("  Лабораторная работа №3. Вариант №7 — Слияние .txt-файлов");
        Console.WriteLine("==================================================================");
        Console.WriteLine("  1 — Объединить все .txt-файлы папки (потоковый режим StreamReader)");
        Console.WriteLine("  2 — Объединить .txt-файлы асинхронно (БОНУС: async/await)");
        Console.WriteLine("  3 — Авто-демонстрация (создать тестовые .txt-файлы и склеить их)");
        Console.WriteLine("  4 — Просмотреть содержимое итогового файла");
        Console.WriteLine("  5 — Пример из раздела 2 методички (статистика папки, топ-5 расширений)");
        Console.WriteLine("  0 — Выход");
        Console.WriteLine("------------------------------------------------------------------");
        Console.Write("Выберите действие (0-5): ");
    }

    /// <summary>
    /// Пункт 1: Синхронное потоковое слияние файлов.
    /// </summary>
    private static void RunSyncMerge()
    {
        Console.WriteLine("--- Слияние .txt-файлов (потоковый режим) ---");

        string folder = ReadPath("Введите путь к папке с .txt-файлами: ");
        string output = ReadOutputPath(folder, "Введите путь к итоговому файлу (Enter = merged_result.txt в этой же папке): ");
        Encoding encoding = PromptEncoding();
        bool recursive = PromptYesNo("Искать .txt-файлы также во вложенных подпапках? (д/н, Enter = н): ");

        ExecuteWithExceptionHandling(() =>
        {
            MergeResult result = FileMerger.MergeTxtFiles(folder, output, encoding, recursive);
            _lastOutputFilePath = result.OutputFilePath;
            PrintMergeResult(result, "Синхронный потоковый (StreamReader / StreamWriter)", encoding);
        });
    }

    /// <summary>
    /// Пункт 2: Бонусное задание — асинхронное слияние файлов (async / await).
    /// </summary>
    private static async Task RunAsyncMerge()
    {
        Console.WriteLine("--- Асинхронное слияние .txt-файлов (БОНУС: async / await) ---");

        string folder = ReadPath("Введите путь к папке с .txt-файлами: ");
        string output = ReadOutputPath(folder, "Введите путь к итоговому файлу (Enter = merged_async.txt в этой же папке): ", "merged_async.txt");
        Encoding encoding = PromptEncoding();
        bool recursive = PromptYesNo("Искать .txt-файлы также во вложенных подпапках? (д/н, Enter = н): ");

        using var cts = new CancellationTokenSource();

        await ExecuteAsyncWithExceptionHandling(async () =>
        {
            MergeResult result = await FileMerger.MergeTxtFilesAsync(folder, output, encoding, recursive, cts.Token);
            _lastOutputFilePath = result.OutputFilePath;
            PrintMergeResult(result, "Асинхронный потоковый (ReadLineAsync / WriteLineAsync)", encoding);
        });
    }

    /// <summary>
    /// Пункт 3: Автоматическое создание тестовых файлов и демонстрация слияния.
    /// </summary>
    private static void RunDemoWithSampleFiles()
    {
        Console.WriteLine("--- Авто-демонстрация работы программы ---");
        string demoFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestTxtFiles");
        string demoOutput = Path.Combine(demoFolder, "merged_result.txt");

        ExecuteWithExceptionHandling(() =>
        {
            var created = FileMerger.CreateSampleTxtFiles(demoFolder, Encoding.UTF8);
            Console.WriteLine($"Создана папка с тестовыми файлами: {demoFolder}");
            foreach (string path in created)
            {
                Console.WriteLine($"  + {Path.GetFileName(path)}");
            }

            MergeResult result = FileMerger.MergeTxtFiles(demoFolder, demoOutput, Encoding.UTF8, includeSubdirectories: false);
            _lastOutputFilePath = result.OutputFilePath;
            PrintMergeResult(result, "Авто-демонстрация (StreamReader / StreamWriter)", Encoding.UTF8);

            Console.WriteLine("\n--- Содержимое полученного итогового файла ---");
            Console.WriteLine(FileMerger.ReadFileContent(result.OutputFilePath, Encoding.UTF8));
            Console.WriteLine("----------------------------------------------");
        });
    }

    /// <summary>
    /// Пункт 4: Просмотр содержимого итогового файла.
    /// </summary>
    private static void ShowMergedFileContent()
    {
        Console.WriteLine("--- Просмотр итогового файла ---");
        string prompt = _lastOutputFilePath is not null
            ? $"Введите путь к файлу (Enter = {_lastOutputFilePath}): "
            : "Введите путь к файлу: ";

        string input = ReadPath(prompt);
        string targetPath = string.IsNullOrWhiteSpace(input) && _lastOutputFilePath is not null
            ? _lastOutputFilePath
            : input;

        Encoding encoding = PromptEncoding();

        ExecuteWithExceptionHandling(() =>
        {
            string content = FileMerger.ReadFileContent(targetPath, encoding);
            Console.WriteLine("\n================ СОДЕРЖИМОЕ ФАЙЛА ================");
            Console.WriteLine(content);
            Console.WriteLine("==================================================");
        });
    }

    /// <summary>
    /// Пункт 5: Пример из раздела 2 методички (статистика папки и топ-5 расширений).
    /// </summary>
    private static void RunSection2Example()
    {
        Console.WriteLine("--- Пример из раздела 2 методички (Статистика папки) ---");
        string folder = ReadPath("Введите путь к папке: ");

        ExecuteWithExceptionHandling(() =>
        {
            var (totalFiles, totalSizeBytes, topExtensions) = FileMerger.AnalyzeFolderStats(folder, topCount: 5);

            Console.WriteLine($"Файлов: {totalFiles}");
            Console.WriteLine($"Суммарный размер: {totalSizeBytes / 1024.0 / 1024.0:F2} МБ ({FileMerger.FormatSize(totalSizeBytes)})");
            Console.WriteLine("Топ-5 расширений:");

            foreach (var item in topExtensions)
            {
                Console.WriteLine($"  {item.Extension,-18} {item.Count,5} шт.   ({FileMerger.FormatSize(item.TotalSizeBytes)})");
            }
        });
    }

    // ======================= Вспомогательные методы UI =======================

    private static string ReadPath(string prompt)
    {
        Console.Write(prompt);
        // Убираем пробелы и кавычки (если путь скопирован через «Копировать как путь» в Проводнике Windows)
        return (Console.ReadLine() ?? string.Empty).Trim().Trim('"', '\'');
    }

    private static string ReadOutputPath(string sourceFolder, string prompt, string defaultFileName = "merged_result.txt")
    {
        string input = ReadPath(prompt);
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        if (string.IsNullOrWhiteSpace(sourceFolder))
        {
            return defaultFileName;
        }

        return Path.Combine(sourceFolder, defaultFileName);
    }

    /// <summary>
    /// БОНУСНОЕ ЗАДАНИЕ №3: Выбор кодировки чтения и записи (UTF-8, Windows-1251, CP866).
    /// </summary>
    private static Encoding PromptEncoding()
    {
        Console.Write("Выберите кодировку [1 - UTF-8 (по умолч.), 2 - Windows-1251, 3 - CP866]: ");
        string? input = Console.ReadLine()?.Trim();

        return input switch
        {
            "2" => Encoding.GetEncoding(1251),
            "3" => Encoding.GetEncoding(866),
            _ => Encoding.UTF8
        };
    }

    private static bool PromptYesNo(string prompt)
    {
        Console.Write(prompt);
        string answer = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
        return answer is "д" or "да" or "y" or "yes" or "1";
    }

    private static void PrintMergeResult(MergeResult result, string modeName, Encoding encoding)
    {
        Console.WriteLine();
        Console.WriteLine("Слияние файлов успешно завершено!");
        Console.WriteLine($"  Режим работы       : {modeName}");
        Console.WriteLine($"  Кодировка          : {encoding.WebName.ToUpperInvariant()}");
        Console.WriteLine($"  Объединено файлов  : {result.MergedFiles.Count}");
        Console.WriteLine($"  Всего записано строк: {result.TotalLinesWritten}");
        Console.WriteLine($"  Итоговый файл      : {result.OutputFilePath}");
        Console.WriteLine($"  Размер итогового   : {FileMerger.FormatSize(result.OutputSizeBytes)} ({result.OutputSizeBytes} байт)");
        Console.WriteLine("  Список объединённых .txt-файлов:");

        for (int i = 0; i < result.MergedFiles.Count; i++)
        {
            MergedFileInfo f = result.MergedFiles[i];
            Console.WriteLine($"    {i + 1}. {f.FileName,-25} | строк: {f.LineCount,4} | размер: {FileMerger.FormatSize(f.SizeBytes)}");
        }
    }

    /// <summary>
    /// КРИТЕРИЙ ОЦЕНКИ: Все ошибки обрабатываются.
    /// Исключения перехватываются от наиболее конкретных (FileNotFoundException, DirectoryNotFoundException,
    /// PathTooLongException) к базовому IOException и общему Exception.
    /// </summary>
    private static void ExecuteWithExceptionHandling(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[Ошибка ввода параметров]: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[Файлы не найдены]: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"[Папка не найдена]: {ex.Message}");
        }
        catch (PathTooLongException ex)
        {
            Console.WriteLine($"[Слишком длинный путь]: {ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("[Ошибка доступа]: Недостаточно прав для чтения папки/файлов или записи итогового файла.");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"[Неподдерживаемый формат пути]: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[Ошибка ввода-вывода]: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Непредвиденная ошибка]: {ex.Message}");
        }
    }

    private static async Task ExecuteAsyncWithExceptionHandling(Func<Task> asyncAction)
    {
        try
        {
            await asyncAction();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[Отмена]: Операция слияния файлов была отменена пользователем.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[Ошибка ввода параметров]: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[Файлы не найдены]: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"[Папка не найдена]: {ex.Message}");
        }
        catch (PathTooLongException ex)
        {
            Console.WriteLine($"[Слишком длинный путь]: {ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("[Ошибка доступа]: Недостаточно прав для чтения папки/файлов или записи итогового файла.");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"[Неподдерживаемый формат пути]: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[Ошибка ввода-вывода]: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Непредвиденная ошибка]: {ex.Message}");
        }
    }
}
