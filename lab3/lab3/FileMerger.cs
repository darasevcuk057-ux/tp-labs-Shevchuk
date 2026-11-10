using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3;

// Логика слияния файлов вынесена в отдельный класс.
// Здесь нет Console.ReadLine/WriteLine — только работа с файлами.
public static class FileMerger
{
    public const string DefaultOutputFileName = "merged_result.txt";

    private const string Separator =
        "============================================================";

    // Бонусное задание №1: метод асинхронный.
    // Бонусное задание №2: файлы читаются построчно через StreamReader,
    // поэтому в память не загружаются целиком — можно сливать большие файлы.
    public static async Task<int> MergeTxtFilesAsync(
        string sourceDirectory,
        string outputFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException(
                "Путь к исходной папке не может быть пустым.",
                nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new ArgumentException(
                "Путь к итоговому файлу не может быть пустым.",
                nameof(outputFilePath));
        }

        string fullSourceDirectory = Path.GetFullPath(sourceDirectory.Trim());
        string fullOutputFilePath = Path.GetFullPath(outputFilePath.Trim());

        if (!Directory.Exists(fullSourceDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Папка не существует: {fullSourceDirectory}");
        }

        if (!string.Equals(
                Path.GetExtension(fullOutputFilePath),
                ".txt",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Итоговый файл должен иметь расширение .txt.",
                nameof(outputFilePath));
        }

        string? outputDirectory = Path.GetDirectoryName(fullOutputFilePath);

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException(
                "Не удалось определить папку для итогового файла.",
                nameof(outputFilePath));
        }

        // Если папки для итогового файла ещё нет, создаём её.
        Directory.CreateDirectory(outputDirectory);

        string[] inputFiles = Directory.GetFiles(
                fullSourceDirectory,
                "*.txt",
                SearchOption.TopDirectoryOnly)
            // Чтобы итоговый файл не объединялся сам с собой при повторном запуске.
            .Where(file => !string.Equals(
                Path.GetFullPath(file),
                fullOutputFilePath,
                StringComparison.OrdinalIgnoreCase))
            // Порядок объединения всегда одинаковый — по имени файла.
            .OrderBy(
                file => Path.GetFileName(file),
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (inputFiles.Length == 0)
        {
            throw new FileNotFoundException(
                "В указанной папке нет .txt-файлов для объединения.");
        }

        await WriteMergedFilesAsync(inputFiles, fullOutputFilePath);

        return inputFiles.Length;
    }

    // Построчное чтение и запись: память не забивается целыми файлами,
    // а асинхронные вызовы не блокируют поток на время дисковых операций.
    private static async Task WriteMergedFilesAsync(
        string[] inputFiles,
        string outputFilePath)
    {
        await using var writer = new StreamWriter(
            outputFilePath,
            false,
            new UTF8Encoding(false));

        foreach (string inputFile in inputFiles)
        {
            // Перед содержимым каждого файла — разделитель с его именем.
            await writer.WriteLineAsync(Separator);
            await writer.WriteLineAsync($"Файл: {Path.GetFileName(inputFile)}");
            await writer.WriteLineAsync(Separator);

            using var reader = new StreamReader(
                inputFile,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                await writer.WriteLineAsync(line);
            }

            // Пустая строка визуально отделяет следующий файл.
            await writer.WriteLineAsync();
        }
    }
}
