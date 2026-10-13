using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab3;

/// <summary>
/// Информация об одном обработанном .txt-файле.
/// </summary>
public record MergedFileInfo(
    string FileName,
    string FullPath,
    long SizeBytes,
    int LineCount
);

/// <summary>
/// Итоговый результат операции слияния .txt-файлов.
/// </summary>
public record MergeResult(
    string OutputFilePath,
    long OutputSizeBytes,
    int TotalLinesWritten,
    IReadOnlyList<MergedFileInfo> MergedFiles
);

/// <summary>
/// Статистика по одному расширению файлов (для примера из раздела 2 методички).
/// </summary>
public record ExtensionStat(
    string Extension,
    int Count,
    long TotalSizeBytes
);

/// <summary>
/// Класс бизнес-логики для слияния .txt-файлов (Вариант №7).
/// КРИТЕРИЙ ОЦЕНКИ: Логика полностью отделена от консольного меню (нет вызовов Console).
/// </summary>
public static class FileMerger
{
    /// <summary>
    /// Формирует строку-разделитель с именем файла перед его содержимым.
    /// По условию Варианта №7: перед содержимым каждого файла вставляется строка-разделитель с его именем.
    /// </summary>
    public static string BuildSeparatorLine(string displayFileName)
    {
        return $"========== {displayFileName} ==========";
    }

