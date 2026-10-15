using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopGremlin;
using DesktopGremlin.Quirks.Companion;

internal static class Program
{
    private static readonly string Character = "WpfRegression_" + Guid.NewGuid().ToString("N");
    private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string MainConfig = Path.Combine(BasePath, "config.txt");
    private static int _assertions;

    [STAThread]
    private static int Main()
    {
        byte[] originalConfig = File.Exists(MainConfig) ? File.ReadAllBytes(MainConfig) : null;
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        try
        {
            WritePack("Gremlins");
            WritePack("Companions");
            TestStartupFailure();
            TestSpriteDecoding();
            TestCompanionFailureIsolation(app);
            Console.WriteLine("PASS: " + _assertions + " Windows WPF assertions");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            app.Shutdown();
            if (originalConfig == null) File.Delete(MainConfig);
            else File.WriteAllBytes(MainConfig, originalConfig);
            foreach (string root in new[] { "Gremlins", "Companions" })
            {
                string directory = Pack(root);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }

    private static string Pack(string root) { return Path.Combine(BasePath, "SpriteSheet", root, Character); }

    private static void WritePack(string root)
    {
        Directory.CreateDirectory(Path.Combine(Pack(root), "Actions"));
        File.WriteAllText(Path.Combine(Pack(root), "config.txt"), "WIDTH=2\nHEIGHT=2\nCOLUMN=2\nIDLE=4\nINTRO=0\n");
        WriteSheet(Path.Combine(Pack(root), "Actions", "idle.png"));
    }

    private static void WriteSheet(string path)
    {
        byte[] pixels = new byte[4 * 4 * 4];
        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 4; x++)
        {
            int offset = (y * 4 + x) * 4;
            pixels[offset] = (byte)(x < 2 ? 10 : 40);
            pixels[offset + 1] = (byte)(y < 2 ? 20 : 50);
            pixels[offset + 2] = 90;
            pixels[offset + 3] = 255;
        }
        var bitmap = BitmapSource.Create(4, 4, 96, 96, PixelFormats.Bgra32, null, pixels, 16);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var stream = File.Create(path)) encoder.Save(stream);
    }

    private static void TestStartupFailure()
    {
        File.WriteAllText(MainConfig, "ALLOW_ERROR_MESSAGES=false\nSTART_CHAR=Missing_" + Character + "\n");
        using (var process = Process.Start(new ProcessStartInfo(Path.Combine(BasePath, "DesktopGremlin.exe"))
        {
            UseShellExecute = false,
            WorkingDirectory = BasePath
        }))
        {
            if (!process.WaitForExit(15000))
            {
                process.Kill();
                throw new Exception("Broken-pack startup must exit without hanging on dialogs or timers.");
            }
            Assert(process.ExitCode == 1, "App startup must handle missing packs with exit code 1");
        }
        Assert(File.ReadAllText(PackDiagnostics.LogPath).Contains("Missing_" + Character),
            "Startup failure logs the selected pack/path even with dialogs disabled");
    }

