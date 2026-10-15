using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DesktopGremlin
{
    public static class MonitorWorkArea
    {
        public static MovementBounds GetFor(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            var source = PresentationSource.FromVisual(window);
            if (handle == IntPtr.Zero || source?.CompositionTarget == null)
            {
                Rect primary = SystemParameters.WorkArea;
                return new MovementBounds(primary.Left, primary.Top, primary.Right, primary.Bottom);
            }

            // Screen.WorkingArea is in pixels; Window.Left/Top are in DIPs.
            // Use the window's current monitor rather than the primary screen.
            var pixels = Screen.FromHandle(handle).WorkingArea;
            var transform = source.CompositionTarget.TransformFromDevice;
            var topLeft = transform.Transform(new Point(pixels.Left, pixels.Top));
            var bottomRight = transform.Transform(new Point(pixels.Right, pixels.Bottom));
            return new MovementBounds(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        }
    }
}
