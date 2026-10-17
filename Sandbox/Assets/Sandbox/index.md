# Sandbox — Feature Index

> Песочница Unity 6000.3.12 для настройки ощущений верёвки вне игры: 4 капсулы-«регдолла» на склоне, та же логика `Repkarbinhake.Core` и тот же визуал `Repkarbinhake.Unity`, что и в моде.

## Как запустить
1. `dotnet build src/Repkarbinhake.Unity` — кладёт `Repkarbinhake.Core.dll` и `Repkarbinhake.Unity.dll` в `Assets/Plugins/Repkarbinhake`.
2. Открыть папку `Sandbox` в Unity Hub (6000.3.12f1).
3. Меню **Repkarbinhake → Build Sandbox Scene** — соберёт `Assets/Sandbox/Sandbox.unity` и ассеты в `Generated/`.
4. Play. Настройки — ассет `Generated/SandboxRopeSettings` (менять можно прямо во время Play).

Управление: `1-4` выбрать игрока, `WASD` идти, `Space` прыжок, мышь — камера, `Esc` отпустить курсор, удерживать `G` по прицелу — связаться / отцепиться (кольцо на прицеле), `E` взять/отпустить свой конец верёвки к цели (без цели — отпустить все; одну верёвку могут держать оба — переключись `1-4` и нажми `E` за второго), колесо — смотать/размотать держимые, `F` упасть, `U` потерять сознание на 6 с, `K` умереть, `L` выйти из игры, `R` всех на старт (вернуть вышедших и воскресить), `P` столкнуть цель (импульс + падение), `T` сцена «скала».

### Сцена «скала» (T)
Игрок 1 стоит на уступе скалы и держит верёвку, укороченную до `CliffRopeLength`; игрок 2 стоит на краю. Через `PushDelay` игрока 2 сталкивают (`PushSpeed`, `PushUpward`). В панели: пиковая нагрузка на верёвку (рывок), пиковая тяга на держащего, насколько держащего протащило, глубина провиса, время до подъёма на уступ, текущая гравитация висящего. Поднимать — колесом (смотка тормозится нагрузкой и тратит стамину держащего; в панели видно, сколько потрачено). Висящий может взять свой конец, но не может сматывать и не упирается — только разматывать (спуск).

**Автоподъём без клавиатуры:** в Play на `Sandbox Rope Back` → ⋮ → `RunCliffAutoLift` (или через Unity MCP: `SendMessage("RunCliffAutoLift")`). Через 1,5 с после падения держащий непрерывно сматывает; раз в 5 с и в конце в консоль пишется строка `LiftTrial: ...` (до края за N с, рывок, протаскивание держащего, стамина). «Дотянут до края» = голова висящего у кромки (глубина ≤ 1,7 м): в игре дальше он хватается за край и залезает сам. Если Unity не в фокусе, для прогона нужно `Application.runInBackground = true`.

**Звук:** `Audio/SandboxMixer.mixer` (Master → SFX), источник `CarabinerAudio` выводится в SFX — как звуки PEAK идут в его SFX-группу. Микшер и группу создаёт **Build Sandbox Scene** (через внутренний `AudioMixerController`, публичного API нет).

**Материалы капсул:** на земле цепкий (`ClimberGrip`, трение 1), в воздухе и при падении скользкий (`ClimberSlide`, 0,15) — упавший съезжает по камню, как регдолл, а не прилипает к стене.

### Гравитация как в PEAK
Капсулы не используют гравитацию Unity: на земле `GroundGravity`, в воздухе — нарастает от `AirGravityStart` до `AirGravityMax` за `AirGravityRampSeconds`. Время в воздухе копится первую секунду и дальше только пока падение быстрее 1 м/с — как `sinceGrounded` в игре (`CharacterMovement.GetGravityForce`, максимум 20). Реальную кривую `jumpGravityCurve` игры прочитать без выгрузки ассета нельзя — тут линейное приближение.

Предложение для проверки: `RopeSupportCapsAirTime` — пока натянутая верёвка держит игрока вверх (≥ `SupportLoadThreshold`), его время в воздухе ограничено `HangAirTimeCap`, и висящий не «тяжелеет». Выключи галочку, чтобы сравнить с поведением без защиты.



## Layer Map
```
SandboxRopeBack                — Back (~370 строк): ввод, выбор игрока, порядок кадра, связка инструментов
├── SandboxCrew                — все капсулы, присутствующие, ClimberOf(actor)
├── SandboxClimber ×4          — капсула с балансом, гравитация как в PEAK, стамина, смерть / без сознания / выход
├── SandboxCamera              — орбитальная камера за выбранным
├── SandboxAimPicker           — на кого смотрит прицел (G)
├── SandboxRopeVisuals         — верёвки на сцене: RopeVisual на связку, путь (span), наведение на верёвку (E), подсветка
├── SandboxRopeForces          — натяжение всем присутствующим, нагрузка, тяга для панели
├── SandboxLifeRules           — вышедшие, обрыв, смерть, отпуск рук без сознания
├── SandboxCliffScenario       — сцена «скала»: толчок, автоподъём, LiftTrial
├── SandboxOverlayReport       — строки панели
├── SandboxClimberTint         — цвет капсулы по состоянию (SandboxClimberColors)
├── LoopbackLinkTransport      — ILinkTransport без сети
└── View
    ├── SandboxDebugOverlay    — панель состояния + прицел-карабин / подсказка
    └── CarabinerAudio         — звук защёлкивания / открытия карабина (в SandboxMixer → SFX)
```

