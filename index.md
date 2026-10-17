# Repkarbinhake — Feature Index

> Мод PEAK: игроки связываются верёвкой (G по взгляду), берут её в руки (E) и сматывают колесом; держащие верёвку устойчивы и усиливают друг друга по цепочке.

## Layer Map
```
Repkarbinhake.Core (netstandard2.1, без Unity/Photon/BepInEx — тестируется вне игры)
├── Data
│   ├── IRopeSettings      — контракт настроек
│   ├── RopeLink           — связка двух актёров: TieId (поколение) + два конца
│   ├── RopeEnd            — конец верёвки: держат ли, сколько смотано
│   ├── LinkState          — сетевое состояние связки (tied, TieId)
│   ├── EndState / EndUpdate — сетевое состояние конца (TieId, held, reel), пишет только владелец
│   ├── LinkUpdate         — запись в сеть: state + автор + порядковый номер
│   ├── LinkKeys           — ключи `rkb.link.A_B` и `rkb.end.A_B.<actor>`
│   ├── ISegmentSettings   — настройки костей верёвки
│   └── SegmentMode        — Auto / Fixed
├── Links
│   ├── ILinkTransport     — публикация связок и концов, удаление через CAS
│   ├── LinkRegistry       — состояние всех связок, порядок записей, эхо
│   ├── UntiedKeySweeper   — какие развязанные ключи хосту пора удалить
│   ├── DeathUntieRule     — какие связки развязать из-за смерти (свои — всегда, чужие — если хост)
│   └── DepartedUntieRule  — какие связки развязать из-за выхода игрока (хост, по списку присутствующих)
├── Physics + Interaction
│   ├── StabilityResolver  — устойчивость актёра (бронирование, группа, цепочка)
│   ├── TensionSolver      — ускорение натяжения вдоль пути верёвки (RopeSpan); Load — нагрузка на верёвку
│   ├── RopeSpan           — направление от своего конца вдоль верёвки + длина пути (через края)
│   ├── RopeReel           — длина = полная − смотки держащих концов; смотка своего конца с торможением от нагрузки, но не медленнее MinReelInFactor (15%)
│   ├── HoldAction (Interaction) — удержание клавиши на цели (связать / отцепить)
│   └── RopeHandPolicy (Interaction) — что можно делать с верёвкой при опоре: Grounded / Airborne / Climbing
└── Visual
    ├── RopeSegmentation   — сколько костей у верёвки (Auto: общий бюджет делится на все верёвки)
    ├── HandOverHand       — цикл перехвата рук: фаза от смотанных метров, вылет правой / левой руки
    ├── CoilShape          — точки витков смотанной верёвки (радиус, число витков от смотки)
    ├── VerletRope         — (не используется с переходом на RopeWrapPath) симуляция точек верёвки (концы закреплены, провис, обход рельефа и после распрямления + проход по цепочке от закреплённого начала — точку, провалившуюся за оболочку меш-коллайдера, возвращает соседняя); TrySpan — путь от конца
    ├── IRopeCollider      — как точка верёвки обходит рельеф
    ├── IRopeObstacles     — IRopeCollider + TryHit (свип сферы по рельефу)
    ├── RopeWrapPath       — путь «конец → липкие контакты → конец» с длиной верёвки по участкам: контакт ставится на ребре (бинарный поиск между прошлым и текущим положением), снимается при видимости соседей; смотка конца меняет участок у ПРОТИВОПОЛОЖНОГО конца (контакты неподвижны); при натяжении с обеих сторон контакт держит длину, иначе запас перетекает; TrySpan — физика по пути
    ├── RopeSag            — отрисовка: провис каждого участка по его запасу (парабола), свип точек от прямой (не уходит в землю), раскачка провисших участков (пружина, амплитуда ≤ провиса)
    └── RopeLengthState    — полная длина + смотка первого и второго конца

Repkarbinhake.Unity (netstandard2.1, только UnityEngine — общий для мода и песочницы)
├── RopeTubeView           — View: меш-трубка по точкам, подсветка (_Interactable / _EmissionColor)
├── PhysicsRopeCollider    — IRopeCollider через Physics.SphereCast
└── VectorConversion       — UnityEngine ↔ System.Numerics

Sandbox (Unity 6000.3.12) — см. Sandbox/Assets/Sandbox/index.md

Repkarbinhake.Plugin (BepInEx 5, Harmony; деплой в BepInEx/plugins/Repkarbinhake)
├── RepkaPlugin              — точка входа: конфиг, ассеты, Harmony, создаёт RepkaBack (DontDestroyOnLoad)
├── RepkaBack                — Back (~220 строк): собирает инструменты, порядок кадра, хуки для патчей, смерть
├── Data
│   ├── RepkaConfig          — ConfigEntry (ModConfig) → IRopeSettings / ISegmentSettings
│   ├── LobbyRopeSettings    — настройки с длиной верёвки от хоста
│   ├── VanillaRopeSpool     — обычная (не анти) катушка из ItemDatabase: Item для поз рук, материал верёвки
│   ├── RepkaAssets / WavReader — иконка PNG → Sprite, WAV → AudioClip
│   ├── RepkaTexts           — строки подсказок
│   └── RepkaColors          — палитра цветов связок
├── Network
│   ├── PhotonLinkTransport  — ILinkTransport поверх room properties (LinkPayloadCodec), CAS-удаление
│   └── RoomLinkSync         — колбэки комнаты Photon, снимок при входе, обязанности хоста
├── Links
│   ├── CharacterDirectory   — actor ↔ Character
│   ├── ActorStates          — IFootingSource + IActorLifeSource + точки крепления верёвки (бедро / катушка в руках)
│   ├── TestAnchor           — локальный якорь для соло-теста (actor −1)
│   ├── LinkColors           — цвет каждой связки из палитры RepkaColors (первый свободный при появлении, свой у каждого клиента)
│   └── TestDummy            — локальный манекен-капсула с Rigidbody для соло-теста (actor −2): привязать G, таскать, отцепить
├── Input
│   ├── LinkHoldInput        — G: цель по взгляду, удержание, связать / отцепить
│   ├── TestAnchorInput      — F8: поставить / убрать тестовый якорь
│   ├── TestDummyInput       — F9: поставить манекен туда, куда смотришь / убрать (без автопривязки)
│   ├── LinkTargetPicker     — на кого смотрит игрок для G (игроки + манекен), отдаёт actor
│   └── KeyBinding           — InputAction из строки конфига, пересоздаётся при смене
├── Handling
│   ├── RopeInteractible     — IInteractible игры на верёвке (ванильная E, подсказка, подсветка)
│   ├── RopeHands            — взять / отпустить, отпуск при предмете, стене, без сознания
│   ├── ReelDriver           — колесо / кнопки → скорость смотки сглажена как у RopeSpool (Lerp ×4/с, −1…5), стамина, публикация ~10 Гц
│   ├── RopeForces           — силы своему персонажу и манекену, обрыв, висящий без нарастающей гравитации, страховка
│   ├── RopeHoldPoses        — кто держит верёвку (все игроки) + угол катушки по изменению Reel (200°/м, как у ванильной)
│   └── RopeHoldPose         — actor + угол вращения катушки
├── Visual
│   ├── RopePresenterPool    — верёвки на сцене: создание, отрисовка, путь (span), наведение, материал
│   ├── RopePresenter        — одна верёвка: RopeWrapPath + RopeSag + RopeTubeView + RopeInteractible; при пропаже конца путь сбрасывается
│   ├── SpoolProps           — визуальная копия обычной катушки в руках каждого держащего: центр её Hand_R/Hand_L ставится между физическими кистями, источник точки выхода верёвки
│   ├── SpoolProp            — одна копия: колесо (ropeSpoolTf) вращается при смотке, верёвка как у ванильной: видимый путь ropeStart → ropeBase → физическая часть (ActorStates: ropeBase — точка физики, TryGetRopeLead — ropeStart для отрисовки)
│   ├── CarabinerSounds / LinkSoundFeedback — SFX_Instance → SFX_Player игры; звук в середине верёвки
│   ├── LinkBadgeRow         — ряд цветных карабинов под аватаркой в списке игроков (ESC), материал прицела, проверка наложения на громкость / кик
│   ├── RopeWrapProbe        — ВРЕМЕННАЯ диагностика: раз в 2 с пишет в лог, если отрезок верёвки проходит сквозь геометрию (имя, слой, тип коллайдера, учитывает ли верёвка этот слой)
│   ├── RopeDebugReport      — строки отладки
│   └── RopeDebugOverlay     — View: текст отладки
└── Patches
    ├── RepkaHooks           — статический мост Back → патчи
    ├── InteractionPatches   — Interaction.DoInteractableRaycasts: верёвка под прицелом — только если игра ничего не нашла и вызов обычный (минимальный приоритет, порядок — в PLAN.md); при цели G верёвка не наводится
    ├── HudPatches           — прицел-карабин (материал и цвет ванильного прицела), подсказка «Привязать / Отцепить верёвку» — клон interactPromptPrimary с иконкой клавиши из InputSpriteData (как у E), над ванильной подсказкой, если она видна, кольцо удержания G (оригинальный UI_UseItemProgress)
    ├── PlayerListPatches    — AudioLevels.Update: для каждой строки списка игроков ряд карабинов цветами его связок; предупреждение в лог при наложении на иконку громкости или кнопку кика
    ├── RopeGaugePatches     — GUIManager.UpdateRope: пока держишь нашу верёвку — ванильный UI_Rope как есть, текущая длина (HeldRopeLength от Back)
    └── HoldPosePatches      — Idle «с предметом», IK рук на Hand_R/Hand_L катушки; CharacterRagdoll.SaveAdditionalTransformPositions — точка хвата animationItemTransform следует за взглядом (игра обновляет её только при предмете в руках)
```

