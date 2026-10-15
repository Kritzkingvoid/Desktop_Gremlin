using System;
using System.Collections.Generic;
using System.IO;

internal static partial class Program
{
    private static void ExpectPackFailure(Action action, string expected)
    {
        try { action(); }
        catch (CharacterPackException exception)
        {
            Assert(exception.Message.Contains(expected), "Pack error must name the failing field/path: " + expected);
            return;
        }
        throw new InvalidOperationException("Expected a character-pack failure containing: " + expected);
    }

    private static void TestPackValidation()
    {
        string path = Path.Combine(CharacterDirectory, "config.txt");
        foreach (string key in new[] { "WIDTH", "HEIGHT", "COLUMN", "IDLE" })
        foreach (string bad in new[] { null, "", "0", "-1", "abc", "NaN", "2147483648" })
        {
            var lines = new List<string>();
            foreach (string other in new[] { "WIDTH", "HEIGHT", "COLUMN", "IDLE" })
            {
                if (other != key) lines.Add(other + "=2");
                else if (bad != null) lines.Add(other + "=" + bad);
            }
            File.WriteAllLines(path, lines);
            ExpectPackFailure(() => new FrameCounts().LoadConfigChar(TestCharacter), key);
        }
        ExpectPackFailure(() => new FrameCounts().LoadConfigChar("Missing_" + TestCharacter), "config.txt");
        ExpectPackFailure(() => new FrameCounts().LoadConfigCompanion("Missing_" + TestCharacter), "config.txt");
        ExpectPackFailure(() => new FrameCounts().LoadConfigChar(""), "START_CHAR");

        // A directory where a file should be is also a readable diagnostic, not an unhandled I/O exception.
        string directoryAsFile = Path.Combine(CharacterDirectory, "directory-config");
        Directory.CreateDirectory(directoryAsFile);
        ExpectPackFailure(() => CharacterPack.ReadConfig(directoryAsFile, false), directoryAsFile);
        ExpectPackFailure(() => CharacterPack.ReadConfig(path + "\0", false), "Cannot read character config");

        string companionPath = Path.Combine(CompanionDirectory, "config.txt");
        File.WriteAllText(companionPath, "WIDTH=2\nHEIGHT=2\nIDLE=4\nCOLUMN=2\n");
        new FrameCounts().LoadConfigCompanion(TestCharacter);
        Equal(2, QuirkSettings.CompanionColumn, "Honor companion column metadata");
        File.WriteAllText(companionPath, "WIDTH=2\nHEIGHT=2\nIDLE=4\n");
        new FrameCounts().LoadConfigCompanion(TestCharacter);
        Equal(10, QuirkSettings.CompanionColumn, "Old companions default to ten columns");
        File.AppendAllText(companionPath, "COLUMN=0\n");
        ExpectPackFailure(() => new FrameCounts().LoadConfigCompanion(TestCharacter), "COLUMN");

        // Preserve the loader's last-value-wins rule and ignore unknown settings.
        File.WriteAllText(path, "WIDTH=bad\nWIDTH=2\nHEIGHT=2\nIDLE=4\nCOLUMN=2\nCUSTOM=yes\n");
        var frames = new FrameCounts();
        frames.LoadConfigChar(TestCharacter);
        Equal(2, Settings.FrameWidth, "Validate effective duplicate value");
        Equal(4, frames.Idle, "Load required idle count");

        foreach (int current in new[] { -1, 0, 1, 2, 3, 4, int.MaxValue })
        {
            Assert(SpriteSheetLayout.TryGetFrame(4, 4, 2, 2, 2, 4, current,
                out int x, out int y, out int next, out string error), "Valid four-frame grid");
            int frame = current >= 0 && current < 4 ? current : 0;
            Equal((frame % 2) * 2, x, "Safe frame X");
            Equal((frame / 2) * 2, y, "Safe frame Y");
            Equal((frame + 1) % 4, next, "Advance/wrap frame");
        }
        Assert(!SpriteSheetLayout.TryGetFrame(4, 4, 2, 2, 2, 5, 0,
            out int ignoredX, out int ignoredY, out int ignoredNext, out string layoutError),
            "Reject overdeclared count before reaching its bad frame");
        Assert(layoutError.Contains("exceeds"), "Explain frame-count/sheet mismatch");
        foreach (int invalid in new[] { 0, -1 })
        {
            Assert(!SpriteSheetLayout.TryGetFrame(4, 4, invalid, 2, 2, 4, 0,
                out ignoredX, out ignoredY, out ignoredNext, out layoutError), "Reject invalid width");
            Assert(!SpriteSheetLayout.TryGetFrame(4, 4, 2, invalid, 2, 4, 0,
                out ignoredX, out ignoredY, out ignoredNext, out layoutError), "Reject invalid height");
            Assert(!SpriteSheetLayout.TryGetFrame(4, 4, 2, 2, invalid, 4, 0,
                out ignoredX, out ignoredY, out ignoredNext, out layoutError), "Reject invalid columns");
            Assert(!SpriteSheetLayout.TryGetFrame(4, 4, 2, 2, 2, invalid, 0,
                out ignoredX, out ignoredY, out ignoredNext, out layoutError), "Reject invalid active count");
        }
        Assert(!SpriteSheetLayout.TryGetFrame(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            int.MaxValue, int.MaxValue, 0, out ignoredX, out ignoredY, out ignoredNext, out layoutError),
            "Huge malformed grids cannot overflow into an apparently valid crop");
        Assert(SpriteSheetLayout.TryGetFrame(2, 2, 2, 2, 10, 1, 0,
            out ignoredX, out ignoredY, out ignoredNext, out layoutError),
            "A one-frame sheet may be narrower than the declared maximum columns");

        var messages = new List<string>();
        var diagnostics = new OnceDiagnostics(messages.Add);
        for (int tick = 0; tick < 1000; tick++) diagnostics.Report("Missing sprite: pack/Actions/hover.png");
        Equal(1, messages.Count, "A repeated 60fps failure produces one report");
        Assert(diagnostics.Report("Missing sprite: pack/Actions/click.png"), "A distinct failure is still reported");
        Equal(2, messages.Count, "Distinct errors are not suppressed together");
    }
}
