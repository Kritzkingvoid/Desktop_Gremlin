using System;

namespace DesktopGremlin
{
    // All values are in WPF device-independent units, including monitor origins.
    public struct MovementBounds
    {
        public double Left { get; }
        public double Top { get; }
        public double Right { get; }
        public double Bottom { get; }

        public MovementBounds(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public double ClampLeft(double left, double width)
        {
            return Math.Max(Left, Math.Min(left, Right - width));
        }

        public double ClampTop(double top, double height)
        {
            return Math.Max(Top, Math.Min(top, Bottom - height));
        }

        public double Fall(double top, double height, double strength)
        {
            double floor = Math.Max(Top, Bottom - height);
            // A sprite above the working area may still fall into it. Never
            // overshoot the floor, even when strength exceeds the distance left.
            return top < floor ? Math.Min(top + strength, floor) : top;
        }
    }
}