## Classes

### Core / Data
| Class | File | What it holds |
|---|---|---|
| `IRopeSettings` | `src/Repkarbinhake.Core/Data/IRopeSettings.cs` | Длины, смотка, жёсткость/демпфер/кап, устойчивость |
| `RopeLink` | `src/Repkarbinhake.Core/Data/RopeLink.cs` | `FirstActor < SecondActor`, `Length`, `HolderActor` (`NoHolder = 0`) |
| `LinkState` | `src/Repkarbinhake.Core/Data/LinkState.cs` | `IsTied`, `TieId`; `Untied`, `ComposeTieId(writer, sequence)` |
| `EndState` / `EndUpdate` | `src/Repkarbinhake.Core/Data/EndState.cs`, `EndUpdate.cs` | `TieId`, `IsHeld`, `Reel` / `Key`, `State` |
| `RopeEnd` | `src/Repkarbinhake.Core/Data/RopeEnd.cs` | `Actor`, `IsHeld`, `Reel`, `HeldReel`, `Reset` |
| `RopeLink` | `src/Repkarbinhake.Core/Data/RopeLink.cs` | `FirstActor`, `SecondActor`, `TieId`, `First`, `Second`, `IsHeld`, `TotalReel`, `EndOf`, `IsHeldBy`, `EndKey`, `Retie` |
| `LinkUpdate` | `src/Repkarbinhake.Core/Data/LinkUpdate.cs` | `Key`, `State`, `WriterActor`, `Sequence` |
| `LinkKeys` | `src/Repkarbinhake.Core/Data/LinkKeys.cs` | `rkb.link.<low>_<high>`, `rkb.end.<low>_<high>.<actor>`; `Build`, `BuildEnd`, `TryParse`, `TryParseEnd` |

