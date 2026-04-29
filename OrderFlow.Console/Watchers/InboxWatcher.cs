using System.Text.Json;
using System.Text.Json.Serialization;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Console.Watchers;

public class InboxWatcher : IDisposable
{
    private readonly FileSystemWatcher  _watcher;
    private readonly OrderPipeline      _pipeline;
    private readonly SemaphoreSlim      _semaphore = new(2);
    private readonly string             _inboxPath;
    private readonly HashSet<string>    _processedFiles = new();
    private readonly object             _hashLock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder              = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters           = { new JsonStringEnumConverter() }
    };

    public InboxWatcher(string inboxPath, OrderPipeline pipeline)
    {
        _inboxPath = inboxPath;
        _pipeline  = pipeline;

        Directory.CreateDirectory(inboxPath);
        Directory.CreateDirectory(Path.Combine(inboxPath, "processed"));
        Directory.CreateDirectory(Path.Combine(inboxPath, "failed"));

        _watcher = new FileSystemWatcher(inboxPath, "*.json")
        {
            NotifyFilter          = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents   = true
        };

        _watcher.Created += OnFileCreated;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        lock (_hashLock)
        {
            if (!_processedFiles.Add(e.FullPath))
            {
                System.Console.WriteLine($"  [INBOX] Pominięto duplikat: {Path.GetFileName(e.FullPath)}");
                return;
            }
        }
        _ = HandleFileAsync(e.FullPath);
    }

    private async Task HandleFileAsync(string filePath)
    {
        await _semaphore.WaitAsync();
        try
        {
            System.Console.WriteLine($"\n  [INBOX] Wykryto plik: {Path.GetFileName(filePath)}");

            var orders = await ReadWithRetryAsync(filePath);
            System.Console.WriteLine($"  [INBOX] Załadowano {orders.Count} zamówień z pliku");

            foreach (var order in orders)
                _pipeline.ProcessOrder(order);

            string dest = Path.Combine(_inboxPath, "processed", Path.GetFileName(filePath));
            File.Move(filePath, dest, overwrite: true);
            System.Console.WriteLine($"  [INBOX] ✓ Przeniesiono → processed/{Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"  [INBOX] ✗ Błąd: {ex.Message}");
            await MoveToFailedAsync(filePath, ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<Order>> ReadWithRetryAsync(string filePath, int maxRetries = 5)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var stream = new FileStream(
                    filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 4096, useAsync: true);

                var result = await JsonSerializer.DeserializeAsync<List<Order>>(stream, _jsonOptions);
                return result ?? new List<Order>();
            }
            catch (IOException) when (attempt < maxRetries)
            {
                int delay = Random.Shared.Next(200, 501);
                System.Console.WriteLine(
                    $"  [INBOX] Plik zajęty, retry {attempt}/{maxRetries} za {delay} ms...");
                await Task.Delay(delay);
            }
        }
        throw new IOException($"Nie udało się odczytać pliku po {maxRetries} próbach: {filePath}");
    }

    private async Task MoveToFailedAsync(string filePath, Exception ex)
    {
        try
        {
            string failedDir = Path.Combine(_inboxPath, "failed");
            string fileName  = Path.GetFileNameWithoutExtension(filePath);
            string destFile  = Path.Combine(failedDir, Path.GetFileName(filePath));
            string errorFile = Path.Combine(failedDir, $"{fileName}.error.txt");

            if (File.Exists(filePath))
                File.Move(filePath, destFile, overwrite: true);

            await File.WriteAllTextAsync(errorFile,
                $"[{DateTime.Now:s}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

            System.Console.WriteLine(
                $"  [INBOX] Przeniesiono do failed/ + zapisano {fileName}.error.txt");
        }
        catch (Exception moveEx)
        {
            System.Console.WriteLine($"  [INBOX] Nie udało się przenieść do failed/: {moveEx.Message}");
        }
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Created            -= OnFileCreated;
        _watcher.Dispose();
        _semaphore.Dispose();
    }
}
