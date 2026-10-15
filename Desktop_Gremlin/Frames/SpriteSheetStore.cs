using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

// A separate instance belongs to each selected main/companion pack. Failed
// sheets/layouts are remembered even when successful-sheet caching is disabled.
internal sealed class SpriteSheetStore
{
    private static readonly Dictionary<string, string> _fileNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["idle"] = "idle.png",
        ["idle2"] = "idle2.png",
        ["intro"] = "intro.png",
        ["runleft"] = "runLeft.png",
        ["runright"] = "runRight.png",
        ["runup"] = "runUp.png",
        ["rundown"] = "runDown.png",
        ["outro"] = "outro.png",
        ["grab"] = "grab.png",
        ["runidle"] = "runIdle.png",
        ["click"] = "click.png",
        ["hover"] = "hover.png",
        ["sleep"] = "sleep.png",
        ["fireleft"] = "fireLeft.png",
        ["fireright"] = "fireRight.png",
        ["reload"] = "reload.png",
        ["pat"] = "pat.png",
        ["upleft"] = "upLeft.png",
        ["upright"] = "upRight.png",
        ["downleft"] = "downLeft.png",
        ["downright"] = "downRight.png",
        ["walkleft"] = "walkLeft.png",
        ["walkright"] = "walkRight.png",
        ["walkdown"] = "walkDown.png",
        ["walkup"] = "walkUp.png",
        ["emote1"] = "emote1.png",
        ["emote2"] = "emote2.png",
        ["emote3"] = "emote3.png",
        ["emote4"] = "emote4.png",
        ["sleeping"] = "sleep.png",
        ["jumpscare"] = "jumpScare.png",
        ["poof"] = "poof.png"
    };
    private readonly string _directory;
    private readonly int _width;
    private readonly int _height;
    private readonly int _columns;
    private readonly Dictionary<string, BitmapImage> _cache = new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _failedSheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _failedLayouts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private BitmapSource _idleFrame;

    public SpriteSheetStore(string root, string character, int width, int height, int columns)
    {
        _directory = CharacterPack.GetDirectory(root, character);
        _width = width;
        _height = height;
        _columns = columns;
    }

    public BitmapSource InitializeIdle(int count)
    {
        BitmapImage sheet = Load("idle", "Actions", false, out string error);
        if (sheet == null) throw new CharacterPackException(error);
        if (!SpriteSheetLayout.TryGetFrame(sheet.PixelWidth, sheet.PixelHeight, _width, _height,
            _columns, count, 0, out int x, out int y, out int next, out error))
        {
            throw new CharacterPackException(LayoutError("idle", "Actions", count, sheet, error));
        }
        _idleFrame = new CroppedBitmap(sheet, new Int32Rect(x, y, _width, _height));
        _idleFrame.Freeze();
        return _idleFrame;
    }

    public int Play(string animation, string folder, int current, int count, Image target)
    {
        // Zero means an optional animation is disabled. Do not load/report it.
        if (count <= 0) return FallBack(target);
        string layoutKey = folder + "/" + animation + ":" + count;
        if (_failedLayouts.Contains(layoutKey)) return FallBack(target);
        BitmapImage sheet = Load(animation, folder, true, out string error);
        if (sheet == null) return FallBack(target);
        if (!SpriteSheetLayout.TryGetFrame(sheet.PixelWidth, sheet.PixelHeight, _width, _height,
            _columns, count, current, out int x, out int y, out int next, out error))
        {
            _failedLayouts.Add(layoutKey);
            PackDiagnostics.ReportOnce(LayoutError(animation, folder, count, sheet, error));
            return FallBack(target);
        }
        target.Source = new CroppedBitmap(sheet, new Int32Rect(x, y, _width, _height));
        return next;
    }

    public BitmapImage GetSpriteSheet(string animation, string folder)
    {
        return Load(animation, folder, true, out string error);
    }

    private int FallBack(Image target)
    {
        if (_idleFrame != null) target.Source = _idleFrame;
        return -1;
    }

    private string LayoutError(string animation, string folder, int count, BitmapImage sheet, string reason)
    {
        return "Invalid animation in " + _directory + "/" + folder + "/" + animation
            + ": " + reason + " Sheet=" + sheet.PixelWidth + "x" + sheet.PixelHeight
            + ", frame=" + _width + "x" + _height + ", columns=" + _columns + ", frames=" + count + ".";
    }

    private BitmapImage Load(string animation, string folder, bool report, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(animation) || !_fileNameMap.TryGetValue(animation, out string fileName))
        {
            error = "Unknown animation '" + animation + "' requested by pack " + _directory + ".";
            if (report) PackDiagnostics.ReportOnce(error);
            return null;
        }
        string path = Path.Combine(_directory, folder, fileName);
        if (Settings.AllowCache && _cache.TryGetValue(path, out BitmapImage cached)) return cached;
        if (_failedSheets.TryGetValue(path, out error))
        {
            if (report) PackDiagnostics.ReportOnce(error);
            return null;
        }
        try
        {
            // OnLoad detaches decoded data from the stream. StreamSource also
            // avoids WPF's URI cache when a pack is repaired and initialized again.
            using (var stream = File.OpenRead(path))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                if (Settings.AllowCache) _cache[path] = image;
                return image;
            }
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
            || exception is NotSupportedException || exception is FormatException
            || exception is ArgumentException || exception is InvalidOperationException)
        {
            error = "Cannot load sprite sheet: " + path + "\n" + exception.Message
                + "\nRestore the sheet and config.txt from the same character pack."
                + " Retry the companion or restart the app after repairing the pack.";
            _failedSheets[path] = error;
            if (report) PackDiagnostics.ReportOnce(error);
            return null;
        }
    }
}
