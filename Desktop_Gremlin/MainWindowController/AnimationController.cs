using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static ConfigManager;

namespace DesktopGremlin
{
    public class AnimationController
    {
        private MainWindow _window;
        private AnimationStates _gremlinState;
        private CurrentFrames _currentFrames;
        private FrameCounts _frameCounts;
        private Image _spriteImage;
        private Random _rng;
        private DispatcherTimer _masterTimer;
        private DateTime _nextRandomActionTime;
        private bool _wasIdleLastFrame = false;
        public bool UseStraightMovementOnly { get; set; } = true;
        private bool _movingHorizontalFirst = true;

        public AnimationController(MainWindow window, AnimationStates gremlinState, CurrentFrames currentFrames, FrameCounts frameCounts, Image spriteImage, Random rng)
        {
            _window = window;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _spriteImage = spriteImage;
            _rng = rng;
            _nextRandomActionTime = DateTime.Now.AddSeconds(1);

            InitializeMasterTimer();
        }

        public void Start()
        {
            _masterTimer.Start();
        }

        public void Stop()
        {
            _masterTimer.Stop();
        }

        private void InitializeMasterTimer()
        {
            _masterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _masterTimer.Tick += MasterTimer_Tick;
        }

        private void MasterTimer_Tick(object sender, EventArgs e)
        {
            // Repeatable Animations
            _currentFrames.Grab = PlayAnimation("Grab", "Actions", _currentFrames.Grab, _frameCounts.Grab, false);
            _currentFrames.Emote1 = PlayAnimation("Emote1", "Emotes", _currentFrames.Emote1, _frameCounts.Emote1, false);
            _currentFrames.Emote3 = PlayAnimation("Emote3", "Emotes", _currentFrames.Emote3, _frameCounts.Emote3, false);
            _currentFrames.Idle = PlayAnimation("Idle", "Actions", _currentFrames.Idle, _frameCounts.Idle, false);
            _currentFrames.Hover = PlayAnimation("Hover", "Actions", _currentFrames.Hover, _frameCounts.Hover, false);
            _currentFrames.Sleep = PlayAnimation("Sleeping", "Actions", _currentFrames.Sleep, _frameCounts.Sleep, false);
            _currentFrames.Pat = PlayAnimation("Pat", "Actions", _currentFrames.Pat, _frameCounts.Pat, false);
            // Single Repeat Animations
            _currentFrames.Emote4 = PlayAnimation("Emote4", "Emotes", _currentFrames.Emote4, _frameCounts.Emote4, true);
            _currentFrames.Emote2 = PlayAnimation("Emote2", "Emotes", _currentFrames.Emote2, _frameCounts.Emote2, true);
            _currentFrames.Intro = PlayAnimation("Intro", "Actions", _currentFrames.Intro, _frameCounts.Intro, true);
            _currentFrames.Outro = PlayAnimation("Outro", "Actions", _currentFrames.Outro, _frameCounts.Outro, true);
            _currentFrames.Click = PlayAnimation("Click", "Actions", _currentFrames.Click, _frameCounts.Click, true);
            HandleCursorFollowing();
            HandleRandomActions();
        }

        private int PlayAnimation(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        {
            if (!_gremlinState.GetState(stateName))
            {
                return currentFrame;
            }

            currentFrame = SpriteManager.PlayAnimation(stateName, folder, currentFrame, frameCount, _spriteImage);

            if (currentFrame == -1)
            {
                _gremlinState.UnlockState();
                _gremlinState.ResetAllExceptIdle();
            }

            if (resetOnEnd && (currentFrame == 0 || currentFrame == -1) && stateName == "Outro")
            {
                Application.Current.Shutdown();
            }

            if (resetOnEnd && currentFrame == 0)
            {
                _gremlinState.UnlockState();
                _gremlinState.ResetAllExceptIdle();
            }

            return currentFrame;
        }

        private void HandleCursorFollowing()
        {
            if (!MouseSettings.FollowCursor || !_gremlinState.GetState("Walking"))
            {
                return;
            }

            MainWindow.POINT cursorPos;
            MainWindow.GetCursorPos(out cursorPos);
            var cursorScreen = new Point(cursorPos.X, cursorPos.Y);

            double halfW = _spriteImage.ActualWidth > 0 ? _spriteImage.ActualWidth / 2.0 : Settings.FrameWidth / 2.0;
            double halfH = _spriteImage.ActualHeight > 0 ? _spriteImage.ActualHeight / 2.0 : Settings.FrameHeight / 2.0;
            var spriteCenterScreen = _spriteImage.PointToScreen(new Point(halfW, halfH));

            var source = PresentationSource.FromVisual(_window);
            Matrix transformFromDevice = Matrix.Identity;

            if (source?.CompositionTarget != null)
            {
                transformFromDevice = source.CompositionTarget.TransformFromDevice;
            }

            var spriteCenterWpf = transformFromDevice.Transform(spriteCenterScreen);
            var cursorWpf = transformFromDevice.Transform(cursorScreen);
            double dx = cursorWpf.X - spriteCenterWpf.X;
            double dy = cursorWpf.Y - spriteCenterWpf.Y;

            if (Settings.EnableGravity)
            {
                dy = 0;
            }

            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > Settings.FollowRadius)
            {
                if (Settings.StraightLine)
                {
                    MoveStraightOnly(dx, dy, distance);
                }
                else
                {
                    MoveDiagonally(dx, dy, distance);
                }
            }
            else
            {
                PlayDirectionalAnimation("Idle");
            }
        }

        private void MoveDiagonally(double dx, double dy, double distance)
        {
            double step = Math.Min(MouseSettings.Speed, distance - Settings.FollowRadius);
            double nx = dx / distance;
            double ny = dy / distance;

            _window.Left += nx * step;
            _window.Top += ny * step;

            string dir = GetDirectionFromAngle(nx, ny, Settings.EnableGravity);
            PlayDirectionalAnimation(dir);
        }

