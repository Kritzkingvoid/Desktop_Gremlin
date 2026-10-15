using DesktopGremlin.Quirks.Companion;
using System;
using System.Windows.Controls;
using System.Windows.Threading;
using static ConfigManager;

namespace DesktopGremlin
{
    public class CompanionAnimationController
    {
        private Companion _companion;
        private AnimationStates _gremlinState;
        private CurrentFrames _currentFrames;
        private FrameCounts _frameCounts;
        private Image _spriteImage;
        private DispatcherTimer _masterTimer;
        private CompanionFollowController _followController;

        public CompanionAnimationController(Companion companion, AnimationStates gremlinState,
            CurrentFrames currentFrames, FrameCounts frameCounts, Image spriteImage)
        {
            _companion = companion;
            _gremlinState = gremlinState;
            _currentFrames = currentFrames;
            _frameCounts = frameCounts;
            _spriteImage = spriteImage;

            InitializeMasterTimer();
        }
        private void InitializeMasterTimer()
        {
            _masterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Settings.FrameRate) };
            _masterTimer.Tick += MasterTimer_Tick;
        }
        private void MasterTimer_Tick(object sender, EventArgs e)
        {
            _currentFrames.Idle = PlayAnimationIfActive("Idle", "Actions", _currentFrames.Idle, _frameCounts.Idle, false);
            _currentFrames.Grab = PlayAnimationIfActive("Grab", "Actions", _currentFrames.Grab, _frameCounts.Grab, false);
            _currentFrames.Intro = PlayAnimationIfActive("Intro", "Actions", _currentFrames.Intro, _frameCounts.Intro, true);

            if (!_gremlinState.GetState("Grab") && !_gremlinState.GetState("Intro") && _followController != null)
            {
                _followController.FollowMainGremlin();
            }
        }
        public void SetFollowController(CompanionFollowController followController)
        {
            _followController = followController;
        }

        public void Start()
        {
            _masterTimer.Start();
        }

        public void Stop()
        {
            _masterTimer.Stop();
        }

        private int PlayAnimationIfActive(string stateName, string folder, int currentFrame, int frameCount, bool resetOnEnd)
        {
            if (!_gremlinState.GetState(stateName))
            {
                return currentFrame;
            }

            currentFrame = SpriteManagerCompanion.PlayAnimation(stateName, folder, currentFrame, frameCount, _spriteImage);

            if (currentFrame == -1 || (resetOnEnd && currentFrame == 0))
            {
                _gremlinState.UnlockState();
                _gremlinState.ResetAllExceptIdle();
            }
            return currentFrame;
        }
    }
}
