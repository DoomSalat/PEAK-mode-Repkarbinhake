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
│   ├── LinkRegistry       — состояние всех связок, порядок записей, эхо; TryGetHeld — какую верёвку держит игрок
│   ├── RopePathAuthority  — кто считает путь верёвки: держащий, если держит один; иначе меньший номер; верёвки к манекену / якорю — локально
│   ├── RopePathAuthority  — кто считает путь верёвки: держащий, если держит один; иначе меньший номер; верёвки к манекену / якорю — локально
│   ├── UntiedKeySweeper   — какие развязанные ключи хосту пора удалить
│   ├── DeathUntieRule     — какие связки развязать из-за смерти (свои — всегда, чужие — если хост)
│   └── DepartedUntieRule  — какие связки развязать из-за выхода игрока (хост, по списку присутствующих)
├── Physics + Interaction
│   ├── StabilityResolver  — устойчивость актёра (бронирование, группа, цепочка)
│   ├── TensionSolver      — ускорение натяжения вдоль пути верёвки (RopeSpan); Load — нагрузка на верёвку
│   ├── RopeSpan           — направление от своего конца вдоль верёвки + длина пути (через края)
│   ├── RopeSnatch         — скорость рывка: насколько быстро натягивается верёвка со стороны партнёра (минус своя скорость от него), скачок > 2 м за кадр (телепорт) игнорируется
│   ├── RopeReel           — длина = полная − смотки держащих концов; смотка своего конца с торможением от нагрузки, но не медленнее MinReelInFactor (15%)
│   ├── HoldAction (Interaction) — удержание клавиши на цели (связать / отцепить)
│   └── RopeHandPolicy (Interaction) — что можно делать с верёвкой при опоре: Grounded / Airborne / Climbing
└── Visual
    ├── RopeSegmentation   — сколько костей у верёвки (Auto: общий бюджет делится на все верёвки)
    ├── IRopeObstacles     — Resolve (свип со скольжением + выталкивание) и TryHit (свип сферы по рельефу)
    ├── RopeWrapPath       — путь «конец → липкие контакты → конец» с длиной верёвки по участкам: контакт ставится на ребре (бинарный поиск между прошлым и текущим положением), снимается при видимости соседей; смотка конца меняет участок у ПРОТИВОПОЛОЖНОГО конца (контакты неподвижны); при натяжении с обеих сторон контакт держит длину, иначе запас перетекает; TrySpan — физика по пути
    ├── RopeSag            — спокойная опорная форма: провис каждого участка по точной длине дуги параболы, точка останавливается на рельефе; на каждую точку — свобода (натянутый участок ~0,03 м, провисший до 0,45 м, концы и контакты 0) и длина верёвки между соседями
    ├── RopeLooseness      — физика точек вокруг опорной формы: инерция, гравитация, верёвка не растягивается больше своей длины, точка держится в радиусе свободы, коллизия свипом от опорной точки (сквозь рельеф не проходит)
    └── RopeLengthState    — полная длина + смотка первого и второго конца

Repkarbinhake.Unity (netstandard2.1, только UnityEngine — общий для мода и песочницы)
├── RopeTubeView           — View: меш-трубка по точкам, подсветка (_Interactable / _EmissionColor)
├── PhysicsRopeObstacles   — IRopeObstacles через Physics.SphereCast / ComputePenetration, сам создаёт пробный коллайдер вдали от сцены
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
│   ├── RopePathSync         — синхронизация пути верёвки: хозяин (держащий, иначе меньший номер; RopePathAuthority) раз в 0,1 с рассылает точки касания и длины участков событием Photon 173, остальные ведут путь за ним (RopeWrapPath.Follow), смена хозяина — Lead, вошедшим — полный снимок
│   ├── ModVersionCheck      — своя версия мода в свойства игрока Photon (rkb.ver), лог версий остальных, предупреждение при расхождении
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
│   ├── RopeKnockdown        — сильный рывок (RopeSnatch ≥ KnockdownSpeed) по верёвке, которую ты НЕ держишь, роняет тебя в регдолл ванильным Character.Fall (по сети)
│   ├── RopeForces           — силы своему персонажу и манекену, обрыв, висящий без нарастающей гравитации, страховка
│   ├── RopeHoldPoses        — кто держит верёвку (все игроки) + угол катушки по изменению Reel (200°/м); показываемый угол догоняет целевой (сглаживание чужих рывков сети)
│   └── RopeHoldPose         — actor + угол вращения катушки
├── Visual
│   ├── RopePresenterPool    — верёвки на сцене: создание, отрисовка, путь (span), наведение, материал
│   ├── RopePresenter        — одна верёвка: RopeWrapPath + RopeSag + RopeLooseness + RopeTubeView + RopeInteractible; при пропаже конца путь сбрасывается
│   ├── SpoolProps           — визуальная копия обычной катушки в руках каждого держащего: центр её Hand_R/Hand_L ставится между физическими кистями, источник точки выхода верёвки
│   ├── SpoolProp            — MonoBehaviour на корне копии (ссылки на колесо и точки выхода переносятся при Instantiate из шаблона, собранного один раз): колесо (ropeSpoolTf) вращается при смотке, верёвка как у ванильной: видимый путь ropeStart → ropeBase → физическая часть (ActorStates: ropeBase — точка физики, TryGetRopeLead — ropeStart для отрисовки)
│   ├── CarabinerSounds / LinkSoundFeedback — SFX_Instance → SFX_Player игры; звук в середине верёвки
│   ├── CarabinerHud         — прицел-карабин (материал ванильного прицела) и подсказка «Привязать / Отцепить» с иконкой клавиши
│   ├── PlayerListBadges     — ряды цветных карабинов в списке игроков (ESC), проверка наложения на громкость / кик
│   ├── LinkBadgeRow         — ряд цветных карабинов под аватаркой в списке игроков (ESC), материал прицела, проверка наложения на громкость / кик
│   ├── RopeDebugReport      — строки отладки
│   └── RopeDebugOverlay     — View: текст отладки
└── Patches
    ├── RepkaHooks           — статический мост Back → патчи
    ├── RepkaHookFeed        — каждый кадр пишет в RepkaHooks данные для патчей (цель G, подсказка, держимая / наведённая верёвка, длина), Clear при выходе
    ├── InteractionPatches   — Interaction.DoInteractableRaycasts: верёвка под прицелом — только если игра ничего не нашла и вызов обычный (минимальный приоритет, порядок — в PLAN.md); при цели G верёвка не наводится
    ├── HudPatches           — тонкий патч: GUIManager.Start / UpdateReticle → CarabinerHud, кольцо удержания G (UI_UseItemProgress)
    ├── PlayerListPatches    — тонкий патч: AudioLevels.Update → PlayerListBadges
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
| `RopeTubeView` | `src/Repkarbinhake.Unity/RopeTubeView.cs` | View верёвки | `Initialize(filter, renderer, material, radius, highlightEmission)`, `Render(points)`, `Highlight`, `Unhighlight` |
| `PhysicsRopeObstacles` | `src/Repkarbinhake.Unity/PhysicsRopeObstacles.cs` | Обход рельефа точками верёвки: проверка по ходу движения + выталкивание из геометрии | ctor `(layerMask, probe SphereCollider)`; `Resolve(from, to, radius)` |
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
| `LinkKey` (G) по взгляду на игрока | `LinkHoldInput` (из `RepkaBack`) | удержание `TieHoldSeconds` / `UntieHoldSeconds` (выбор при нажатии) → `HoldAction` → `LinkRegistry.Tie` / `Untie` |
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
