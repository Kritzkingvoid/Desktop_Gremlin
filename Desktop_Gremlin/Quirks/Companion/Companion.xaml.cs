using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DesktopGremlin.Quirks.Companion
{
    public partial class Companion : Window
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        public static string characterChoice = "";
        public Window MainGremlin { get; set; }

        public struct POINT
        {
            public int X;
            public int Y;
        }

        private AnimationStates GremlinState = new AnimationStates();
        private CurrentFrames CurrentFrames = new CurrentFrames();
        private FrameCounts FrameCounts = new FrameCounts();

        private CompanionAnimationController _animationController;
        private CompanionFollowController _followController;
        public Companion()
        {
            InitializeComponent();
            Closed += (s, e) =>
            {
                _animationController?.Stop();
                _followController?.Stop();
            };
            try
            {
                InitializeCompanion();
            }
            catch (CharacterPackException)
            {
                Close();
                throw;
            }
        }

        private void InitializeCompanion()
        {
            FrameCounts.LoadConfigCompanion(QuirkSettings.CompanionChar);
            SpriteImage.Source = SpriteManagerCompanion.Initialize(FrameCounts);
            GremlinState.LockState();

            ApplyScaling();
            ApplyWindowSettings();

            _animationController = new CompanionAnimationController(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage);
            _followController = new CompanionFollowController(this, GremlinState, CurrentFrames, FrameCounts, SpriteImage);
           

            _animationController.SetFollowController(_followController);
            _animationController.Start();
            MediaManager.PlaySound("intro.wav", QuirkSettings.CompanionChar);
        }

        private void ApplyScaling()
        {
            this.Width = this.Width * QuirkSettings.CompanionScale;
            this.Height = this.Height * QuirkSettings.CompanionScale;
            SpriteImage.Width = SpriteImage.Width * QuirkSettings.CompanionScale;
            SpriteImage.Height = SpriteImage.Height * QuirkSettings.CompanionScale;
        }

        private void ApplyWindowSettings()
        {
            if (Settings.FakeTransparent)
            {
                this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#01000000"));
            }
            if (Settings.ManualReize)
            {
                this.SizeToContent = SizeToContent.Manual;
            }
            if (Settings.ForceCenter)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            if (Settings.EnableMinSize)
            {
                this.MinWidth = this.Width;
                this.MinHeight = this.Height;
            }
            if (Settings.ShowTaskBar)
            {
                this.ShowInTaskbar = true;
            }
        }

        public void SetMainGremlin(Window mainGremlin)
        {
            MainGremlin = mainGremlin;
            if (_followController != null)
            {
                _followController.SetMainGremlin(mainGremlin);
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GremlinState.UnlockState();
            GremlinState.SetState("Grab");
            MediaManager.PlaySound("grab.wav",QuirkSettings.CompanionChar);
            DragMove();
            GremlinState.SetState("Idle");
        }
    }
}
