using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class OnceDiagnostics
{
    private readonly HashSet<string> _reported = new HashSet<string>(StringComparer.Ordinal);
    private readonly Action<string> _write;

    public OnceDiagnostics(Action<string> write) { _write = write; }

    public bool Report(string message)
    {
        if (!_reported.Add(message)) return false;
        _write(message);
        return true;
    }
}

public static class PackDiagnostics
{
    private static readonly OnceDiagnostics Reports = new OnceDiagnostics(Write);
    public static string LogPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopGremlin", "diagnostics.log");

    public static bool ReportOnce(string message) { return Reports.Report(message); }

    private static void Write(string message)
    {
        Trace.WriteLine(message);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
            File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("O") + "] " + message + Environment.NewLine);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (System.Security.SecurityException) { }
        // Diagnostics must not turn a recoverable pack problem into another crash.
    }
}
