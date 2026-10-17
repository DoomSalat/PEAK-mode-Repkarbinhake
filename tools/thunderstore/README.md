# Rope Carabiner

Climb PEAK together on a real rope. Tie yourself to a teammate, hold the rope, reel them up, catch them when they fall, or drive a hook into the wall to belay yourself.

**Everyone in the lobby needs the mod** (same version). If a player has a different version, a line appears in the join/leave feed.

## Controls

| Key | Action |
|---|---|
| **G** (hold) while looking at a player | Tie a rope to them / untie it |
| **E** on a rope | Grab the rope / let go |
| **Mouse wheel** while holding the rope | Reel in / pay out |
| **T** (hold) while standing on the ground, aiming at a wall | Aim a wall hook; release to set it. Press **T** again to take it back |

Keys can be changed in the config.

## How the rope works

- **Physical rope.** It wraps around rock edges, sags when slack, slides up gentle slopes and swings. It ignores chests, thin generated bridges and the scout cannon.
- **Holding and reeling.** Reeling in a loaded rope is slower and costs stamina. Teammates holding ropes to the same player help: the first adds +60%, each next one two thirds of the previous.
- **Bracing.** Holding a rope while standing on the ground makes you hard to drag. Other bracing players in the group add to it.
- **Belay.** While a teammate holds your rope, running out of stamina on a wall does not send you sliding fast.
- **Knockdown.** A very hard yank knocks you down unless you hold that rope.
- **Ledges.** If the rope pins you under an overhang, it stops pressing you into the wall and pushes you off it, so you swing around the ledge.
- **Wall hook.** The hook rope pays out while you climb and locks as soon as you fall. It is stiff, so a fall on the hook barely stretches it. An icon by the crosshair shows that your hook is set.
- **Colors.** Each rope gets a color — the same as its badge in the player list (ESC). The rope, the spool in your hands and the rope length gauge take that color.
- Interaction prompts follow the game language (15 languages).

## Settings

All values live in `BepInEx/config/repkarbinhake.cfg` (edit it through your mod manager's config editor; the file keeps the mod's internal name).

Pulling forces are computed by each player's own game for their own character. If a teammate cannot be lifted, they should raise **`MaxPull`** in their config. The full rope length (**`MaxLength`**) is taken from the host.

Useful values:

| Setting | Default | What it does |
|---|---|---|
| `MaxLength` | 14 | Full rope length, host value is used |
| `MinLength` | 0.1 | Shortest length after reeling in |
| `ReelSpeed` | 10 | Reeling speed at full wheel speed |
| `ArmStrength` | 85 | How well you reel in a loaded rope |
| `MaxPull` | 80 | Max pull of a rope on **you** |
| `AnchorStiffness` | 300 | Stiffness of the wall hook rope |
| `RopeColor` | 0.85 | How strongly ropes take their color |
