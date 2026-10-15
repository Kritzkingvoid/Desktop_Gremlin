using Desktop_Gremlin;
using DesktopGremlin;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;
public static class ConfigManager
{
    //TODO: Refactor this entire config manager to be more modular and easier to maintain.
    //No more giant switch statements.  
    public static void LoadMasterConfig()
    {
        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        try
        {
            ConfigSettings.LoadMaster(path);
        }
        catch (IOException exception)
        {
            throw new CharacterPackException("Cannot read the main config: " + path + "\n" + exception.Message
                + "\nExtract the complete app archive before launching it.");
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new CharacterPackException("Cannot read the main config: " + path + "\n" + exception.Message);
        }
    }

    public static bool TryParseDoubleInvariant(string input, out double result)
    {
        return ConfigNumbers.TryParseDouble(input, out result);
    }

    public static void ApplyXamlSettings(Window window)
    {
        if (window == null)
        {
            return;
        }
        Border LeftHotspot = window.FindName("LeftHotspot") as Border;
        Border LeftDownHotspot = window.FindName("LeftDownHotspot") as Border;
        Border RightHotspot = window.FindName("RightHotspot") as Border;
        Border RightDownHotspot = window.FindName("RightDownHotspot") as Border;
        Border TopHotspot = window.FindName("TopHotspot") as Border;
        Image SpriteImage = window.FindName("SpriteImage") as Image;
        if (LeftHotspot != null)
        {
            if (Settings.AllowColoredHotSpot && !Settings.DisableHotspots)
            {
                LeftHotspot.Background = new SolidColorBrush(Colors.Red);
                LeftDownHotspot.Background = new SolidColorBrush(Colors.Yellow);
                RightHotspot.Background = new SolidColorBrush(Colors.Blue);
                RightDownHotspot.Background = new SolidColorBrush(Colors.Orange);
                TopHotspot.Background = new SolidColorBrush(Colors.Purple);
            }
            else
            {
                var noColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
                LeftHotspot.Background = noColor;
                LeftDownHotspot.Background = noColor;
                RightHotspot.Background = noColor;
                RightDownHotspot.Background = noColor;
                TopHotspot.Background = noColor;
            }

            if (Settings.DisableHotspots)
            {
                LeftHotspot.IsEnabled = false;
                LeftDownHotspot.IsEnabled = false;
                RightDownHotspot.IsEnabled = false;
                RightHotspot.IsEnabled = false;
                TopHotspot.IsEnabled = false;
            }
        }
        window.ShowInTaskbar = Settings.ShowTaskBar;
        if (Settings.FakeTransparent)
        {
            window.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#01000000");
        }

        if (Settings.ManualReize)
        {
            window.SizeToContent = SizeToContent.Manual;
        }

        if (Settings.ForceCenter)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        if (SpriteImage == null)
        {
            return;
        }

        double originalWidth = SpriteImage.Width;
        double originalHeight = SpriteImage.Height;
        double newWidth = originalWidth * Settings.SpriteSize;
        double newHeight = originalHeight * Settings.SpriteSize;

        window.Width *= Settings.SpriteSize;
        window.Height *= Settings.SpriteSize;

        SpriteImage.Width = newWidth;
        SpriteImage.Height = newHeight;

        if (Settings.EnableMinSize)
        {
            window.MinWidth = window.Width;
            window.MinHeight = window.Height;
        }

        double centerX = (window.Width - newWidth) / 2;
        double centerY = (window.Height - newHeight) / 2;
        SpriteImage.Margin = new Thickness(centerX, centerY, 0, 0);

        ScaleHotspotSafe(LeftHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(LeftDownHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(RightHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(RightDownHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        ScaleHotspotSafe(TopHotspot, SpriteImage, centerX, centerY, newWidth / originalWidth, newHeight / originalHeight);
        if (Settings.ForceBottomSpawn)
        {
            window.Left = (SystemParameters.WorkArea.Width - window.Width) / 2;
            window.Top = SystemParameters.WorkArea.Bottom - window.Height;
        }
        if (Settings.RandomizeSpawn)
        {
            SpawnNearCenter(window);
        }
    }
    private static void ScaleHotspotSafe(Border hotspot, Image sprite, double centerX, double centerY, double scaleX, double scaleY)
    {
        if (hotspot == null || sprite == null) return;

        double offsetX = hotspot.Margin.Left - sprite.Margin.Left;
        double offsetY = hotspot.Margin.Top - sprite.Margin.Top;

        hotspot.Width *= scaleX;
        hotspot.Height *= scaleY;
        hotspot.Margin = new Thickness(centerX + offsetX * scaleX, centerY + offsetY * scaleY, 0, 0);
    }
    private static void SpawnNearCenter(Window window)
    {
        if (window == null)
        {
            return;
        }

        Rect workArea = SystemParameters.WorkArea;

        double centerX = workArea.Left + (workArea.Width - window.Width) / 2;
        double centerY = workArea.Top + (workArea.Height - window.Height) / 2;

        Random rng = new Random();

        double offsetX = rng.Next(-Settings.SpawnDistance, Settings.SpawnDistance + 1);
        double offsetY = rng.Next(-Settings.SpawnDistance, Settings.SpawnDistance + 1);

        double left = centerX + offsetX;
        double top = centerY + offsetY;


        left = Math.Max(workArea.Left,Math.Min(left, workArea.Right - window.Width));

        top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - window.Height));

        window.Left = left;
        window.Top = top;
    }
    public class AppConfig
    {
        private MainWindow _gremlin;
        private NotifyIcon _trayIcon;
        public AnimationStates _states;
        public AppConfig(MainWindow gremlin, AnimationStates states)
        {

            _gremlin = gremlin;
            _states = states;
            SetupTrayIcon();
        }
        public void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon();
            
            if (File.Exists($"SpriteSheet/Gremlins/{Settings.StartingChar}/Misc/" +
                $"{Settings.StartingChar}.ico"))
            {
                _trayIcon.Icon = new Icon($"SpriteSheet/Gremlins/{Settings.StartingChar}/Misc/" +
                $"{Settings.StartingChar}.ico");
            }
            else
            {
                _trayIcon.Icon = SystemIcons.Application;
            }

            _trayIcon.Visible = true;
            _trayIcon.Text = "Gremlin";

            var menu = new ContextMenuStrip();
            menu.Items.Add("Stylish Close", null, (s, e) => CloseApp());
            menu.Items.Add("Force Close", null, (s, e) => ForceClose());
            menu.Items.Add("Restart", null, (s, e) => RestartApp());
            menu.Items.Add(new ToolStripSeparator());

            var disableHotspotsItem = menu.Items.Add("Disable Hotspots");
            var showHotspotsItem = menu.Items.Add("Show Hotspots");
            var enableGravity = menu.Items.Add("Toggle Gravity");
            var enableClickThrough = menu.Items.Add("Toggle Click Through");
            var disableItem = disableHotspotsItem as ToolStripMenuItem;
            var showItem = showHotspotsItem as ToolStripMenuItem;
            var gravity = enableGravity as ToolStripMenuItem; 
            disableItem.Click += (s, e) =>
            {
                disableItem.Checked = !disableItem.Checked;
                _gremlin.HotSpot();
            };

            showItem.Click += (s, e) =>
            {
                showItem.Checked = !showItem.Checked;
                _gremlin.ShowHotSpot();
            };
            enableClickThrough.Click += (s, e) =>
            {
                _gremlin.ToggleClickThrough();
            };
            gravity.Click += (s, e) =>
            {
                _gremlin.ToggleGravity();  
            };
            _trayIcon.ContextMenuStrip = menu;
        }

        public void CloseApp()
        {
            _states.PlayOutro();
            MediaManager.PlaySound("outro.wav", Settings.StartingChar);
        }
        private void ForceClose()
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void RestartApp()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exePath);
            System.Windows.Application.Current.Shutdown();
        }
    }
}



