# Character-pack reliability

## What failed before

- Missing or unreadable sprite sheets returned `null` without naming the broken
  file. The main controller could reset its animation state without ever showing
  an image, leaving an apparently invisible character.
- Invalid active frame counts called the app-wide shutdown method. Other layout
  mismatches returned the same frame index forever when cropping exceeded the
  image, freezing the animation. A zero column count could divide by zero.
- A missing companion config used the same app-wide shutdown path as a missing
  main config, so a bad companion selection could close the working main gremlin.
- The companion controller did not release its locked intro state after a sheet
  failure. Its cropper also hardcoded ten columns even if a pack supplied another
  grid. Closing a companion left its animation and gravity timers running.
- An unknown animation could show a dialog on each timer tick. Failed sheet
  decoding could be retried every tick when successful-image caching was off.
- A missing outro could prevent Stylish Close from completing.

These are verified source-level failure paths related to upstream invisible-pack,
animation and installation reports, including Kritzkingvoid/Desktop_Gremlin#54,
Kritzkingvoid/Desktop_Gremlin#68 and Kritzkingvoid/Desktop_Gremlin#86. This change
does not establish that every such report has the same cause.

## What changes

### Required pack data

Before creating animation, movement or gravity timers, the loader checks that:

1. A character has been selected and its config can be read.
2. `WIDTH`, `HEIGHT` and `IDLE` are positive integers.
3. Main packs provide a positive `COLUMN`. Companions may omit it for the legacy
   ten-column default; if supplied, it must be positive and is now honored.
4. `Actions/idle.png` can be decoded, and its complete declared idle animation
   fits its frame grid.

Missing or malformed required data raises `CharacterPackException` with the path
or config key and repair guidance. Validation happens before applying the pack's
frame settings, so a failed companion config does not partially change them.
The first verified idle frame is assigned immediately, before the intro timer.

The WPF app handles pack failures at startup and exits with code 1. It attempts a
diagnostic log even when dialogs are disabled. Summoning a broken companion is a
recoverable error: the main gremlin stays open, and an incomplete companion window
is closed without leaving it registered. Its timers are stopped on normal close.

### Optional animations

Zero frame count means an animation is disabled; its file is not loaded or
reported as missing. For positive counts, the shared sprite store checks the
complete declared grid against the decoded sheet before its first frame is drawn.
This catches an overdeclared count before the animation reaches its bad frame.
Long arithmetic prevents large malformed dimensions/counts from overflowing.

Missing, unreadable, unsupported or corrupt sheets and mismatched active grids
fall back to the verified idle frame and return the failure sentinel `-1`.
Both main and companion intro controllers unlock/reset after that sentinel.
Movement can continue with an idle image if the requested movement artwork is
unavailable. Stylish Close still exits if its optional outro cannot play.

Failures are remembered for that pack instance, even when `ALLOW_CACHE=false`,
and diagnostics are deduplicated per distinct message per process. This avoids
repeated decoding attempts, dialog storms and repeated log writes. Restarting the
main app or retrying a companion constructs a fresh store so repaired files can
load. A retained idle frame remains usable if its file is later removed.

Both sprite managers share the same decoding/layout/failure implementation.
Successful-sheet caching still follows `ALLOW_CACHE`; the idle fallback is always
retained. Stream loading with `OnLoad` closes file handles and avoids stale WPF
URI-cache data when a repaired pack is initialized again.

### Diagnostics

The log is `%LOCALAPPDATA%\DesktopGremlin\diagnostics.log`. Entries identify the
failing config/sheet path or animation grid. `ALLOW_ERROR_MESSAGES` controls
dialogs, not log attempts. Recoverable animation failures use the log and idle
fallback instead of interrupting the user with modal dialogs. If the log cannot
be written, I/O/access/security errors are ignored and tracing remains available.

Restore sprites and config from the same complete pack. Restart the main app or
retry the companion after repair. Existing release archives do not change until
the maintainer publishes rebuilt binaries.

## Regression prevention

The portable runner exercises the production config readers, frame-grid math and
diagnostic deduplication in addition to the earlier locale/monitor checks:

```sh
dotnet run --project tests/DesktopGremlin.RegressionTests -c Release -f net8.0
```

On Windows, CI also runs those cases on .NET Framework 4.8, builds the full WPF
Release app, then runs integration tests against that built executable:

```sh
msbuild Desktop_Gremlin.sln /m /p:Configuration=Release
dotnet run --project tests/DesktopGremlin.WindowsTests -c Release
```

The Windows runner generates tiny PNG fixtures and tests actual WPF decoding and
crop pixels, optional missing/corrupt sheets, invalid declared counts, required
idle failures, cache-on/off behavior, repaired-pack reinitialization, intro state
recovery and companion column metadata. It launches the app with a missing pack
to verify exit code/log behavior, then creates a real main window and verifies
that repeated failed companion attempts leave it running and do not accumulate
windows. A valid companion can still be summoned and closed afterward.

## Remaining limits

- No absent artwork, audio or Options GUI source is reconstructed. Gold Ship's
  reported release-specific animation/audio mismatch still needs that exact pack.
- Blue rendering (#85) remains undiagnosed. These checks cannot infer an OS/GPU
  rendering cause from the issue text.
- Only required idle data is decoded up front. Other sheets are checked when
  requested, avoiding a startup decode of every optional animation.
- Sound playback behavior is unchanged; audio decoding/volume defects remain a
  separate change. Sprite/config validation does not certify the pack's audio.
- Automated fixtures verify failure handling and cropping, not whether artwork
  visually matches its intended animation. Real-pack and multi-monitor visual
  checks remain part of release testing.
