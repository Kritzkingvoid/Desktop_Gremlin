
# Desktop Gremlin 
<img width="925" height="436" alt="image" src="https://github.com/user-attachments/assets/7f3f1631-b2d2-4b8e-adc4-b287dd81d784" />
<br>

[![Ko-fi](https://img.shields.io/badge/support_me_on_ko--fi-F16061?style=for-the-badge&logo=kofi&logoColor=f5f5f5)](https://ko-fi.com/kritzkingvoid)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/@kritzkingvoid)




```Python
//4.0 (I skipped to 4.0)
+Compile all the Uma gremlins into one executable
+Added a GUI for configuring option settings
+Added a setting to enable/disable click-through events for the sprite
+Re-added Haru Urara
```
```c#
FORCE_FAKE_TRANSPARENT = true //If for some reason it's translucent or opaque. This is a hack will prevent input around the sprite
```
```c#
ALLOW_COLOR_HOTSPOT = true //Makes the Hotspot visible in case you resize the image and want to know where the hotspots are
```
<img width="527" height="385" alt="image" src="https://github.com/user-attachments/assets/45434679-7b5b-49c1-9055-a753252e2e86" />

```c#
SHOW_TASKBAR = true //Show the Icon in the Task bar
```
# Linux
| Ver| Download Link |
|-----------|---------------|
| Linux Version | [Download](https://github.com/iluvgirlswithglasses/linux-desktop-gremlin) Thank you [@iluvgirlwithglasses](https://github.com/iluvgirlswithglasses), written in Python|

# Links

**Note:** If your browser falsely detects a file, please try using a different browser.

**Older release archives may still contain locale-dependent configuration bugs. The updated source accepts ungrouped decimal values using either `0.5` or `0,5`, regardless of Windows language or `LANGUAGE_DIFF`. Rebuilding this source does not update previously downloaded executables.**
https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/4.3/TracenAcademy_Speaki_v4.3.zip


## Trickal

| Character | Version | Download Link |
|-----------|---------|---------------|
|Speaki Pet| 4.3 |[Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/4.3/TracenAcademy_Speaki_v4.3.zip) |

## Umamusume 

| Character | Version | Download Link |
|-----------|---------|---------------|
| Tracen Academy with Tamamo Cross | 4.3 |[Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v4.3/TracenAcademy_Crossu_v4.3.zip) |
|StayGold only| 4.1 Just overwrite 4.0 with this |[Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v4.1/TracenAcademy_v4.1.zip) |
| Tracen Acacdemy [All umas from 1.0 up to Cafe]| 4.0 |[Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v4.0/TracenAcademy_v4.0.zip) |
| Doto | v2.9 Outdated |[Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.9/MeishoDoto_v2.9.zip) |
| Agnes Tachyon | v2.4 Outdated | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.4/Tachyon_v2.4.zip) |
| Matikanetannhauser | v2.7.2 Outdated| [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.7.2/Mambo_v2.8.zip) |
| Rice Shower | v2.2 Outdated | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/2.2/Tracen_v2.1_Rice.zip) |
| GoldShip | v2.5 Outdated | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.5/GoldShip_v2.5.zip) |
| Oguri | v2.7 Outdated | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.7/Oguri_v2.7.zip) |
| Manhattan | v3.0 Outdated | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v3.0/Cafe_v3.0.zip) |
## Blue Archive

| Character | Download Link |
|-----------|---------------|
| Koyuki | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.6/Koyuki_v2.6.zip) |

## Arknights
| Character | Download Link |
|-----------|---------------|
| Exusiai | [Download](https://github.com/Kritzkingvoid/Desktop_Gremlin/releases/download/v2.9.9/Exusiai_v2.9.zip) |


## 🛠️ Troubleshooting

| Problem | Possible Solution |
|---------|-----------------|
| Browser falsely detects  download | Try using a different browser or temporarily disable strict security settings. |
| Gremlin to Big or out-screen | This is mostly a screen scaling issue -> set sprite scaling to lower or 0.1 |
| Gremlin not following my mouse | Try lowering the FOLLOW_RADIUS in config.txt |
| Gremlin not animatiing while dragging | Performance options in windows. Check "Animate controls and elements inside windows" |
| Transparency not working | Settings → Personalization → Colors → Transparency Effects → On **OR** Settings → Accessibility → Contrast Themes → None |
| Gremlin Sprites are off-settings | Redownload the config file text |


## 🛠️ Q & A

| Q | A |
|---------|-----------------|
| Is this Umamusume only? | No. I also do Blue Archive and prob Arknights in the future. |
| Best Girl? | Manhattan Cafe and Calstone Light O |
| Can you do X characters? | I just add stuff I personally like|
| Where do get the Umamusume sheets? | Umaviewer |
| Do I take Commissions? | Whats a Commissions? |



## Building and troubleshooting current source

Build `Desktop_Gremlin.sln` in Visual Studio with the .NET desktop development
workload and the .NET Framework 4.8 targeting pack, or run
`msbuild Desktop_Gremlin.sln /p:Configuration=Release` from a Developer Command Prompt.
The project copies the available `SpriteSheet` and `Sounds` folders into the output.
The source tree currently includes Cafe's sprite pack; the release archives contain
additional characters. Keep each character's sprites, sounds and config from a
matching pack together. The separate Options GUI source is not included here.

- To summon a companion, set `COMPANION_CHAR` to an installed companion folder,
  enable keyboard controls, click the gremlin to focus it and press **5**.
  Press **F1** for keyboard help. Click-through mode must be off to click the sprite.
- To keep a character still, disable random actions and cursor following.
  Gravity and keyboard input can still move it.
- To resize a character, use a positive `SCALE` in its character config, or
  `SPRITE_SCALE` in the main config. A character's `SCALE` takes precedence.
- Dragging a character to another monitor makes that monitor's working area the
  boundary for random movement, keyboard movement and gravity. Cursor following
  and deliberate dragging can still move it to another monitor.

See [maintenance and validation notes](docs/maintenance.md) for root causes,
regression checks, manual display checks and unresolved reports.

## Character-pack failures

A main character needs readable `config.txt` and `Actions/idle.png`, positive
`WIDTH`, `HEIGHT`, `COLUMN` and `IDLE`, and enough sheet pixels for its declared
idle animation. Missing optional animations fall back to that verified idle
image. A missing/broken companion is rejected without closing the main gremlin.

Pack failures are logged once per distinct error per process to
`%LOCALAPPDATA%\DesktopGremlin\diagnostics.log`. `ALLOW_ERROR_MESSAGES=false`
suppresses dialogs, but diagnostics are still attempted. Fix the exact path or
config field named in the log using files from the same character pack; then
restart the app, or retry summoning a repaired companion. Legacy companion packs
without `COLUMN` keep their ten-column layout.

See [character-pack reliability](docs/character-pack-reliability.md) for causes,
error behavior, automated coverage and limitations.