## Classes
| Class | File | Role | Key API |
|---|---|---|---|
| `SandboxRopeBack` | `Scripts/SandboxRopeBack.cs` | Back | `[ContextMenu] RunCliffAutoLift` |
| `SandboxCrew` | `Scripts/SandboxCrew.cs` | Instrument | `All`, `Present()`, `ClimberOf(actor)` |
| `SandboxClimber` | `Scripts/SandboxClimber.cs` | Instrument | `Initialize(actor, groundMask)`, `Tick`, `Move`, `Jump`, `Fall`, `KnockOut`, `Kill`, `Leave`, `Rejoin`, `Push`, `AddAcceleration`, `UseStamina`, `ClampAirTime`, `Teleport`, `Respawn`; `RopePoint`, `Center`, `Velocity`, `Footing`, `Stamina`, `IsPresent`, `IsDead`, `IsUnconscious`, `IsFallen`, `IsGrounded`, `CurrentGravity` |
| `SandboxRopeVisuals` | `Scripts/SandboxRopeVisuals.cs` | Instrument | `Add`, `Remove`, `Render(links, dt)`, `SpanFor(link, self)`, `PickOwn(links, actor, origin, direction)`, `Highlight(link)`, `SegmentsOf(link)` |
| `SandboxRopeForces` | `Scripts/SandboxRopeForces.cs` | Instrument | `Apply(climbers)`, `PullOf(actor)`, `LoadOn(link, self)` |
| `SandboxLifeRules` | `Scripts/SandboxLifeRules.cs` | Instrument | `Apply(selectedActor)`, `IsHolding(actor)` |
| `SandboxCliffScenario` | `Scripts/SandboxCliffScenario.cs` | Instrument | `Begin`, `BeginAutoLift`, `Push(target, from)`, `Tick(dt)`, `Stop`; `Trial`, `Holder` |
| `LiftTrial` | `Scripts/LiftTrial.cs` | Instrument | `Begin(holder, hanger, ledgeHeight)`, `MarkLiftStart`, `Tick(dt, load, holderPull)`, `AddStaminaSpent`, `Report`, `Describe(lines)`, `Stop` |
| `SandboxOverlayReport` | `Scripts/SandboxOverlayReport.cs` | Instrument | `Build(selected, trial)` |
| `SandboxClimberTint` / `SandboxClimberColors` | `Scripts/SandboxClimberTint.cs`, `SandboxClimberColors.cs` | Instrument / данные | `Apply(climber, isSelected, isHolding)` |
| `SandboxCamera` | `Scripts/SandboxCamera.cs` | Instrument | `Look`, `Follow`, `Snap`; `Forward`, `FlatForward`, `FlatRight`, `Position` |
| `SandboxAimPicker` | `Scripts/SandboxAimPicker.cs` | Instrument | `TryPick(self, climbers, viewPosition, viewForward, maxDistance, maxAngle, out target)` |
| `SandboxFooting` / `SandboxLives` | `Scripts/SandboxFooting.cs`, `SandboxLives.cs` | Instrument | `IFootingSource`, `IActorLifeSource` |
| `LoopbackLinkTransport` | `Scripts/LoopbackLinkTransport.cs` | Instrument | `ILinkTransport`, `PublishedCount` |
| `RopeVisual` | `Scripts/RopeVisual.cs` | Instrument | `Tick(...)`, `TrySpan`, `TryPick`, `Highlight`, `Unhighlight`, `Destroy` |
| `SandboxDebugOverlay` | `Scripts/SandboxDebugOverlay.cs` | View | `Show(lines)`, `ShowCarabiner(hint, progress)`, `ShowHint(hint)`, `HideCarabiner` |
| `CarabinerAudio` | `Scripts/CarabinerAudio.cs` | View | `PlayLock(position)`, `PlayUnlock(position)` |
| `SandboxRopeSettings` | `Scripts/SandboxRopeSettings.cs` | SO | `IRopeSettings` + `ISegmentSettings` + ввод, висящий, сцена «скала» |
| `SandboxSceneBuilder` | `Scripts/Editor/SandboxSceneBuilder.cs` | Editor tool | меню `Repkarbinhake/Build Sandbox Scene` |

## Data Flow
```
Input (Update) → LinkRegistry.Tie/Untie/Grab/Release/SetReel
FixedUpdate: Climber.Tick → SandboxLifeRules.Apply (вышедшие, обрыв, смерть, руки) → SandboxRopeForces.Apply (StabilityResolver → TensionSolver по пути верёвки) → SandboxCliffScenario.Tick
LateUpdate: SandboxRopeVisuals.Render (RopeSegmentation → VerletRope + PhysicsRopeCollider → RopeTubeView) → тинт → SandboxOverlayReport → SandboxDebugOverlay
LinkRegistry.LinkAdded/Removed → SandboxRopeVisuals.Add/Remove + CarabinerAudio
```

## Отличия от игры
- Все «клиенты» в одном процессе: силы считаются для каждой капсулы, `LinkRegistry.Bind` переключается на выбранного игрока.
- Капсула с балансом вместо регдолла PEAK: формулы и визуал проверяются, точное ощущение — только в игре.
- Игроки на слое `Ignore Raycast`, рельеф — `Default`.

## Save Note
Нет.