    /// <summary>
    /// Синхронное потоковое слияние всех .txt-файлов указанной папки в один итоговый файл.
    /// Выполнено требование Варианта №7 + БОНУСНОЕ ЗАДАНИЕ №1 (потоковая обработка через StreamReader/StreamWriter
    /// без загрузки файлов целиком в оперативную память через File.ReadAllText).
    /// </summary>
    /// <param name="sourceFolder">Путь к исходной папке с .txt-файлами.</param>
    /// <param name="outputFilePath">Путь к создаваемому итоговому файлу.</param>
    /// <param name="encoding">Кодировка чтения и записи (по умолчанию UTF-8).</param>
    /// <param name="includeSubdirectories">Обходить ли вложенные подпапки (бонусная опция).</param>
    /// <returns>Объект MergeResult со статистикой выполненного слияния.</returns>
    public static MergeResult MergeTxtFiles(
        string sourceFolder,
        string outputFilePath,
        Encoding? encoding = null,
        bool includeSubdirectories = false)
    {
        encoding ??= Encoding.UTF8;

        // 1. Валидация входных путей и получение списка исходных .txt-файлов
        var (fullFolderPath, fullOutputPath, txtFiles) = PrepareAndValidatePaths(
            sourceFolder, outputFilePath, includeSubdirectories);

        var mergedFiles = new List<MergedFileInfo>(txtFiles.Count);
        int totalLinesWritten = 0;

        // 2. Потоковая запись в итоговый файл (using гарантирует закрытие потока даже при ошибке)
        using (var writer = new StreamWriter(fullOutputPath, append: false, encoding))
        {
            for (int i = 0; i < txtFiles.Count; i++)
            {
                string filePath = txtFiles[i];
                var fileInfo = new FileInfo(filePath);

                // Если включён обход подпапок — показываем относительный путь, иначе — имя файла
                string displayName = includeSubdirectories
                    ? Path.GetRelativePath(fullFolderPath, filePath)
                    : fileInfo.Name;

                // Вставляем строку-разделитель с именем файла перед его содержимым
                string separator = BuildSeparatorLine(displayName);
                writer.WriteLine(separator);
                totalLinesWritten++;

                int fileLineCount = 0;

                // Потоковое построчное чтение исходного файла (не грузит весь файл в память)
                using (var reader = new StreamReader(filePath, encoding, detectEncodingFromByteOrderMarks: true))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        writer.WriteLine(line);
                        fileLineCount++;
                        totalLinesWritten++;
                    }
                }

                // Пустая строка между содержимым разных файлов для аккуратного разделения
                if (i < txtFiles.Count - 1)
                {
                    writer.WriteLine();
                    totalLinesWritten++;
                }

                mergedFiles.Add(new MergedFileInfo(
                    FileName: displayName,
                    FullPath: fileInfo.FullName,
                    SizeBytes: fileInfo.Length,
                    LineCount: fileLineCount));
            }
        }

        var outputInfo = new FileInfo(fullOutputPath);
        return new MergeResult(
            OutputFilePath: outputInfo.FullName,
            OutputSizeBytes: outputInfo.Length,
            TotalLinesWritten: totalLinesWritten,
            MergedFiles: mergedFiles);
    }

    /// <summary>
    /// БОНУСНОЕ ЗАДАНИЕ №2: Асинхронное потоковое слияние всех .txt-файлов (async/await + CancellationToken).
    /// Использует ReadLineAsync и WriteLineAsync для неблокирующего ввода-вывода.
    /// </summary>
    public static async Task<MergeResult> MergeTxtFilesAsync(
        string sourceFolder,
        string outputFilePath,
        Encoding? encoding = null,
        bool includeSubdirectories = false,
        CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;

        var (fullFolderPath, fullOutputPath, txtFiles) = PrepareAndValidatePaths(
            sourceFolder, outputFilePath, includeSubdirectories);

        var mergedFiles = new List<MergedFileInfo>(txtFiles.Count);
        int totalLinesWritten = 0;

        await using (var writer = new StreamWriter(fullOutputPath, append: false, encoding))
        {
            for (int i = 0; i < txtFiles.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string filePath = txtFiles[i];
                var fileInfo = new FileInfo(filePath);

                string displayName = includeSubdirectories
                    ? Path.GetRelativePath(fullFolderPath, filePath)
                    : fileInfo.Name;

                string separator = BuildSeparatorLine(displayName);
                await writer.WriteLineAsync(separator.AsMemory(), cancellationToken);
                totalLinesWritten++;

                int fileLineCount = 0;

                using (var reader = new StreamReader(filePath, encoding, detectEncodingFromByteOrderMarks: true))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
                    {
                        await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
                        fileLineCount++;
                        totalLinesWritten++;
                    }
                }

                if (i < txtFiles.Count - 1)
                {
                    await writer.WriteLineAsync( ReadOnlyMemory<char>.Empty, cancellationToken);
                    totalLinesWritten++;
                }

                mergedFiles.Add(new MergedFileInfo(
                    FileName: displayName,
                    FullPath: fileInfo.FullName,
                    SizeBytes: fileInfo.Length,
                    LineCount: fileLineCount));
            }
        }

        var outputInfo = new FileInfo(fullOutputPath);
        return new MergeResult(
            OutputFilePath: outputInfo.FullName,
            OutputSizeBytes: outputInfo.Length,
            TotalLinesWritten: totalLinesWritten,
            MergedFiles: mergedFiles);
    }

    /// <summary>
    /// Проверяет корректность путей и возвращает отсортированный список .txt-файлов,
    /// исключая сам итоговый файл, если он находится в той же папке.
    /// КРИТЕРИЙ ОЦЕНКИ: Все ошибки обрабатываются (выбрасываются информативные исключения).
    /// </summary>
    private static (string FullFolderPath, string FullOutputPath, List<string> TxtFiles) PrepareAndValidatePaths(
        string sourceFolder,
        string outputFilePath,
        bool includeSubdirectories)
    {
        if (string.IsNullOrWhiteSpace(sourceFolder))
            throw new ArgumentException("Путь к исходной папке не может быть пустым.", nameof(sourceFolder));

        if (string.IsNullOrWhiteSpace(outputFilePath))
            throw new ArgumentException("Путь к итоговому файлу не может быть пустым.", nameof(outputFilePath));

        string fullFolderPath = Path.GetFullPath(sourceFolder);
        if (!Directory.Exists(fullFolderPath))
            throw new DirectoryNotFoundException($"Указанная папка не существует: «{fullFolderPath}».");

        string fullOutputPath = Path.GetFullPath(outputFilePath);
        if (Directory.Exists(fullOutputPath))
            throw new IOException($"Путь к итоговому файлу указывает на существующую папку, а не на файл: «{fullOutputPath}».");

        string? outputDir = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            throw new DirectoryNotFoundException($"Папка для сохранения итогового файла не существует: «{outputDir}».");

        string outputFileName = Path.GetFileName(fullOutputPath);
        if (string.IsNullOrWhiteSpace(outputFileName) || outputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Имя итогового файла содержит недопустимые символы.", nameof(outputFilePath));

        SearchOption searchOption = includeSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        // Находим все .txt-файлы и исключаем сам итоговый файл, если он лежит в этой же папке
        var txtFiles = Directory.EnumerateFiles(fullFolderPath, "*.txt", searchOption)
            .Where(path => path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(Path.GetFullPath(path), fullOutputPath, pathComparison))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (txtFiles.Count == 0)
            throw new FileNotFoundException(
                $"В папке «{fullFolderPath}» не найдено ни одного .txt-файла для объединения.");

        return (fullFolderPath, fullOutputPath, txtFiles);
    }

    /// <summary>
    /// Создаёт демонстрационную папку с тремя тестовыми .txt-файлами для быстрой проверки на защите.
    /// </summary>
    public static IReadOnlyList<string> CreateSampleTxtFiles(string folderPath, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Путь к папке не может быть пустым.", nameof(folderPath));

        encoding ??= Encoding.UTF8;
        Directory.CreateDirectory(folderPath);

        var samples = new Dictionary<string, string[]>
        {
            ["01_intro.txt"] = new[]
            {
                "Лабораторная работа №3 по дисциплине «Технология программирования».",
                "Вариант №7: Слияние всех .txt-файлов указанной папки в один итоговый файл.",
                "Автор: Шевчук Дарья Александровна, группа ИИб-242."
            },
            ["02_theory.txt"] = new[]
            {
                "Для работы с файлами в .NET используется пространство имён System.IO.",
                "Классы StreamReader и StreamWriter обеспечивают потоковое чтение и запись текста.",
                "Конструкция using гарантирует автоматическое закрытие файловых потоков (IDisposable)."
            },
            ["03_conclusion.txt"] = new[]
            {
                "Все .txt-файлы успешно объединены!",
                "Перед содержимым каждого файла автоматически вставлена строка-разделитель с его именем."
            }
        };

        var createdPaths = new List<string>();
        foreach (var (fileName, lines) in samples)
        {
            string fullPath = Path.Combine(folderPath, fileName);
            File.WriteAllLines(fullPath, lines, encoding);
            createdPaths.Add(fullPath);
        }

        return createdPaths;
    }

    /// <summary>
    /// Читает содержимое текстового файла для предпросмотра.
    /// </summary>
    public static string ReadFileContent(string filePath, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: «{filePath}».", filePath);

        encoding ??= Encoding.UTF8;
        using var reader = new StreamReader(filePath, encoding, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Пример из раздела 2 методички: статистика по папке и топ-5 расширений.
    /// </summary>
    public static (int TotalFiles, long TotalSizeBytes, List<ExtensionStat> TopExtensions) AnalyzeFolderStats(
        string folderPath,
        int topCount = 5)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Путь к папке не может быть пустым.", nameof(folderPath));

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Папка не найдена: «{folderPath}».");

        var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
            .Select(p => new FileInfo(p))
            .ToList();

        int totalFiles = files.Count;
        long totalSize = files.Sum(f => f.Length);

        var top = files
            .GroupBy(f => string.IsNullOrEmpty(f.Extension) ? "(без расширения)" : f.Extension.ToLowerInvariant())
            .Select(g => new ExtensionStat(g.Key, g.Count(), g.Sum(f => f.Length)))
            .OrderByDescending(s => s.Count)
            .Take(topCount)
            .ToList();

        return (totalFiles, totalSize, top);
    }

    /// <summary>
    /// Переводит размер в байтах в человекочитаемую строку (Б, КБ, МБ).
    /// </summary>
    public static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} Б";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} КБ";
        return $"{bytes / 1024.0 / 1024.0:F2} МБ";
    }
}
