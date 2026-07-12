using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using MujiScreenSaver.Models;

namespace MujiScreenSaver.Services;

public sealed class TimerStore
{
    private readonly TimerStateCalculator _calculator;
    private readonly JsonSerializerOptions _jsonOptions;

    public TimerStore(TimerStateCalculator calculator, string? path = null)
    {
        _calculator = calculator;
        FilePath = path ?? GetDefaultPath();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new LenientEnumConverter<ThemeMode>(),
                new LenientEnumConverter<ColorPalette>()
            }
        };
    }

    public string FilePath { get; }

    public TimerSettingsFile Load()
    {
        if (!File.Exists(FilePath))
        {
            return _calculator.Normalize(null);
        }

        try
        {
            var json = File.ReadAllText(FilePath, Encoding.UTF8);
            var file = JsonSerializer.Deserialize<TimerSettingsFile>(json, _jsonOptions);
            return _calculator.Normalize(file);
        }
        catch (JsonException)
        {
            PreserveCorruptFile();
            return _calculator.Normalize(null);
        }
        catch (IOException)
        {
            return _calculator.Normalize(null);
        }
        catch (UnauthorizedAccessException)
        {
            return _calculator.Normalize(null);
        }
    }

    public void Save(TimerSettingsFile file)
    {
        var normalized = _calculator.Normalize(file);
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = FilePath + ".tmp";
        var backupPath = FilePath + ".bak";
        var json = JsonSerializer.Serialize(normalized, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(flushToDisk: true);
                }

                if (File.Exists(FilePath))
                {
                    try
                    {
                        File.Replace(tempPath, FilePath, backupPath, ignoreMetadataErrors: true);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(tempPath, FilePath, overwrite: true);
                        File.Delete(tempPath);
                    }
                    catch (IOException)
                    {
                        File.Copy(tempPath, FilePath, overwrite: true);
                        File.Delete(tempPath);
                    }
                }
                else
                {
                    File.Move(tempPath, FilePath);
                }

                return;
            }
            catch when (attempt == 0)
            {
                TryDelete(tempPath);
                Thread.Sleep(50);
            }
        }
    }

    public static string GetDefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "MujiScreenSaver", "timers.json");
    }

    private void PreserveCorruptFile()
    {
        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var extension = Path.GetExtension(FilePath);
            var stamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            var target = Path.Combine(directory ?? string.Empty, $"{fileName}.corrupt.{stamp}{extension}");

            if (File.Exists(FilePath))
            {
                File.Move(FilePath, target, overwrite: false);
            }
        }
        catch
        {
            // A corrupt file should not prevent the screensaver from starting.
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }
}

internal sealed class LenientEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String &&
            Enum.TryParse<TEnum>(reader.GetString(), ignoreCase: true, out var parsed) &&
            Enum.IsDefined(parsed))
        {
            return parsed;
        }

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numeric))
        {
            var numericValue = (TEnum)Enum.ToObject(typeof(TEnum), numeric);
            if (Enum.IsDefined(numericValue))
            {
                return numericValue;
            }
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var normalized = Enum.IsDefined(value) ? value : default;
        writer.WriteStringValue(normalized.ToString());
    }
}
