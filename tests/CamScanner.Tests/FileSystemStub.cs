namespace CamScanner.Data;

internal static class FileSystem
{
    public static string AppDataDirectory => Path.GetTempPath();
}