        private void MoveStraightOnly(double dx, double dy, double distance)
        {
            double absDx = Math.Abs(dx);
            double absDy = Math.Abs(dy);
            double step = Math.Min(MouseSettings.Speed, distance - Settings.FollowRadius);

            bool moveHorizontal = false;
            bool moveVertical = false;
            double alignmentThreshold = 5.0;

            if (absDx < alignmentThreshold)
            {
                moveVertical = true;
            }
            else if (absDy < alignmentThreshold)
            {
                moveHorizontal = true;
            }
            else
            {
                if (_movingHorizontalFirst)
                {
                    moveHorizontal = true;
                }
                else
                {
                    moveVertical = true;
                }
                if (moveHorizontal && absDx < step * 2)
                {
                    _movingHorizontalFirst = false;
                }
                else if (moveVertical && absDy < step * 2)
                {
                    _movingHorizontalFirst = true;
                }
            }
            if (moveHorizontal)
            {
                double moveX = dx > 0 ? step : -step;
                if (absDx < step)
                {
                    moveX = dx;
                }
                _window.Left += moveX;

                string dir = dx > 0 ? "Right" : "Left";
                PlayDirectionalAnimation(dir);
            }
            else if (moveVertical)
            {
                double moveY = dy > 0 ? step : -step;
                if (absDy < step)
                {
                    moveY = dy;
                }
                _window.Top += moveY;

                string dir = dy > 0 ? "Down" : "Up";
                PlayDirectionalAnimation(dir);
            }
        }

        private void HandleRandomActions()
        {
            if (!Settings.AllowRandomness)
            {
                return;
            }

            bool isIdleNow = _gremlinState.IsCompletelyIdle();

            if (isIdleNow && !_wasIdleLastFrame)
            {
                int interval = _rng.Next(Settings.RandomMinInterval, Settings.RandomMaxInterval);
                _nextRandomActionTime = DateTime.Now.AddSeconds(interval);
            }

            if (isIdleNow && DateTime.Now >= _nextRandomActionTime)
            {
                _gremlinState.SetState("Random");

                int action = _rng.Next(0, 4);
                switch (action)
                {
                    case 0:
                        _currentFrames.Click = 0;
                        _gremlinState.UnlockState();
                        _gremlinState.SetState("Click");
                        MediaManager.PlaySound("mambo.wav", Settings.StartingChar);
                        _gremlinState.LockState();
                        break;
                    case 1:
                    case 2:
                    case 3:
                        _window.TriggerRandomMove();
                        break;
                }

                int intervalAfterAction = _rng.Next(Settings.RandomMinInterval, Settings.RandomMaxInterval);
                _nextRandomActionTime = DateTime.Now.AddSeconds(intervalAfterAction);
            }

            _wasIdleLastFrame = isIdleNow;
        }

        private string GetDirectionFromAngle(double dx, double dy, bool gravity)
        {
            if (gravity)
            {
                dy = 0;
            }

            double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
            if (angle < 0)
            {
                angle += 360;
            }

            if (gravity)
            {
                if (dx >= 0)
                {
                    return "Right";
                }
                else
                {
                    return "Left";
                }
            }

            if (angle >= 337.5 || angle < 22.5)
            {
                return "Right";
            }
            if (angle < 67.5)
            {
                return "DownRight";
            }
            if (angle < 112.5)
            {
                return "Down";
            }
            if (angle < 157.5)
            {
                return "DownLeft";
            }
            if (angle < 202.5)
            {
                return "Left";
            }
            if (angle < 247.5)
            {
                return "UpLeft";
            }
            if (angle < 292.5)
            {
                return "Up";
            }
            return "UpRight";
        }
        private void PlayDirectionalAnimation(string dir)
        {
            switch (dir)
            {
                case "Right":
                    _currentFrames.Right = SpriteManager.PlayAnimation("runRight", "Run", _currentFrames.Right, _frameCounts.Right, _spriteImage);
                    break;

                case "DownRight":
                    _currentFrames.DownRight = SpriteManager.PlayAnimation("downRight", "Run", _currentFrames.DownRight, _frameCounts.DownRight, _spriteImage);
                    break;

                case "Down":
                    _currentFrames.Down = SpriteManager.PlayAnimation("runDown", "Run", _currentFrames.Down, _frameCounts.Down, _spriteImage);
                    break;

                case "DownLeft":
                    _currentFrames.DownLeft = SpriteManager.PlayAnimation("downLeft", "Run", _currentFrames.DownLeft, _frameCounts.DownLeft, _spriteImage);
                    break;

                case "Left":
                    _currentFrames.Left = SpriteManager.PlayAnimation("runLeft", "Run", _currentFrames.Left, _frameCounts.Left, _spriteImage);
                    break;

                case "UpLeft":
                    _currentFrames.UpLeft = SpriteManager.PlayAnimation("upLeft", "Run", _currentFrames.UpLeft, _frameCounts.UpLeft, _spriteImage);
                    break;

                case "Up":
                    _currentFrames.Up = SpriteManager.PlayAnimation("runUp", "Run", _currentFrames.Up, _frameCounts.Up, _spriteImage);
                    break;

                case "UpRight":
                    _currentFrames.UpRight = SpriteManager.PlayAnimation("upRight", "Run", _currentFrames.UpRight, _frameCounts.UpRight, _spriteImage);
                    break;

                default:
                    _currentFrames.WalkIdle = SpriteManager.PlayAnimation("runIdle", "Actions", _currentFrames.WalkIdle, _frameCounts.WalkIdle, _spriteImage);
                    break;
            }
        }
    }
}