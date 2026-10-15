using System;
using System.Collections.Generic;
using System.IO;

public static class ConfigSettings
{
    public static void LoadMaster(string path)
    {
        var settingsMap = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["LANGUAGE_DIFF"] = val => { if (bool.TryParse(val, out bool v)) Settings.LanguageDiff = v; },
            ["START_CHAR"] = val => Settings.StartingChar = val,
            ["FOOD_SPAWN"] = val => Settings.FoodSpawn = val,
            ["COMPANION_CHAR"] = val => QuirkSettings.CompanionChar = val,
            ["SPRITE_FRAMERATE"] = val => { if (ConfigNumbers.TryParseInt(val, out int v) && v > 0) Settings.FrameRate = v; },
            ["FOLLOW_RADIUS"] = val => { if (ConfigNumbers.TryParseDouble(val, out double v) && v >= 0) Settings.FollowRadius = v; },
            ["MAX_INTERVAL"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.RandomMaxInterval = v; },
            ["MIN_INTERVAL"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.RandomMinInterval = v; },
            ["RANDOM_MOVE_DISTANCE"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.MoveDistance = v; },
            ["ALLOW_RANDOM_ACTIONS"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowRandomness = v; },
            ["SLEEP_TIME"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.SleepTime = v; },
            ["ALLOW_FOOTSTEP_SOUNDS"] = val => { if (bool.TryParse(val, out bool v)) Settings.FootStepSounds = v; },
            ["AMMO"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.Ammo = v; },
            ["ALLOW_COLOR_HOTSPOT"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowColoredHotSpot = v; },
            ["SHOW_TASKBAR"] = val => { if (bool.TryParse(val, out bool v)) Settings.ShowTaskBar = v; },
            ["SPRITE_SCALE"] = val => { if (ConfigNumbers.TryParseScale(val, out double v)) Settings.SpriteSize = v; },
            ["FORCE_FAKE_TRANSPARENT"] = val => { if (bool.TryParse(val, out bool v)) Settings.FakeTransparent = v; },
            ["ALLOW_ERROR_MESSAGES"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowErrorMessages = v; },
            ["MAX_ACCELERATION"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) QuirkSettings.MaxItemAcceleration = v; },
            ["FOLLOW_ACCELERATION"] = val => { if (ConfigNumbers.TryParseDouble(val, out double v)) QuirkSettings.CurrentItemAcceleration = v; },
            ["CURRENT_ACCELERATION"] = val => { if (ConfigNumbers.TryParseDouble(val, out double v)) QuirkSettings.ItemAcceleration = v; },
            ["ITEM_WIDTH"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.ItemWidth = v; },
            ["ITEM_HEIGHT"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.ItemHeight = v; },
            ["COMPANIONS_SCALE"] = val => { if (ConfigNumbers.TryParseScale(val, out double v)) Settings.CompanionScale = v; },
            ["ENABLE_MIN_RESIZE"] = val => { if (bool.TryParse(val, out bool v)) Settings.EnableMinSize = v; },
            ["FORCE_CENTER"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceCenter = v; },
            ["ENABLE_MANUAL_RESIZE"] = val => { if (bool.TryParse(val, out bool v)) Settings.ManualReize = v; },
            ["VOLUME_LEVEL"] = val => { if (ConfigNumbers.TryParseDouble(val, out double v) && v >= 0 && v <= 1) Settings.VolumeLevel = v; },
            ["DISABLE_HOTSPOTS"] = val => { if (bool.TryParse(val, out bool v)) Settings.DisableHotspots = v; },
            ["START_BUTTOM"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceBottomSpawn = v; },
            ["START_BOTTOM"] = val => { if (bool.TryParse(val, out bool v)) Settings.ForceBottomSpawn = v; },
            ["ENABLE_GRAVITY"] = val => { if (bool.TryParse(val, out bool v)) Settings.EnableGravity = v; },
            ["GRAVITY_STRENGTH"] = val => { if (ConfigNumbers.TryParseDouble(val, out double v) && v >= 0) Settings.SvGravity = v; },
            ["ALLOW_CACHE"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowCache = v; },
            ["SPRITE_SPEED"] = val => { if (ConfigNumbers.TryParseDouble(val, out double v) && v > 0) MouseSettings.Speed = v; },
            ["ENABLE_KEYBOARD"] = val => { if (bool.TryParse(val, out bool v)) Settings.AllowKeyboard = v; },
            ["WALK_DISTANCE"] = val => { if (ConfigNumbers.TryParseInt(val, out int v) && v > 0) Settings.WalkDistance = v; },
            ["USE_WPFPLAYER"] = val => { if (bool.TryParse(val, out bool v)) Settings.UseWPF = v; },
            ["RANDOMIZE_SPAWN"] = val => { if (bool.TryParse(val, out bool v)) Settings.RandomizeSpawn = v; },
            ["STRAIGHT_MOVE"] = val => { if (bool.TryParse(val, out bool v)) Settings.StraightLine = v; },
            ["CLICK_THROUGH"] = val => { if (bool.TryParse(val, out bool v)) Settings.ClickThrough = v; },
            ["SPAWN_DISTANCE"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) Settings.SpawnDistance = v; },
            ["COMPANION_SCALE"] = val => { if (ConfigNumbers.TryParseScale(val, out double v)) QuirkSettings.CompanionScale = v; },
            ["COMPANION_FOLLOW"] = val => { if (ConfigNumbers.TryParseInt(val, out int v)) QuirkSettings.CompanionFollow = v; },
            ["STRAIGHT_LINE"] = val => { if (bool.TryParse(val, out bool v)) Settings.StraightLine = v; },
        };
        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
            {
                continue;
            }
            var parts = line.Split('=');

            if (parts.Length != 2)
            {
                continue;
            }
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            if (settingsMap.TryGetValue(key, out var setter))
            {
                setter(value);
            }
        }
    }
}
