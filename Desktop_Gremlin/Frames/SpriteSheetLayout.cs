using System;

public static class SpriteSheetLayout
{
    public static bool TryGetFrame(int sheetWidth, int sheetHeight, int width, int height,
        int columns, int count, int current, out int x, out int y, out int next, out string error)
    {
        x = y = next = 0;
        error = null;
        if (width <= 0 || height <= 0 || columns <= 0 || count <= 0)
        {
            error = "Frame width, height, columns and active frame count must be positive.";
            return false;
        }

        // Validate the entire declared animation, not just the current crop. Long
        // arithmetic prevents malformed large configs from overflowing into a valid crop.
        long requiredWidth = (long)Math.Min(columns, count) * width;
        long requiredHeight = ((long)(count - 1) / columns + 1) * height;
        if (requiredWidth > sheetWidth || requiredHeight > sheetHeight)
        {
            error = "The declared frame count/grid exceeds the sprite sheet dimensions."
                + " Check WIDTH, HEIGHT, COLUMN and the animation's frame count against the matching pack.";
            return false;
        }

        int frame = current >= 0 && current < count ? current : 0;
        x = (frame % columns) * width;
        y = (frame / columns) * height;
        next = (frame + 1) % count;
        return true;
    }
}
