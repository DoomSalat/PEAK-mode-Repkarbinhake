# Changelog

## 1.1.4

- Fixed: in the Gloom the rope no longer jumps onto the outside of the big mangrove trees. Only leafy tree crowns keep the rope outside now; tree trunks, cacti and palm trunks are wrapped around like rock.
- The rope stays on top of the Nadir hanging bridge and the bridges between tree platforms in the redwood forest, like on other hanging bridges, instead of tangling in them.

## 1.1.3

- **One file:** the whole mod is now a single `RopeCarabiner.dll` (pictures and sounds are built in). Manual install: drop it into `BepInEx/plugins/RopeCarabiner`. If you installed an older version by hand, delete the old `BepInEx/plugins/Repkarbinhake` folder first. Your settings (`repkarbinhake.cfg`) are kept.
- **Grab the wall while being pulled:** a friend holding your taut rope brings you to with full control right away, and a rope yank no longer knocks you over while you hang or climb — only while you stand.
- **Pulling a climber:** while you reel in, the rope drags a friend climbing the wall up toward you (`ClimbHaulForce`), and the helping hand now also catches friends who climb, not only those hanging.
- **Firmer pull:** the rope stretches less while it's being reeled in (`HaulStiffness`).
- Removed `HangRecoverySeconds`: without a friend holding the rope you recover the game's normal way.

## 1.1.2

- **Come to faster on the rope:** knocked out of control while hanging, you recover right away if a teammate holds your rope (`RecoverWhenHeld`), otherwise within 5 seconds (`HangRecoverySeconds`) instead of the game's slow mid-air recovery. Then you can grab the wall.
- **Steadier camera while dangling** (`StableCamera`): the camera no longer jerks with your body and the yank shake is off while you hang on a taut rope.
- **Give a hand** now starts only while you actively pull the rope in, not when you just hold it.
- The mod now shows up as "Rope Carabiner" in BepInEx, the settings window and the version warning. Your settings file stays `repkarbinhake.cfg`.

## 1.1.1

- Fixed: the carabiner crosshair was invisible when looking at a teammate.

## 1.1.0

- **Give a hand:** pull a hanging or passed-out friend close and look at them — you let go of the rope and help them up like with the right mouse button (`AutoHandUp`).
- **Rescue:** a taut rope pulls a friend out of a cactus, a spider web or a flytrap (`FreeWhenPulled`).
- **Help onto the edge:** when you're hauled up and stuck right under the edge, you get pushed onto the floor (`MantleStep`, `MantlePush`).
- **Wall hook:** after catching a fall it reels you back up; own rope length `HookLength`; the icon by the crosshair shows meters, turns orange at full length, red while holding you, with red arrows while reeling you up; its click sounds lower and comes from the hook.
- **Let the rope out:** hold **T** while holding a rope; hanging in the air you let it out slower, so a friend pulling you up always wins.
- The rope stays on the outside of tree crowns, citadel floors and hanging bridges instead of tangling inside.
- The partner's name sits next to "Rope", both in the rope color; "Pull in" and "Let out" hints share one line.
- Optional `StableCamera`: the camera stops spinning while you dangle (off by default).
- `ArmStrength` default raised to 90.
- Mod page pictures now load from the main repository.

## 1.0.3

- Mod page now shows pictures and the tie GIF.
- Links to the source code and the bug tracker.

## 1.0.2

- Hold **G** while holding a rope to pull it in (key: `PullKey`). On-screen hint "G Pull in" in every game language.
- Looking at the friend you're tied to while holding the rope, **G** pulls instead of untying.
- In-game settings window: set `ConfigEditorKey` to open it (no key by default).
- Friendlier mod page with pictures.

## 1.0.1

- New package icon.

## 1.0.0

First public release.

- Tie to teammates with **G**, grab the rope with **E**, reel with the mouse wheel.
- Physical rope: wraps around rocks, sags, slides up gentle slopes, ignores chests, thin bridges and the scout cannon.
- Bracing, team pulling, belay on walls, knockdown on hard yanks, help under ledges.
- Wall hook on **T**: pays out while you climb, locks on a fall, stiff rope.
- Rope colors from the player list, on the rope, the spool and the length gauge.
- Partner name under the grab prompt; prompts in 15 languages.
- Mod version check between players.