    private static void TestSpriteDecoding()
    {
        Settings.StartingChar = Character;
        Settings.AllowErrorMessages = false;
        var frames = new FrameCounts();
        frames.LoadConfigChar(Character);
        var image = new Image();
        string actions = Path.Combine(Pack("Gremlins"), "Actions");

        foreach (bool caching in new[] { false, true })
        {
            Settings.AllowCache = caching;
            image.Source = SpriteManager.Initialize(frames);
            Assert(SpriteManager.PlayAnimation("idle", "Actions", 3, 4, image) == 0, "Valid animation wraps");
            AssertPixel(image, 40, 50, "Crop uses the final frame's real pixels");
            for (int i = 0; i < 100; i++)
                Assert(SpriteManager.PlayAnimation("hover", "Actions", 0, 4, image) == -1, "Missing optional sheet is recoverable");
            AssertPixel(image, 10, 20, "Missing sheet displays verified idle fallback");
            Assert(SpriteManager.PlayAnimation("pat", "Actions", 0, 0, image) == -1, "Disabled animation is skipped");

            File.WriteAllText(Path.Combine(actions, "click.png"), "not a PNG");
            Assert(SpriteManager.PlayAnimation("click", "Actions", 0, 4, image) == -1, "Corrupt optional PNG is recoverable");
            AssertPixel(image, 10, 20, "Corrupt sheet displays idle fallback");
            WriteSheet(Path.Combine(actions, "sleep.png"));
            Assert(SpriteManager.PlayAnimation("sleep", "Actions", 0, 5, image) == -1,
                "Frame-count mismatch fails at frame zero rather than freezing later");

            File.Delete(Path.Combine(actions, "idle.png"));
            ExpectPackFailure(() => SpriteManager.Initialize(frames), "idle.png");
            File.WriteAllText(Path.Combine(actions, "idle.png"), "broken idle");
            ExpectPackFailure(() => SpriteManager.Initialize(frames), "idle.png");
            WriteSheet(Path.Combine(actions, "idle.png"));
            frames.Idle = 5;
            ExpectPackFailure(() => SpriteManager.Initialize(frames), "exceeds");
            frames.Idle = 4;
            image.Source = SpriteManager.Initialize(frames);
            AssertPixel(image, 10, 20, "Reinitialization reloads a repaired pack");
        }

        string[] diagnostics = File.ReadAllLines(PackDiagnostics.LogPath);
        Assert(diagnostics.Count(line => line.StartsWith("[", StringComparison.Ordinal)
            && line.Contains(Path.Combine(actions, "hover.png"))) == 1,
            "Repeated missing sheets produce one diagnostic per session across cache modes");
        Assert(!diagnostics.Any(line => line.Contains(Path.Combine(actions, "pat.png"))),
            "Intentionally disabled animation does not produce a missing-file diagnostic");

        Settings.FrameRate = 60;
        Settings.AllowRandomness = false;
        var states = new AnimationStates();
        states.LockState();
        var controller = new AnimationController(null, states, new CurrentFrames(), frames, image, new Random(1));
        controller.Start();
        Pump();
        controller.Stop();
        Assert(states.GetState("Idle") && !states.IsLocked, "Missing intro releases main animation state");

        QuirkSettings.CompanionChar = Character;
        var companionFrames = new FrameCounts();
        companionFrames.LoadConfigCompanion(Character);
        image.Source = SpriteManagerCompanion.Initialize(companionFrames);
        Assert(SpriteManagerCompanion.PlayAnimation("idle", "Actions", 3, 4, image) == 0,
            "Companion crop honors COLUMN=2 rather than hardcoded ten columns");
        AssertPixel(image, 40, 50, "Companion final frame's pixels");
        var companionStates = new AnimationStates();
        companionStates.LockState();
        var companionController = new CompanionAnimationController(null, companionStates,
            new CurrentFrames(), companionFrames, image);
        companionController.Start();
        Pump();
        companionController.Stop();
        Assert(companionStates.GetState("Idle") && !companionStates.IsLocked,
            "Missing companion intro releases its locked state");
    }

    private static void TestCompanionFailureIsolation(Application app)
    {
        File.WriteAllText(MainConfig, "ALLOW_ERROR_MESSAGES=false\nSTART_CHAR=" + Character
            + "\nSPRITE_FRAMERATE=60\nSLEEP_TIME=500\nSTART_BOTTOM=false\nCOMPANION_CHAR=Missing_" + Character + "\n");
        var main = new MainWindow { Left = 100, Top = 100, Opacity = 0, ShowActivated = false };
        app.MainWindow = main;
        main.Show();
        int windowCount = app.Windows.Count;
        for (int i = 0; i < 3; i++) main.ToggleCompanion();
        Assert(app.Windows.Count == windowCount, "Failed companion constructors leave no registered windows");
        Assert(main.IsVisible && !app.Dispatcher.HasShutdownStarted, "Missing companion does not shut down the main gremlin");
        QuirkSettings.CompanionChar = Character;
        main.ToggleCompanion();
        Pump();
        var companion = app.Windows.OfType<Companion>().Single();
        Assert(companion.IsVisible, "Valid companion can still be summoned after failed attempts");
        main.ToggleCompanion();
        Assert(!app.Windows.OfType<Companion>().Any(), "Closing companion removes its window");
        main.Close();
    }

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        timer.Tick += (s, e) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void AssertPixel(Image image, byte blue, byte green, string message)
    {
        var source = (BitmapSource)image.Source;
        byte[] pixels = new byte[16];
        source.CopyPixels(pixels, 8, 0);
        Assert(source.PixelWidth == 2 && source.PixelHeight == 2 && pixels[0] == blue && pixels[1] == green, message);
    }

    private static void ExpectPackFailure(Action action, string expected)
    {
        try { action(); }
        catch (CharacterPackException exception)
        {
            Assert(exception.Message.Contains(expected), "Required-sheet error names its cause: " + expected);
            return;
        }
        throw new Exception("Expected a required-sheet error: " + expected);
    }

    private static void Assert(bool condition, string message)
    {
        _assertions++;
        if (!condition) throw new Exception(message);
    }
}