### Core / Instruments
| Class | File | Role | Key API |
|---|---|---|---|
| `LinkRegistry` | `src/Repkarbinhake.Core/Links/LinkRegistry.cs` | Единственный источник состояния связок на клиенте | `Bind(localActor)`, `Receive(LinkUpdate)`, `ReceiveEnd(EndUpdate)`, `Tie`, `Untie`, `Grab`, `Release`, `SetReel`, `ReleaseAllHeld`, `UntieActor`, `Clear`; события `LinkAdded`, `LinkRemoved` |
| `StabilityResolver` | `src/Repkarbinhake.Core/Physics/StabilityResolver.cs` | 0..`MaxStability` для актёра; упор и поддержка только при опоре | `Resolve(actor, links)`; зависимости `IRopeSettings`, `RopeHandPolicy`, `IFootingSource` |
| `TensionSolver` | `src/Repkarbinhake.Core/Physics/TensionSolver.cs` | Ускорение вдоль пути верёвки, только при натянутой (растяжение = длина пути через края − длина верёвки) | `Solve(span, velocity, length, stability)`, `Load(span, length)`, `IsSnapped` |
| `RopeSpan` | `src/Repkarbinhake.Core/Physics/RopeSpan.cs` | Направление от своего конца к ближайшему изгибу + длина пути; `Straight(self, partner)` — без препятствий | `Direction`, `PathLength` |
| `RopeReel` | `src/Repkarbinhake.Core/Physics/RopeReel.cs` | Длина верёвки и смотка | `FullLength`, `EffectiveLength(link)`, `StepReel(link, actor, scroll, dt, load, footing, stamina)` → `ReelStep(Reel, StaminaCost)`, `StaminaPerMeter(load)`, `ReelForLength`, `ReelInFactor(load)` = 1 − нагрузка / `ArmStrength` (при нагрузке ≥ силы рук смотка стоит) |
| `RopeHandPolicy` | `src/Repkarbinhake.Core/Interaction/RopeHandPolicy.cs` | Правила рук по опоре | `CanHold`, `CanBrace`, `CanReelIn`, `CanReelOut` |
| `Footing` / `IFootingSource` | `src/Repkarbinhake.Core/Data/Footing.cs`, `IFootingSource.cs` | Опора актёра (Grounded / Airborne / Climbing / Unconscious) | `FootingOf(actor)` |
| `IActorLifeSource` | `src/Repkarbinhake.Core/Data/IActorLifeSource.cs` | Жив ли актёр | `IsDead(actor)` |
| `DeathUntieRule` | `src/Repkarbinhake.Core/Links/DeathUntieRule.cs` | Связки на развязку из-за смерти | `Collect(links, localActor, isHost, result)` |
| `DepartedUntieRule` | `src/Repkarbinhake.Core/Links/DepartedUntieRule.cs` | Связки на развязку из-за выхода игрока | `Collect(links, presentActors, result)` |
| `HoldAction` | `src/Repkarbinhake.Core/Interaction/HoldAction.cs` | Удержание клавиши на цели (отцеп) | `Begin(target, duration)`, `Advance(isHeld, currentTarget, dt)`, `Cancel`, `Progress`, `IsActive` |
| `UntiedKeySweeper` | `src/Repkarbinhake.Core/Links/UntiedKeySweeper.cs` | Учёт развязанных ключей; наблюдают все, собирает хост | `Observe(update, now)`, `Collect(now, maxAge, result)`, `Forget`, `Clear` |
| `RopeRayPicker` | `src/Repkarbinhake.Core/Visual/RopeRayPicker.cs` | Попадание луча взгляда в верёвку (ближайшее сближение луча с отрезками) | `TryPick(origin, direction, maxDistance, tolerance, points, out distance)` |
| `VerletRope` | `src/Repkarbinhake.Core/Visual/VerletRope.cs` | Точки верёвки между двумя концами; столкновения проверяются и после распрямления — верёвка ложится на край, а не режет угол; в конце шага — свип каждой точки от соседней (оболочки меш-коллайдеров) | `Resize(count, start, end)`, `Step(start, end, length, gravity, dt, radius, collider)`, `TrySpan(fromStart, probeDistance, out span)`, `PathLength`, `Positions` |
| `RopeTubeView` | `src/Repkarbinhake.Unity/RopeTubeView.cs` | View верёвки | `Initialize(filter, renderer, material, radius, highlightEmission)`, `Render(points)`, `Highlight`, `Unhighlight` |
| `PhysicsRopeCollider` | `src/Repkarbinhake.Unity/PhysicsRopeCollider.cs` | Обход рельефа точками верёвки: проверка по ходу движения + выталкивание из геометрии | ctor `(layerMask, probe SphereCollider)`; `Resolve(from, to, radius)` |
| `RopeSegmentation` | `src/Repkarbinhake.Core/Visual/RopeSegmentation.cs` | Кол-во костей: `clamp(ceil(length / SegmentLength), 4, Cap)`; `Cap` = `FixedSegments` или `SegmentBudget / кол-во верёвок` в пределах `AutoMaxSegments` | `Count(length, ropeCount)`, `Cap(ropeCount)` |

