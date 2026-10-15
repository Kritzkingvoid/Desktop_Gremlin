using DesktopGremlin;
using DesktopGremlin.Quirks.Companion;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static ConfigManager;


    public class KeyboardController
    {
        private readonly MainWindow _gremlin;
        private readonly AnimationStates _gremlinState;
        private readonly CurrentFrames _currentFrames;
        private readonly FrameCounts _frameCounts;
        private readonly Random _rng;

        private bool _isKeyboardMoving = false;
        private DispatcherTimer _keyboardMoveTimer;
        private double _keyboardMoveSpeedX = 0;
        private double _keyboardMoveSpeedY = 0;
        private double KEY_MOVE_SPEED = MouseSettings.Speed;
        private bool _keyboardControlEnabled = false;

        public bool IsKeyboardMoving => _isKeyboardMoving;

        public KeyboardController(MainWindow gremlin, AnimationStates gremlinState, CurrentFrames currentFrames, FrameCounts frameCounts, Random rng)
        {
            _gremlin = gremlin;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _rng = rng;

            InitializeKeyboardTimer();
            AttachKeyboardEvents();
        }

        private void InitializeKeyboardTimer()
        {
            _keyboardMoveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _keyboardMoveTimer.Tick += KeyboardMove_Tick;
        }

        private void AttachKeyboardEvents()
        {
            _gremlin.KeyDown += Gremlin_KeyDown;
            _gremlin.KeyUp += Gremlin_KeyUp;
            _gremlin.Focusable = true;
            _gremlin.Focus();
        }
        private void Gremlin_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.W || e.Key == Key.Up || e.Key == Key.S || e.Key == Key.Down ||
                e.Key == Key.A || e.Key == Key.Left ||
                e.Key == Key.D || e.Key == Key.Right)
            {
                HandleMovementKeyDown(e.Key);
                return;
            }
            switch (e.Key)
            {
                case Key.Space:
                    _gremlin.TriggerClickAnimation();
                    break;
                case Key.C:
                    _gremlin.ToggleGravity();
                break;
                case Key.E:
                    _gremlin.ToggleCursorFollow();
                    break;

                case Key.Q:
                    _gremlin.TriggerFood();
                    break;
                case Key.D1:
                case Key.NumPad1:
                    _gremlin.TriggerLeftEmote();
                    break;
                case Key.D2:
                case Key.NumPad2:
                    _gremlin.TriggerRightEmote();
                    break;
                case Key.D3:
                case Key.NumPad3:
                    _gremlin.TriggerLeftDownEmote();
                    break;
                case Key.D4:
                case Key.NumPad4:
                    _gremlin.TriggerRightDownEmote();
                    break;
                case Key.D5:
                case Key.NumPad5:
                    _gremlin.ToggleCompanion();
                    break;
                case Key.R:
                    _gremlin.HotSpot();
                    break;
                case Key.T:
                    _gremlin.ToggleSleep();
                    break;
                case Key.Escape:
                    StopAllMovement();
                    break;
                case Key.F1:
                    ShowKeyboardHelp();
                    break;
                case Key.X:
                    _gremlin.ForceShutDown();
                    break;
                case Key.D0:
                    _gremlin.ToggleClickThrough();
                    break;
        }
        }
        private void Gremlin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W || e.Key == Key.Up || e.Key == Key.S || e.Key == Key.Down ||
                e.Key == Key.A || e.Key == Key.Left ||
                e.Key == Key.D || e.Key == Key.Right)
            {
                HandleMovementKeyUp(e.Key);
            }

        }
        private void HandleMovementKeyDown(Key key)
        {
            if (!_keyboardControlEnabled)
            {
                return;
            }
            if (MouseSettings.FollowCursor)
            {
                MouseSettings.FollowCursor = false;
                _gremlinState.UnlockState();
            }

            bool wasMoving = _isKeyboardMoving;
            switch (key)
            {
                case Key.W:
                case Key.Up:
                    if (!Settings.EnableGravity)
                    {
                        _keyboardMoveSpeedY = -KEY_MOVE_SPEED;
                    }
                    break;
                case Key.S:
                case Key.Down:
                    if (!Settings.EnableGravity)
                    {
                        _keyboardMoveSpeedY = KEY_MOVE_SPEED;
                    }
                    break;
                case Key.A:
                case Key.Left:
                    _keyboardMoveSpeedX = -KEY_MOVE_SPEED;
                    break;
                case Key.D:
                case Key.Right:
                    _keyboardMoveSpeedX = KEY_MOVE_SPEED;
                    break;
            }

            _isKeyboardMoving = (_keyboardMoveSpeedX != 0 || _keyboardMoveSpeedY != 0);

            if (_isKeyboardMoving && !wasMoving)
            {
                _gremlinState.UnlockState();
                _gremlinState.SetState("Random");
                _keyboardMoveTimer.Start();
            }
        }
        private void HandleMovementKeyUp(Key key)
        {
            if (!_keyboardControlEnabled)
            {
                return;
            }
            switch (key)
            {
                case Key.W:
                case Key.Up:
                    if (_keyboardMoveSpeedY < 0) _keyboardMoveSpeedY = 0;
                    break;
                case Key.S:
                case Key.Down:
                    if (_keyboardMoveSpeedY > 0) _keyboardMoveSpeedY = 0;
                    break;
                case Key.A:
                case Key.Left:
                    if (_keyboardMoveSpeedX < 0) _keyboardMoveSpeedX = 0;
                    break;
                case Key.D:
                case Key.Right:
                    if (_keyboardMoveSpeedX > 0) _keyboardMoveSpeedX = 0;
                    break;
            }

            bool wasMoving = _isKeyboardMoving;
            _isKeyboardMoving = (_keyboardMoveSpeedX != 0 || _keyboardMoveSpeedY != 0);

            if (!_isKeyboardMoving && wasMoving)
            {
                _keyboardMoveTimer.Stop();
                _gremlinState.SetState("Idle");
            }
        }
        private void KeyboardMove_Tick(object sender, EventArgs e)
        {
            double newLeft = _gremlin.Left + _keyboardMoveSpeedX;
            double newTop = _gremlin.Top + _keyboardMoveSpeedY;
            MovementBounds workArea = MonitorWorkArea.GetFor(_gremlin);
            newLeft = workArea.ClampLeft(newLeft, _gremlin.SpriteImage.ActualWidth);
            newTop = workArea.ClampTop(newTop, _gremlin.SpriteImage.ActualHeight);
            _gremlin.Left = newLeft;
            _gremlin.Top = newTop;

            double angle = Math.Atan2(_keyboardMoveSpeedY, _keyboardMoveSpeedX) * (180 / Math.PI);
            if (angle < 0) angle += 360;
           
            if (angle >= 337.5 || angle < 22.5)
            {
                _currentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run",
                    _currentFrames.Right, _frameCounts.Right, _gremlin.SpriteImage);
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                _currentFrames.DownRight = SpriteManager.PlayAnimation("downRight", "Run",
                    _currentFrames.DownRight, _frameCounts.DownRight, _gremlin.SpriteImage);
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                _currentFrames.Down = SpriteManager.PlayAnimation("runDown", "Run",
                    _currentFrames.Down, _frameCounts.Down, _gremlin.SpriteImage);
            }
            else if (angle >= 112.5 && angle < 157.5)
            {
                _currentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft", "Run",
                    _currentFrames.DownLeft, _frameCounts.DownLeft, _gremlin.SpriteImage);
            }
            else if (angle >= 157.5 && angle < 202.5)
            {
                _currentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run",
                    _currentFrames.Left, _frameCounts.Left, _gremlin.SpriteImage);
            }
            else if (angle >= 202.5 && angle < 247.5)
            {
                _currentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft", "Run",
                    _currentFrames.UpLeft, _frameCounts.UpLeft, _gremlin.SpriteImage);
            }
            else if (angle >= 247.5 && angle < 292.5)
            {
                _currentFrames.Up = SpriteManager.PlayAnimation("runUp", "Run",
                    _currentFrames.Up, _frameCounts.Up, _gremlin.SpriteImage);
            }
            else if (angle >= 292.5 && angle < 337.5)
            {
                _currentFrames.UpRight = SpriteManager.PlayAnimation("upRight", "Run",
                    _currentFrames.UpRight, _frameCounts.UpRight, _gremlin.SpriteImage);
            }                     
        }
        public void StopAllMovement()
        {
            _keyboardMoveSpeedX = 0;
            _keyboardMoveSpeedY = 0;
            _isKeyboardMoving = false;
            _keyboardMoveTimer.Stop();

            if (!_gremlinState.GetState("Sleeping"))
            {
                _gremlinState.SetState("Idle");
            }
        }
        public void DisableKeyboardMovement()
        {
            _keyboardControlEnabled = false;

            _keyboardMoveSpeedX = 0;
            _keyboardMoveSpeedY = 0;
            _isKeyboardMoving = false;
            if (_keyboardMoveTimer.IsEnabled)
            {
                _keyboardMoveTimer.Stop();
            }

            _gremlinState.SetState("Idle");
        }

        public void EnableKeyboardMovement()
        {
            _keyboardControlEnabled = true;
        }
    private void ShowKeyboardHelp()
    {
        string helpText = @"KEYBOARD CONTROLS:

            MOVEMENT (Disabled in Combat Mode):
                WASD / Arrow Keys - Move character
                E - Toggle cursor following
                R - Toggle HotSpot
                ESC - Stop all movement

            ACTIONS:
                SPACE - Click animation
                T - Toggle sleep/wake
                C - Toggle Gravity                  
                Q - Spawn Food
                1 - Emote 1 
                2 - Emote 3 
                3 - Emote 2 
                4 - Emote 4 
                5 - Un/Summon Companion
            HELP:
                F1 - Show this help
                X - Close Program
                0/Zero - Disable Hitbox //Non-Mouse Interactable";             
        MessageBox.Show(helpText, "Desktop Gremlin - Keyboard Controls",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }


}
