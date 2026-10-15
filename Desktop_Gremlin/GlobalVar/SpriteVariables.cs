public static class Settings
{
    public static int SpriteColumn { get; set; } = 0;
    public static int FrameRate { get; set; } = 60;
    public static int FrameWidth { get; set; } = 0;
    public static int FrameHeight { get; set; } = 0;
    public static int FrameWidthJs { get; set; } = 0;
    public static int FrameHeightJs { get; set; } = 0;
    public static int RandomMinInterval { get; set; } = 0;
    public static int RandomMaxInterval { get; set; } = 0;
    public static int MoveDistance { get; set; } = 0;
    public static int SleepTime { get; set; } = 0;
    public static int Ammo { get; set; } = 0;
    public static int CurrentAmmo { get; set; } = 0;
    public static int CurrendIdle { get; set; } = 0;
    public static int ItemWidth { get; set; } = 0;
    public static int ItemHeight { get; set; } = 0;
    public static int WalkDistance { get; set; } = 200;
    public static int SpawnDistance { get; set; } = 0;
    public static double FollowRadius { get; set; } = 0;
    public static double SpriteSize { get; set; } = 1.0;
    public static double CompanionScale { get; set; } = 1.0;
    public static double VolumeLevel { get; set; } = 1.0;
    public static double SvGravity { get; set; } = 10.0;
    public static string StartingChar { get; set; } = "";
    public static string FoodSpawn { get; set; } = "food1.png";  
    public static bool AllowRandomness { get; set; } = false;
    public static bool AllowGravity { get; set; } = false;
    public static bool FootStepSounds { get; set; } = false;
    public static bool AllowColoredHotSpot { get; set; } = false;
    public static bool ShowTaskBar { get; set; } = false;
    public static bool FakeTransparent { get; set; } = false;
    public static bool AllowErrorMessages { get; set; } = false;
    public static bool ManualReize { get; set; } = false;
    public static bool ForceCenter { get; set; } = false;
    public static bool EnableMinSize { get; set; } = false;
    public static bool ForceBottomSpawn { get; set; } = true;
    public static bool DisableHotspots { get; set; } = true;
    public static bool EnableGravity { get; set; } = false;
    public static bool LanguageDiff { get; set; } = true;
    public static bool AllowCache { get; set; } = false;
    public static bool AllowKeyboard { get; set; } = false;
    public static bool UseWPF { get; set; } = false; 
    public static bool RandomizeSpawn { get; set; } = false;
    public static bool ClickThrough { get; set; } = false;
    public static bool StraightLine { get; set; } = true;

}
public static class QuirkSettings
{
    public static int MaxItemAcceleration { get; set; } = 0;
    public static double ItemAcceleration { get; set; } = 1.0;
    public static double CurrentItemAcceleration { get; set; } = 1.0;
    public static string CompanionChar { get; set; } = "";
    public static double CompanionScale { get; set; } = 1.0;
    public static int CompanionColumn { get; set; } = 10;
    public static int CompanionHeight { get; set; } = 0;    
    public static int CompanionWidth { get; set; } = 0;   
    public static int CompanionFollow { get; set; } = 0;
}
public static class MouseSettings
{
    public static bool FollowCursor { get; set; } = false;
    public static System.Drawing.Point LastMousePosition { get; set; }
    public static double FollowSpeed { get; set; } = 10.0;
    public static double MouseX { get; set; }
    public static double MouseY { get; set; }
    public static double Speed { get; set; } = 20.0;
}
