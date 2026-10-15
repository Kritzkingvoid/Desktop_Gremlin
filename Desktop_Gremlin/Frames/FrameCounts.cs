using System.IO;
public class FrameCounts
{
    public int LeftFire { get; set; } = 0;
    public int RightFire { get; set; } = 0;
    public int Intro { get; set; } = 0;
    public int Idle { get; set; } = 0;
    public int Idle2 { get; set; } = 0;
    public int Outro { get; set; } = 0;
    public int Down { get; set; } = 0;
    public int Up { get; set; } = 0;
    public int Right { get; set; } = 0;
    public int Left { get; set; } = 0;
    public int UpLeft { get; set; } = 0;
    public int UpRight { get; set; } = 0;
    public int DownLeft { get; set; } = 0;
    public int DownRight { get; set; } = 0;
    public int Grab { get; set; } = 0;
    public int WalkIdle { get; set; } = 0;
    public int Click { get; set; } = 0;
    public int Dance { get; set; } = 0;
    public int Hover { get; set; } = 0;
    public int Sleep { get; set; } = 0;
    public int Reload { get; set; } = 0;
    public int Pat { get; set; } = 0;
    public int WalkLeft { get; set; } = 0;
    public int WalkRight { get; set; } = 0;
    public int WalkUp { get; set; } = 0;
    public int WalkDown { get; set; } = 0;
    public int Emote1 { get; set; } = 0;
    public int Emote2 { get; set; } = 0;
    public int Emote3 { get; set; } = 0;
    public int Emote4 { get; set; } = 0;
    public int JumpScare { get; set; } = 0;
    public int Poof { get; set; } = 0;


    public void LoadConfigChar(string character)
    {
        string path = Path.Combine(CharacterPack.GetDirectory("Gremlins", character), "config.txt");
        string[] lines = CharacterPack.ReadConfig(path, false);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                continue;

            var parts = line.Split('=');

            if (parts.Length != 2)
                continue;

            string key = parts[0].Trim().ToUpperInvariant();
            string value = parts[1].Trim();

            if(key == "SCALE")
            {
                if (ConfigNumbers.TryParseScale(value, out double spriteScale))
                {
                    Settings.SpriteSize = spriteScale;
                }
            }

            if (!ConfigNumbers.TryParseInt(value, out int intValue) || intValue < 0
                || ((key == "WIDTH" || key == "HEIGHT" || key == "COLUMN") && intValue == 0))
            {
                continue;
            }

            switch (key)
            {
                case "INTRO": Intro = intValue; break;
                case "IDLE": Idle = intValue; break;
                case "IDLE2": Idle2 = intValue; break;
                case "RUNUP": Up = intValue; break;
                case "RUNDOWN": Down = intValue; break;
                case "RUNLEFT": Left = intValue; break;
                case "RUNRIGHT": Right = intValue; break;
                case "UPLEFT": UpLeft = intValue; break;
                case "UPRIGHT": UpRight = intValue; break;
                case "DOWNLEFT": DownLeft = intValue; break;
                case "DOWNRIGHT": DownRight = intValue; break;
                case "OUTRO": Outro = intValue; break;
                case "GRAB": Grab = intValue; break;
                case "RUNIDLE": WalkIdle = intValue; break;
                case "CLICK": Click = intValue; break;
                case "HOVER": Hover = intValue; break;
                case "SLEEP": Sleep = intValue; break;
                case "FIREL": LeftFire = intValue; break;
                case "FIRER": RightFire = intValue; break;
                case "RELOAD": Reload = intValue; break;
                case "PAT": Pat = intValue; break;
                case "WALKLEFT": WalkLeft = intValue; break;
                case "WALKRIGHT": WalkRight = intValue; break;
                case "WALKUP": WalkUp = intValue; break;
                case "WALKDOWN": WalkDown = intValue; break;
                case "EMOTE1": Emote1 = intValue; break;
                case "EMOTE2": Emote2 = intValue; break;
                case "EMOTE3": Emote3 = intValue; break;
                case "EMOTE4": Emote4 = intValue; break;
                case "JUMPSCARE": JumpScare = intValue; break;
                case "POOF": Poof = intValue; break;
                case "WIDTH": Settings.FrameWidth = intValue; break;
                case "HEIGHT": Settings.FrameHeight = intValue; break;
                case "COLUMN": Settings.SpriteColumn = intValue; break;                 
            }
        }
    }
    public void LoadConfigCompanion(string character)
    {
        string path = Path.Combine(CharacterPack.GetDirectory("Companions", character), "config.txt");
        string[] lines = CharacterPack.ReadConfig(path, true);
        QuirkSettings.CompanionColumn = 10;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                continue;

            var parts = line.Split('=');

            if (parts.Length != 2)
                continue;

            string key = parts[0].Trim().ToUpperInvariant();
            string value = parts[1].Trim();

            if (!ConfigNumbers.TryParseInt(value, out int intValue) || intValue < 0
                || ((key == "WIDTH" || key == "HEIGHT" || key == "COLUMN") && intValue == 0))
            {
                continue;
            }

            switch (key)
            {
                case "INTRO": Intro = intValue; break;
                case "IDLE": Idle = intValue; break;
                case "IDLE2": Idle2 = intValue; break;
                case "RUNUP": Up = intValue; break;
                case "RUNDOWN": Down = intValue; break;
                case "RUNLEFT": Left = intValue; break;
                case "RUNRIGHT": Right = intValue; break;
                case "UPLEFT": UpLeft = intValue; break;
                case "UPRIGHT": UpRight = intValue; break;
                case "DOWNLEFT": DownLeft = intValue; break;
                case "DOWNRIGHT": DownRight = intValue; break;
                case "OUTRO": Outro = intValue; break;
                case "GRAB": Grab = intValue; break;
                case "RUNIDLE": WalkIdle = intValue; break;
                case "CLICK": Click = intValue; break;
                case "HOVER": Hover = intValue; break;
                case "SLEEP": Sleep = intValue; break;
                case "FIREL": LeftFire = intValue; break;
                case "FIRER": RightFire = intValue; break;
                case "RELOAD": Reload = intValue; break;
                case "PAT": Pat = intValue; break;
                case "WALKLEFT": WalkLeft = intValue; break;
                case "WALKRIGHT": WalkRight = intValue; break;
                case "WALKUP": WalkUp = intValue; break;
                case "WALKDOWN": WalkDown = intValue; break;
                case "EMOTE1": Emote1 = intValue; break;
                case "EMOTE2": Emote2 = intValue; break;
                case "EMOTE3": Emote3 = intValue; break;
                case "EMOTE4": Emote4 = intValue; break;
                case "JUMPSCARE": JumpScare = intValue; break;
                case "WIDTH": QuirkSettings.CompanionWidth = intValue; break;
                case "HEIGHT": QuirkSettings.CompanionHeight = intValue; break;
                case "COLUMN": QuirkSettings.CompanionColumn = intValue; break;
            }
        }
    }
    public static bool TryParseDoubleInvariant(string input, out double result)
    {
        return ConfigNumbers.TryParseDouble(input, out result);
    }
}
