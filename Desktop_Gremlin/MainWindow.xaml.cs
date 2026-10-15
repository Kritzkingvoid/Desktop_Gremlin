using DesktopGremlin.Quirks.Companion;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static ConfigManager;

namespace DesktopGremlin
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private int _windowTrasnparent = 0x20;
        private int _wsxLayer = 0x80000;
        private int _exlLayaer = -20;
        private Random _rng = new Random();

        private AnimationStates _gremlinState = new AnimationStates();
        private FrameCounts _frameCounts = new FrameCounts();
        private CurrentFrames _currentFrames = new CurrentFrames();

        private AnimationController _animationController;
        private MovementController _movementController;
        private TimerController _timerController;
        private HotspotController _hotspotController;
        private KeyboardController _keyboardController;
        private FoodFollower _foodFollower;
        private Companion _companionInstance;
        private AppConfig _config;

        private Target _currentFood;
        private bool _clickThrough = false;

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeConfig();
            MediaManager.PlaySound("intro.wav", Settings.StartingChar);
        }

        public void ToggleClickThrough()
        {
            _clickThrough = !_clickThrough;

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hwnd, _exlLayaer);
            style |= _wsxLayer;

            if (_clickThrough)
            {
                style = style | _windowTrasnparent;
            }
            else
            {
                style = style & (~_windowTrasnparent);
            }
            SetWindowLong(hwnd, _exlLayaer, style);
        }

        public void InitializeConfig()
        {
            ConfigManager.LoadMasterConfig();
            _frameCounts.LoadConfigChar(Settings.StartingChar);
            SpriteImage.Source = SpriteManager.Initialize(_frameCounts);
            ConfigManager.ApplyXamlSettings(this);
            _timerController = new TimerController(this, _gremlinState);
            _animationController = new AnimationController(this, _gremlinState, _currentFrames, _frameCounts, SpriteImage, _rng);
            _movementController = new MovementController(this, _gremlinState, _currentFrames, _frameCounts, SpriteImage, _rng);
            _hotspotController = new HotspotController(this, LeftHotspot, RightHotspot, TopHotspot, LeftDownHotspot, RightDownHotspot);
            _foodFollower = new FoodFollower(this, _gremlinState, _currentFrames, _frameCounts, SpriteImage);
            _config = new AppConfig(this, _gremlinState);

            if (Settings.AllowKeyboard)
            {
                _keyboardController = new KeyboardController(this, _gremlinState, _currentFrames, _frameCounts, _rng);
            }
            _gremlinState.LockState();
            _animationController.Start();
            _timerController.Start();
        }

        public static void ErrorClose(string errorMessage, string errorTitle, bool close)
        {
            if (Settings.AllowErrorMessages)
            {
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (close)
            {
                Application.Current.Shutdown();
            }
        }

        private void SpriteImage_RightClick(object sender, MouseButtonEventArgs e)
        {
            _timerController.ResetIdleTimer();
            _currentFrames.Click = 0;
            _gremlinState.UnlockState();
            _gremlinState.SetState("Click");
            MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
            _gremlinState.LockState();
        }

        private void SpriteImage_MouseEnter(object sender, MouseEventArgs e)
        {
            _keyboardController?.DisableKeyboardMovement();
            _gremlinState.SetState("Hover");
            if (_gremlinState.GetState("Hover"))
            {
                MediaManager.PlaySound("hover.wav", Settings.StartingChar, 5);
            }
        }

        private void SpriteImage_MouseLeave(object sender, MouseEventArgs e)
        {
            _keyboardController?.EnableKeyboardMovement();
            _gremlinState.SetState("Idle");
            _currentFrames.Hover = 0;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _movementController.StopRandomMove();
            _timerController.ResetIdleTimer();
            _gremlinState.UnlockState();
            _gremlinState.SetState("Grab");
            MediaManager.PlaySound("grab.wav", Settings.StartingChar);
            DragMove();
            _gremlinState.SetState("Idle");
            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
            if (MouseSettings.FollowCursor)
            {
                _gremlinState.SetState("Walking");
                _gremlinState.LockState();
            }
            _currentFrames.Grab = 0;
            if (MouseSettings.FollowCursor)
            {
                MediaManager.PlaySound("run.wav", Settings.StartingChar);
            }
        }

        private void TopHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            if (_gremlinState.GetState("FollowItem"))
            {
                return;
            }
            _gremlinState.UnlockState();
            _gremlinState.SetState("FollowItem");
            _gremlinState.LockState();

            SpawnFoodOnCurrentMonitor();
            _currentFood.Show();
            StartFollowingFood();
        }

        private void LeftHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFrames.Emote1 = 0;
            EmoteHelper("Emote1", "emote1.wav");
        }

        private void LeftDownHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFrames.Emote2 = 0;
            EmoteHelper("Emote2", "emote2.wav");
        }

        private void RightHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFrames.Emote3 = 0;
            EmoteHelper("Emote3", "emote3.wav");
        }

        private void RightDownHotspot_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFrames.Emote4 = 0;
            EmoteHelper("Emote4", "emote4.wav");
        }

        private void StartFollowingFood()
        {
            MediaManager.PlaySound("food.wav", Settings.StartingChar);
            _foodFollower.StartFollowing(_currentFood, QuirkSettings.CurrentItemAcceleration);
        }

        private void EmoteHelper(string emote, string mp3)
        {
            _timerController.ResetIdleTimer();
            _gremlinState.UnlockState();
            _gremlinState.SetState(emote);
            MediaManager.PlaySound(mp3, Settings.StartingChar);
            _gremlinState.LockState();
        }
        public void HotSpot()
        {
            _hotspotController.ToggleHotspots();
        }
        public void ShowHotSpot()
        {
            _hotspotController.ShowHotspots();
        }
        public void ToggleGravity()
        {
            _timerController.ToggleGravity();
        }
        public void TriggerClickAnimation()
        {
            _timerController.ResetIdleTimer();
            _currentFrames.Click = 0;
            _currentFrames.Idle = 0;
            _gremlinState.UnlockState();
            _gremlinState.SetState("Click");
            MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
            _gremlinState.LockState();
        }

        public void ToggleCursorFollow()
        {
            _keyboardController?.StopAllMovement();
            _movementController.StopRandomMove();

            MouseSettings.FollowCursor = !MouseSettings.FollowCursor;
            if (MouseSettings.FollowCursor)
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Walking");
                _gremlinState.LockState();
                MediaManager.PlaySound("run.wav", Settings.StartingChar);
            }
            else
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Idle");
            }
        }
        public void TriggerFood()
        {
            if (_gremlinState.GetState("FollowItem"))
            {
                return;
            }
            _gremlinState.UnlockState();
            _gremlinState.SetState("FollowItem");
            _gremlinState.LockState();

            SpawnFoodOnCurrentMonitor();
            _currentFood.Show();
            StartFollowingFood();
        }

        private void SpawnFoodOnCurrentMonitor()
        {
            MovementBounds workArea = MonitorWorkArea.GetFor(this);
            _currentFood = new Target();
            _currentFood.Left = workArea.ClampLeft(workArea.Left + _rng.NextDouble()
                * Math.Max(0, workArea.Right - workArea.Left - _currentFood.Width), _currentFood.Width);
            _currentFood.Top = workArea.ClampTop(workArea.Top + _rng.NextDouble()
                * Math.Max(0, workArea.Bottom - workArea.Top - _currentFood.Height), _currentFood.Height);
        }

        public void TriggerLeftEmote()
        {
            _currentFrames.Emote1 = 0;
            _currentFrames.Idle = 0;
            EmoteHelper("Emote1", "emote1.wav");
        }
        public void ForceShutDown()
        {
            Application.Current.Shutdown();
        }
        public void TriggerRightEmote()
        {
            _currentFrames.Emote3 = 0;
            _currentFrames.Idle = 0;
            EmoteHelper("Emote3", "emote3.wav");
        }

        public void TriggerLeftDownEmote()
        {
            _currentFrames.Emote2 = 0;
            _currentFrames.Idle = 0;
            EmoteHelper("Emote2", "emote2.wav");
        }
        public void TriggerRightDownEmote()
        {
            _currentFrames.Emote4 = 0;
            _currentFrames.Idle = 0;
            EmoteHelper("Emote4", "emote4.wav");
        }
        
        public void ToggleSleep()
        {
            if (_gremlinState.GetState("Sleeping"))
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Idle");
                _timerController.ResetIdleTimer();
            }
            else
            {
                _gremlinState.UnlockState();
                MediaManager.PlaySound("sleep.wav", Settings.StartingChar);
                _gremlinState.SetState("Sleeping");
                _gremlinState.LockState();
            }
        }
        public void ToggleCompanion()
        {
            if (_companionInstance != null && _companionInstance.IsVisible && QuirkSettings.CompanionChar != null)
            {
                _companionInstance.Close();
                return;
            }
            try
            {
                var companion = new Companion();
                companion.SetMainGremlin(this);
                companion.Closed += (s, args) => _companionInstance = null;
                _companionInstance = companion;
                companion.Show();
            }
            catch (CharacterPackException exception)
            {
                if (PackDiagnostics.ReportOnce(exception.Message))
                {
                    ErrorClose(exception.Message + "\nThe main gremlin will keep running.\nDiagnostics: "
                        + PackDiagnostics.LogPath, "Companion pack unavailable", false);
                }
            }
        }
        public void TriggerRandomMove()
        {
            _movementController.RandomMove();
        }
    }
}