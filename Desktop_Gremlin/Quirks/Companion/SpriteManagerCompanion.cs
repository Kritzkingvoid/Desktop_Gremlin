using System.Windows.Controls;
using System.Windows.Media.Imaging;

public static class SpriteManagerCompanion
{
    private static SpriteSheetStore _store;

    public static BitmapSource Initialize(FrameCounts frames)
    {
        // A fresh store permits repaired packs and different companion selections
        // to load without carrying failures/cache entries from an earlier instance.
        _store = new SpriteSheetStore("Companions", QuirkSettings.CompanionChar,
            QuirkSettings.CompanionWidth, QuirkSettings.CompanionHeight, QuirkSettings.CompanionColumn);
        return _store.InitializeIdle(frames.Idle);
    }

    private static SpriteSheetStore Store => _store ?? (_store = new SpriteSheetStore("Companions",
        QuirkSettings.CompanionChar, QuirkSettings.CompanionWidth, QuirkSettings.CompanionHeight, QuirkSettings.CompanionColumn));

    public static int PlayAnimation(string sheetName, string actionType, int currentFrame, int frameCount, Image targetImage)
    {
        return Store.Play(sheetName, actionType, currentFrame, frameCount, targetImage);
    }

    public static BitmapImage GetSpriteSheet(string animationName, string actionType)
    {
        return Store.GetSpriteSheet(animationName, actionType);
    }
}
