using System;
using System.Collections.Generic;
using System.IO;

public sealed class CharacterPackException : Exception
{
    public CharacterPackException(string message) : base(message) { }
}

public static class CharacterPack
{
    public static string GetDirectory(string rootFolder, string character)
    {
        if (string.IsNullOrWhiteSpace(character))
        {
            throw new CharacterPackException("No character is selected for " + rootFolder
                + ". Set START_CHAR or COMPANION_CHAR to an installed pack folder in the main config.txt.");
        }
        try
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpriteSheet", rootFolder, character);
        }
        catch (ArgumentException exception)
        {
            throw new CharacterPackException("Invalid character folder in START_CHAR / COMPANION_CHAR. " + exception.Message);
        }
    }

    public static string[] ReadConfig(string path, bool companion)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (IOException exception)
        {
            throw ReadFailure(path, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw ReadFailure(path, exception.Message);
        }
        catch (ArgumentException exception)
        {
            throw ReadFailure(path, exception.Message);
        }
        catch (NotSupportedException exception)
        {
            throw ReadFailure(path, exception.Message);
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length == 2) values[parts[0].Trim()] = parts[1].Trim();
        }
        RequirePositive(values, "WIDTH", path);
        RequirePositive(values, "HEIGHT", path);
        RequirePositive(values, "IDLE", path);
        // Legacy companion packs use a fixed ten-column grid when COLUMN is absent.
        if (!companion || values.ContainsKey("COLUMN")) RequirePositive(values, "COLUMN", path);
        return lines;
    }

    private static void RequirePositive(Dictionary<string, string> values, string key, string path)
    {
        if (!values.TryGetValue(key, out string text) || !ConfigNumbers.TryParseInt(text, out int value) || value <= 0)
        {
            throw new CharacterPackException("Invalid character config: " + path + "\n"
                + key + " must be a positive integer. Restore config.txt from the same pack as its sprite sheets.");
        }
    }

    private static CharacterPackException ReadFailure(string path, string reason)
    {
        return new CharacterPackException("Cannot read character config: " + path + "\n" + reason
            + "\nInstall the complete character pack and check START_CHAR / COMPANION_CHAR in the main config.txt.");
    }
}
