# Maintenance and validation

## Configuration parsing: #74 and the locale-related symptoms in #54

Both configuration loaders previously selected a parser using `LANGUAGE_DIFF`.
The invariant branch used `NumberStyles.Any`, which allows thousands separators:
`0,50` could be read as `50`. The other branch depended on the current culture,
so dot-decimal config files could also be misread. In the master config, the
position of `LANGUAGE_DIFF` affected which parser handled earlier settings.
Character and companion keys also used culture-sensitive uppercasing; lowercase
`idle`, `intro` and `runidle` do not match the expected keys under Turkish casing.

The master loader and frame loader now share `ConfigNumbers`. An ungrouped number
may use one decimal point or comma; grouping/currency formats, mixed separators,
NaN and infinity are rejected. A single comma is always a decimal separator:
`1,234` means `1.234`, never `1234`. Invalid master settings preserve their previous value. Required character-pack
metadata is now validated before loading; see [character-pack reliability](character-pack-reliability.md).
Scales and timer/movement divisors must be positive; volume must be between zero
and one. Optional animation counts may still be zero. Configuration key casing is
invariant. `LANGUAGE_DIFF` remains accepted for compatibility but no longer
changes numeric interpretation. The old `START_BUTTOM` typo remains accepted;
the bundled config uses `START_BOTTOM`.

`ConfigSettings` contains the production master mapping separately from the WPF
window/tray setup so the regression runner can exercise the actual loader.

The Options GUI source is absent (#77). This change makes the app consume its
comma-decimal output safely; it does not repair or rebuild that GUI. Likewise,
it does not replace existing Doto release executables or establish that every
invisible-character report has the same cause. Issue #54 requires a test with
the affected character pack before its full resolution can be confirmed.

## Monitor behavior: #72 and #76

Random movement, keyboard movement, gravity and food placement used the primary
monitor's `SystemParameters.WorkArea`. Dragging a gremlin onto a secondary monitor
could therefore cause later actions to send it back toward the primary display.

`MonitorWorkArea` now selects the current window's monitor and converts its pixel
working area into WPF device-independent coordinates. `MovementBounds` handles
negative desktop origins, sprite extents and gravity overshoot. Dragging cancels
an existing random walk before it can continue moving the window.

This is current-monitor containment for those actions, not a new saved monitor
selector or a lock against dragging/cursor following. It does not change the
app's existing system-DPI-aware manifest to per-monitor DPI awareness.

## Source build and asset copying

The project file enumerated 85 paths for files absent from the source tree.
It also omitted many sound files that are present. A normal build could fail
copying missing content, or omit available audio from its output.

The project now copies the actual files under `SpriteSheet` and `Sounds` with
recursive content globs. Additional packs placed in those folders are included
without updating a handwritten asset list. No missing character artwork or sound
files are invented. The tracked source currently contains Cafe's sprite pack;
the configured Durandal companion must be installed separately before summoning.

## Companion documentation: #79

The F1 help showed `4` for companion summoning while the keyboard handler uses
`5`. The help and README now show `5`, including the focus, keyboard-enable and
installed-pack requirements. This is a documentation correction, not a change
to the companion binding.

## Automated checks

The dependency-free regression executable links the production config loaders,
settings and monitor geometry. Run:

```sh
dotnet run --project tests/DesktopGremlin.RegressionTests -c Release -f net8.0
```

On Windows, also run against the app's .NET Framework runtime:

```sh
dotnet run --project tests/DesktopGremlin.RegressionTests -c Release -f net48
msbuild Desktop_Gremlin.sln /m /p:Configuration=Release
```

The GitHub workflow runs portable checks on Windows and Linux, plus the net48
regression executable and full WPF build on Windows. Cases cover five locales
(`en-US`, `de-DE`, `fr-FR`, `tr-TR`, `es-ES`), both decimal separators, both flag
values in both file orders, character/companion key casing, malformed numbers,
invalid scale/dimensions/divisors, negative monitor origins and gravity overshoot.

## Manual Windows checks

The portable geometry checks do not exercise Windows monitor selection or WPF
layout/rendering. Before publishing rebuilt binaries:

1. Build and launch Cafe, including intro, emotes and sound from the build output.
2. Test dot/comma configs under a comma-decimal locale. Check scale, volume and
   companion scale with an installed companion pack. Keep the original configs.
3. Drag to monitors to the left, right and above the primary display. Enable
   random movement, keyboard movement, gravity and food placement separately.
   Verify the character stays in the selected working area and honors taskbars.
4. Repeat with 100%, 150% and mixed display scaling. Confirm cursor following and
   dragging can still cross monitors intentionally. Drag during a random walk.
5. Focus the gremlin and check F1/5 with keyboard controls enabled and click-through
   disabled. Confirm the companion uses its own monitor's floor.

## Reports requiring separate evidence or assets

- **#61:** GitHub currently lists the Doto v2.9 asset. The report has no failing
  URL or reproduction detail. README links use the current repository owner and
  include the available Tracen Academy v4.3 Tamamo Cross archive. This does not
  establish that the original download failure has been reproduced or fixed.
- **#68:** Gold Ship's spritesheet/config are not tracked in this source tree.
  Inspect the affected release pack to resolve its animation/audio mismatch.
- **#85:** Blue rendering has no reproduction steps or diagnosed cause.
- **#77:** The Options GUI source cannot be recovered from this branch.
- New character, Linux-port, jukebox and launcher requests remain separate work.

An upstream merge only changes source. Older published archives retain their
behavior until the maintainer builds, tests and publishes replacement binaries.
