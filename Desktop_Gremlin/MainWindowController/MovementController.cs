using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static ConfigManager;

namespace DesktopGremlin
{
    public class MovementController
    {
        private MainWindow _window;
        private AnimationStates _gremlinState;
        private CurrentFrames _currentFrames;
        private FrameCounts _frameCounts;
        private Image _spriteImage;
        private Random _rng;
        private DispatcherTimer _activeRandomMoveTimer;

        public MovementController(MainWindow window, AnimationStates gremlinState, CurrentFrames currentFrames,
            FrameCounts frameCounts, Image spriteImage, Random rng)
        {
            _window = window;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _spriteImage = spriteImage;
            _rng = rng;
        }

        public void RandomMove()
        {
            StopRandomMove();
            _gremlinState.SetState("Random");

            double moveX = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;
            double moveY = (_rng.NextDouble() - 0.5) * Settings.MoveDistance * 2;

            MovementBounds workArea = MonitorWorkArea.GetFor(_window);
            double targetLeft = workArea.ClampLeft(_window.Left + moveX, _spriteImage.ActualWidth);
            double targetTop = workArea.ClampTop(_window.Top + moveY, _spriteImage.ActualHeight);

            int step = Settings.WalkDistance;
            double dx = (targetLeft - _window.Left) / step;
            double dy = (targetTop - _window.Top) / step;

            DispatcherTimer moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _activeRandomMoveTimer = moveTimer;

            int moveCount = 0;

            moveTimer.Tick += (s, e) =>
            {
                _window.Left += dx;
                _window.Top += dy;
                moveCount++;

                if (Settings.EnableGravity)
                {
                    dy = 0;
                }

                PlayWalkAnimation(dx, dy);

                if (moveCount >= step || !_gremlinState.GetState("Random"))
                {
                    moveTimer.Stop();
                    _gremlinState.SetState("Idle");
                    _activeRandomMoveTimer = null;
                }
            };
            moveTimer.Start();
        }

        public void StopRandomMove()
        {
            if (_activeRandomMoveTimer != null)
            {
                _activeRandomMoveTimer.Stop();
                _activeRandomMoveTimer = null;
            }
        }

        private void PlayWalkAnimation(double dx, double dy)
        {
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx > 0)
                {
                    _currentFrames.WalkRight = SpriteManager.PlayAnimation("walkRight", "Walk",
                        _currentFrames.WalkRight, _frameCounts.WalkRight, _spriteImage);
                }
                else
                {
                    _currentFrames.WalkLeft = SpriteManager.PlayAnimation("walkLeft", "Walk",
                        _currentFrames.WalkLeft, _frameCounts.WalkLeft, _spriteImage);
                }
            }
            else
            {
                if (dy > 0)
                {
                    _currentFrames.WalkDown = SpriteManager.PlayAnimation("walkDown", "Walk",
                        _currentFrames.WalkDown, _frameCounts.WalkDown, _spriteImage);
                }
                else
                {
                    _currentFrames.WalkUp = SpriteManager.PlayAnimation("walkUp", "Walk",
                        _currentFrames.WalkUp, _frameCounts.WalkUp, _spriteImage);
                }
            }
        }
    }
}