### Plugin
| Class | File | Role | Key API |
|---|---|---|---|
| `RepkaConfig` | `src/Repkarbinhake.Plugin/Data/RepkaConfig.cs` | BepInEx config, редактируется ModConfig | `LinkKey`, `LinkAimAngle`, `Thickness` + `IRopeSettings` + `ISegmentSettings` (секция `Performance`) |
| `CharacterDirectory` | `src/Repkarbinhake.Plugin/Links/CharacterDirectory.cs` | Поиск персонажа по Photon actor (кэш, обновляется раз в кадр) | `Refresh`, `LocalActor`, `TryFind`, `ActorOf`, `IsActive` |
| `LinkTargetPicker` | `src/Repkarbinhake.Plugin/Input/LinkTargetPicker.cs` | Ближайший к прицелу игрок / манекен в радиусе взаимодействия игры (`Interaction.distance`, как подобрать нокнутого); манекен — также по попаданию луча в тело | `TryPick(self, view, out int targetActor)` |

## Data Flow
```
Локальное действие (G / E / колесо) → LinkRegistry.Tie/Untie → Publish(LinkUpdate); Grab/Release/SetReel → PublishEnd(EndUpdate, только свой конец) → room property
room property (от сервера, всем в одном порядке) → LinkRegistry.Receive / ReceiveEnd → LinkAdded / LinkRemoved / изменение RopeLink и RopeEnd
FixedUpdate: LinkRegistry.Links → StabilityResolver.Resolve(local) → TensionSolver.Solve(для каждой связки) → AddForce своему персонажу
```


