using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lab3;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Console.WriteLine("Лабораторная работа №3. Вариант №7.");
        Console.WriteLine("Слияние всех .txt-файлов указанной папки.");

        bool isRunning = true;

        while (isRunning)
        {
            Console.WriteLine();
            Console.WriteLine("========== МЕНЮ ==========");
            Console.WriteLine("1 — Объединить .txt-файлы");
            Console.WriteLine("0 — Выйти");
            Console.Write("Ваш выбор: ");

            // При Ctrl+Z / окончании входного потока программа завершит работу.
            string choice = (Console.ReadLine() ?? "0").Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await MergeFilesFromMenu();
                    break;
                case "0":
                    isRunning = false;
                    break;
                default:
                    Console.WriteLine("Ошибка: введите 1 или 0.");
                    break;
            }
        }

        Console.WriteLine("Программа завершена.");
    }

    // Этот метод отвечает только за диалог с пользователем.
    // Само слияние выполняет отдельный класс FileMerger.
    private static async Task MergeFilesFromMenu()
    {
        try
        {
            Console.Write("Введите путь к папке с .txt-файлами: ");
            string sourceDirectory = ReadPath();

            Console.Write("Введите путь к итоговому файлу " +
                          $"(Enter — {FileMerger.DefaultOutputFileName} в исходной папке): ");
            string outputFilePath = ReadPath();

            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                outputFilePath = Path.Combine(
                    sourceDirectory,
                    FileMerger.DefaultOutputFileName);
            }

            if (File.Exists(outputFilePath) && !ConfirmOverwrite())
            {
                Console.WriteLine("Операция отменена. Существующий файл не изменён.");
                return;
            }

            int mergedFilesCount = await FileMerger.MergeTxtFilesAsync(
                sourceDirectory,
                outputFilePath);

            Console.WriteLine();
            Console.WriteLine("Слияние успешно завершено.");
            Console.WriteLine($"Объединено файлов: {mergedFilesCount}");
            Console.WriteLine($"Итоговый файл: {Path.GetFullPath(outputFilePath)}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка в параметрах: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Папка не найдена: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Файлы не найдены: {ex.Message}");
        }
        catch (PathTooLongException ex)
        {
            Console.WriteLine($"Слишком длинный путь: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Недостаточно прав доступа: {ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"Неподдерживаемый формат пути: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка ввода-вывода: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
        }
    }

    // Убираем кавычки, которые Windows добавляет при команде
    // «Копировать как путь».
    private static string ReadPath()
    {
        return (Console.ReadLine() ?? string.Empty)
            .Trim()
            .Trim('"', '\'');
    }

    private static bool ConfirmOverwrite()
    {
        while (true)
        {
            Console.Write("Итоговый файл уже существует. Перезаписать? (д/н, Enter — нет): ");
            string answer = (Console.ReadLine() ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            switch (answer)
            {
                case "д":
                case "да":
                case "y":
                case "yes":
                    return true;
                case "н":
                case "нет":
                case "n":
                case "no":
                case "":
                    return false;
                default:
                    Console.WriteLine("Введите «д» для перезаписи или «н» для отмены.");
                    break;
            }
        }
    }
}
