using System;
using System.Globalization;
using System.IO;
using System.Threading;
using DesktopGremlin;

internal static partial class Program
{
    private static int _assertions;
    private static readonly string TestCharacter = "Regression_" + Guid.NewGuid().ToString("N");
    private static readonly string CharacterDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "SpriteSheet", "Gremlins", TestCharacter);
    private static readonly string CompanionDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "SpriteSheet", "Companions", TestCharacter);
    private static readonly string MasterPath = Path.Combine(Path.GetTempPath(), TestCharacter + ".txt");

    private static int Main()
    {
        CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
        CultureInfo originalUiCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Directory.CreateDirectory(CharacterDirectory);
            Directory.CreateDirectory(CompanionDirectory);
            foreach (string culture in new[] { "en-US", "de-DE", "fr-FR", "tr-TR", "es-ES" })
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                TestNumbers();
                TestMasterConfig();
                TestCharacterConfig();
                Console.WriteLine("PASS config loaders: " + culture);
            }
            TestPackValidation();
            Console.WriteLine("PASS character-pack validation and failure handling");
            TestMonitorBounds();
            Console.WriteLine("PASS monitor geometry");
            Console.WriteLine("PASS: " + _assertions + " regression assertions");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
            Directory.Delete(CharacterDirectory, true);
            Directory.Delete(CompanionDirectory, true);
            if (File.Exists(MasterPath)) File.Delete(MasterPath);
        }
    }

    private static void TestNumbers()
    {
        foreach (string input in new[] { "0.50", "0,50", " .5 ", " ,5 ", "5e-1" })
        {
            Assert(ConfigNumbers.TryParseDouble(input, out double value), "Must parse: " + input);
            Equal(0.5, value, "Decimal must not become 5 or 50: " + input);
        }
        Assert(ConfigNumbers.TryParseDouble("-100", out double offset), "Signed companion offset");
        Equal(-100, offset, "Negative offsets remain supported");
        Assert(ConfigNumbers.TryParseDouble("1,234", out double decimalValue), "Ungrouped comma decimal");
        Equal(1.234, decimalValue, "Single comma is always decimal, never grouping");
        foreach (string input in new[] { null, "", "abc", "NaN", "Infinity", "-Infinity",
            "1e9999", "1,234.56", "1.234,56", "1,2,3", "1 234", "$0.5", "(0.5)" })
        {
            Assert(!ConfigNumbers.TryParseDouble(input, out double ignored), "Reject invalid number: " + input);
        }
        foreach (string input in new[] { "0", "-0.5", "NaN", "Infinity" })
        {
            Assert(!ConfigNumbers.TryParseScale(input, out double ignored), "Reject invisible/invalid scale: " + input);
        }
        Assert(ConfigNumbers.TryParseScale("0,001", out double small), "Small positive scales stay valid");
        Equal(0.001, small, "No arbitrary minimum scale");
    }

    private static void TestMasterConfig()
    {
        foreach (bool languageDiff in new[] { true, false })
        foreach (bool flagFirst in new[] { true, false })
        foreach (string separator in new[] { ".", "," })
        {
            string flag = "LANGUAGE_DIFF=" + languageDiff + "\n";
            string settings = "sprite_scale=0" + separator + "5\n"
                + "VOLUME_LEVEL=0" + separator + "25\n"
                + "FOLLOW_RADIUS=12" + separator + "5\n"
                + "COMPANION_SCALE=0" + separator + "75\n"
                + "CURRENT_ACCELERATION=0" + separator + "3\n"
                + "COMPANION_FOLLOW=-100\nSPRITE_FRAMERATE=60\nWALK_DISTANCE=200\n";
            File.WriteAllText(MasterPath, flagFirst ? flag + settings : settings + flag);
            Settings.SpriteSize = 1;
            ConfigSettings.LoadMaster(MasterPath);
            Equal(0.5, Settings.SpriteSize, "Master scale independent of flag, order and locale");
            Equal(0.25, Settings.VolumeLevel, "Master volume");
            Equal(12.5, Settings.FollowRadius, "Master radius");
            Equal(0.75, QuirkSettings.CompanionScale, "Companion scale");
            Equal(0.3, QuirkSettings.ItemAcceleration, "Acceleration");
            Equal(-100, QuirkSettings.CompanionFollow, "Companion offset");
        }
        Settings.SpriteSize = 0.5;
        QuirkSettings.CompanionScale = 0.75;
        File.WriteAllText(MasterPath, "SPRITE_SCALE=0\nCOMPANION_SCALE=-1\nVOLUME_LEVEL=NaN\n"
            + "SPRITE_FRAMERATE=0\nWALK_DISTANCE=0\n");
        ConfigSettings.LoadMaster(MasterPath);
        Equal(0.5, Settings.SpriteSize, "Invalid scale preserves previous value");
        Equal(0.75, QuirkSettings.CompanionScale, "Invalid companion scale preserves previous value");
        Equal(0.25, Settings.VolumeLevel, "Non-finite volume preserves previous value");
        Equal(60, Settings.FrameRate, "Zero frame rate must not reach timer division");
        Equal(200, Settings.WalkDistance, "Zero walk steps must not reach movement division");
        Settings.FollowRadius = 12.5;
        Settings.SvGravity = 20;
        MouseSettings.Speed = 10;
        File.WriteAllText(MasterPath, "FOLLOW_RADIUS=-1\nGRAVITY_STRENGTH=-1\nSPRITE_SPEED=0\nVOLUME_LEVEL=1.5\n");
        ConfigSettings.LoadMaster(MasterPath);
        Equal(12.5, Settings.FollowRadius, "Reject negative follow radius");
        Equal(20, Settings.SvGravity, "Reject negative gravity strength");
        Equal(10, MouseSettings.Speed, "Reject zero movement speed");
        Equal(0.25, Settings.VolumeLevel, "Reject out-of-range volume");
        File.WriteAllText(MasterPath, "START_BUTTOM=false\n");
        ConfigSettings.LoadMaster(MasterPath);
        Assert(!Settings.ForceBottomSpawn, "Legacy bottom-spawn spelling remains accepted");
        File.WriteAllText(MasterPath, "START_BOTTOM=true\n");
        ConfigSettings.LoadMaster(MasterPath);
        Assert(Settings.ForceBottomSpawn, "Canonical bottom-spawn spelling works");
    }

    private static void TestCharacterConfig()
    {
        File.WriteAllText(Path.Combine(CharacterDirectory, "config.txt"),
            "scale=0,5\nintro=8\nidle=12\nrunidle=10\nwidth=100\nheight=120\ncolumn=10\npat=0\n");
        var frames = new FrameCounts();
        frames.LoadConfigChar(TestCharacter);
        Equal(0.5, Settings.SpriteSize, "Character decimal comma");
        Equal(8, frames.Intro, "Lowercase intro under Turkish locale");
        Equal(12, frames.Idle, "Lowercase idle under Turkish locale");
        Equal(10, frames.WalkIdle, "Lowercase runidle under Turkish locale");
        Equal(100, Settings.FrameWidth, "Character width");
        Equal(120, Settings.FrameHeight, "Character height");
        Equal(10, Settings.SpriteColumn, "Character columns");
        Equal(0, frames.Pat, "Optional animations may have zero frames");

        File.WriteAllText(Path.Combine(CharacterDirectory, "config.txt"),
            "SCALE=0\nWIDTH=0\nHEIGHT=-1\nCOLUMN=0\nIDLE=-10\n");
        ExpectPackFailure(() => frames.LoadConfigChar(TestCharacter), "WIDTH");
        Equal(0.5, Settings.SpriteSize, "Invalid pack does not mutate character scale");
        Equal(100, Settings.FrameWidth, "Reject zero frame width");
        Equal(120, Settings.FrameHeight, "Reject negative frame height");
        Equal(10, Settings.SpriteColumn, "Reject zero columns");
        Equal(12, frames.Idle, "Reject negative frame counts");

        File.WriteAllText(Path.Combine(CompanionDirectory, "config.txt"),
            "intro=7\nidle=11\nrunidle=9\nwidth=80\nheight=90\n");
        var companion = new FrameCounts();
        companion.LoadConfigCompanion(TestCharacter);
        Equal(7, companion.Intro, "Companion intro under Turkish locale");
        Equal(11, companion.Idle, "Companion idle under Turkish locale");
        Equal(9, companion.WalkIdle, "Companion runidle under Turkish locale");
        Equal(80, QuirkSettings.CompanionWidth, "Companion width");
        Equal(90, QuirkSettings.CompanionHeight, "Companion height");
    }

    private static void TestMonitorBounds()
    {
        // Left monitor, top taskbar, negative desktop coordinates.
        var left = new MovementBounds(-1920, 40, 0, 1080);
        Equal(-1800, left.ClampLeft(-1800, 300), "Keep character on left monitor");
        Equal(-300, left.ClampLeft(100, 300), "Clamp right edge using sprite size");
        Equal(-1920, left.ClampLeft(-2000, 300), "Clamp negative left edge");
        Equal(40, left.ClampTop(0, 300), "Respect top taskbar");
        Equal(780, left.ClampTop(1000, 300), "Respect bottom edge");
        Equal(780, left.Fall(775, 300, 20), "Gravity must not overshoot floor");
        Equal(780, left.Fall(780, 300, 20), "Gravity at floor is stable");
        Equal(800, left.Fall(800, 300, 20), "Gravity does not teleport upward");
        Equal(-1920, left.ClampLeft(-1000, 2500), "Oversized sprite anchors to monitor origin");

        // Above/right monitor with bounds already converted from pixels to DIPs.
        var scaled = new MovementBounds(1280, -720, 2560, 0);
        Equal(1500, scaled.ClampLeft(1500, 200), "Keep scaled monitor origin");
        Equal(-720, scaled.ClampTop(-900, 200), "Clamp above-monitor edge");
        Equal(-200, scaled.Fall(-205, 200, 20), "Negative-origin floor");
        Equal(-200, scaled.ClampTop(100, 200), "Clamp above-monitor bottom edge");
    }

    private static void Equal(double expected, double actual, string message)
    {
        Assert(Math.Abs(expected - actual) < 0.0000001, message + ": expected " + expected + ", got " + actual);
    }

    private static void Assert(bool condition, string message)
    {
        _assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