- **Ключ связки** пишет любой участник. Чужая запись применяется, только если у меня нет неподтверждённой своей записи по этому ключу (сервер упорядочил чужую раньше моей — моя её перезапишет).
- Своё эхо ключа связки: устаревшее (номер меньше последнего отправленного) игнорируется, актуальное применяется. Одновременные завязки сходятся к записи, которую сервер получил последней.
- **Ключ конца** пишет только его владелец, поэтому конфликтов нет: своё эхо игнорируется, чужое применяется. Оба участника могут держать и сматывать одновременно.
- `TieId` (автор + номер записи завязки) — поколение связки. Запись конца с чужим `TieId` откладывается (может прийти раньше завязки при входе в комнату) и применяется, когда появится связка с этим `TieId`; хвосты прошлых завязок так и не применяются. Перезавязка сбрасывает оба конца.
- «Развязать» — это запись `IsTied = false`, а не удаление ключа: у неё есть автор и номер.
- Транспорт без эха собственных записей (`EchoesOwnUpdates = false`) — ожиданий не бывает, всё применяется сразу.
- Уборка: хост раз в несколько секунд берёт из `UntiedKeySweeper` развязанные ключи старше порога и удаляет их через `DeleteIfUnchanged` (Photon CAS с ожидаемым значением). Если кто-то успел завязать заново, значение уже другое, и удаление не пройдёт. Payload кодируется строкой, чтобы сравнение на сервере было надёжным.

## Entry Points
| Trigger | Who fires | Who handles |
|---|---|---|
| `LinkKey` (G) по взгляду на игрока | `RepkaBack` | удержание `TieHoldSeconds` / `UntieHoldSeconds` (выбор при нажатии) → `HoldAction` → `LinkRegistry.Tie` / `Untie` |
| E по верёвке | `InteractionPatches` → `RopeInteractible` → `RepkaBack` | `LinkRegistry.Grab` / `Release` своего конца |
| Колесо, пока держишь | `RepkaBack` | `TensionSolver.Load` → `RopeReel.StepReel` → `LinkRegistry.SetReel` (~10 Гц); отпустил → смотка конца 0 |
| Room properties update | Photon | `LinkRegistry.Receive` / `ReceiveEnd` |

## Cross-Feature Connections
| This feature uses | From | Via |
|---|---|---|
| Синхронизация позиций персонажей | PEAK (`Character.AllCharacters`, `Character.Center`) | прямое чтение |
| Хранилище состояния | Photon room custom properties | `ILinkTransport` |
| Настройки в игре | ModConfig | BepInEx `ConfigEntry` |

## Save Note
Сохранения между сессиями нет: связки живут в комнате Photon и пропадают с ней.

## Reuse Candidates
| Class | Why reusable |
|---|---|
| `LinkRegistry` | Упорядоченная синхронизация пар через любой транспорт с общим порядком доставки |
| `TensionSolver` | Мягкая верёвочная связь двух тел без джоинтов |
