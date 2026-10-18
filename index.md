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
│   ├── LinkRegistry       — состояние всех связок (словарь по числовой паре актёров LinkKeys.PairId, без строк в TryGet), порядок записей, эхо; концы, пришедшие раньше связки, ждут её (PendingEndCount) и удаляются при появлении или удалении связки пары; TryGetHeld — какую верёвку держит игрок
│   ├── RopePathAuthority  — кто считает путь верёвки: держащий, если держит один; иначе меньший номер; верёвки к манекену / якорю — локально
│   ├── UntiedKeySweeper   — какие развязанные ключи хосту пора удалить
│   ├── DeathUntieRule     — какие связки развязать из-за смерти (свои — всегда, чужие — если хост)
│   └── DepartedUntieRule  — какие связки развязать из-за выхода игрока (хост, по списку присутствующих)
├── Physics + Interaction
│   ├── StabilityResolver  — устойчивость актёра (бронирование, группа, цепочка)
│   ├── TensionSolver      — ускорение натяжения вдоль пути верёвки (RopeSpan); Load — нагрузка на верёвку
│   ├── RopeSpan           — направление от своего конца вдоль верёвки + длина пути (через края)
│   ├── TeamPull           — командное усилие: помощники (держат верёвку к тому же игроку, стоят на опоре) повышают силу рук: +60%, дальше каждый следующий 2/3 предыдущего (×1,6 / ×2,0 / ×2,27 / ×2,45), нагрузка делится на множитель
│   ├── AnchorLength       — длина верёвки крюка: на опоре = путь + 0,2 м (до MaxLength), срыв вниз (> 3 м/с) запирает, снова на опоре — отпирает
│   ├── RopeSnatch         — скорость рывка: насколько быстро натягивается верёвка со стороны партнёра (минус своя скорость от него), скачок > 2 м за кадр (телепорт) игнорируется
│   ├── RopeReel           — длина = полная − смотки держащих концов; смотка своего конца с торможением от нагрузки, но не медленнее MinReelInFactor (15%); в воздухе разматывание — AirborneReelOutFactor (30%) скорости, чтобы висящий спускался, но не перетягивал вытаскивающего
│   ├── HoldAction (Interaction) — удержание клавиши на цели (связать / отцепить)
│   └── RopeHandPolicy (Interaction) — что можно делать с верёвкой при опоре: Grounded / Airborne / Climbing
└── Visual
    ├── RopeSegmentation   — сколько костей у верёвки (Auto: общий бюджет делится на все верёвки)
    ├── IRopeObstacles     — Resolve (свип со скольжением + выталкивание) и TryHit (свип сферы по рельефу)
    ├── RopeWrapPath       — путь «конец → липкие контакты → конец» с длиной верёвки по участкам: контакт ставится на ребре (бинарный поиск между прошлым и текущим положением), снимается при видимости соседей; смотка конца меняет участок у ПРОТИВОПОЛОЖНОГО конца (контакты неподвижны); при натяжении с обеих сторон контакт держит длину, иначе запас перетекает; кромка над нижним концом (SeatLowerLip, только одна точка касания — ближайшая к концу, что ниже): если отрезок от нижнего конца к ней режет камень, бинарным поиском между этим отрезком и чистой вертикалью ставится точка на саму кромку (TryFindEdge); затем эта точка подтягивается вдоль тяги (Tighten: до 0,5 м, 6 шагов, не вниз), пока оба отрезка свободны — встаёт ровно на ребро, лишняя точка за краем снимается Unwrap; натянутый контакт ползёт вверх по склону (Climb: раз в кадр пробует одно направление из набора «вперёд/вверх/вбок», шаг принимается, если не опускает контакт, укорачивает путь сильнее трения μ = 0,36 — склоны положе ~70° — и соседние участки не входят в камень); TrySpan — физика по пути
    ├── RopeSag            — спокойная опорная форма: провис каждого участка по точной длине дуги параболы, точка останавливается на рельефе; на каждую точку — свобода (натянутый участок ~0,03 м, провисший до 0,45 м, концы и контакты 0) и длина верёвки между соседями
    ├── RopeLooseness      — физика точек вокруг опорной формы: инерция, гравитация, верёвка не растягивается больше своей длины, точка держится в радиусе свободы, коллизия свипом от опорной точки (сквозь рельеф не проходит)
    ├── RopeLengthState    — полная длина + смотка первого и второго конца
    └── AnchorActors / AnchorPlacement / AnchorPayloadCodec — номер крюка (−владелец), место и ориентация, строка свойства комнаты

Repkarbinhake.Unity (netstandard2.1, только UnityEngine — общий для мода и песочницы)
├── RopeTubeView           — View: меш-трубка по точкам, подсветка (_Interactable / _EmissionColor), ChangeMaterial(material) — смена материала (перекрашенная копия)
├── PhysicsRopeObstacles   — IRopeObstacles через Physics.SphereCast / ComputePenetration, сам создаёт пробный коллайдер вдали от сцены; пропускает коллайдеры по IColliderFilter (NoColliderFilter в песочнице); «оболочки» (IColliderFilter.ShellOf → ShellShape) не выталкиваются к ближайшей поверхности, а держатся снаружи: луч снаружи к точке (с тыльными гранями), задел оболочку — точка на внешнюю поверхность. Round (кроны) — направление от центра наружу; Slab (полы цитадели — полые плиты: верх смотрит вверх, низ вниз, 1–2,4 м между ними) — вдоль самой тонкой оси коробки коллайдера, в сторону той половины, где точка. Top (подвесные мосты) — только вверх, и только если точка не ниже низа коробки коллайдера (верёвка под мостом остаётся под ним). Пробная сфера для оболочек 0,75 м
└── VectorConversion       — UnityEngine ↔ System.Numerics

Sandbox (Unity 6000.3.12) — см. Sandbox/Assets/Sandbox/index.md

Repkarbinhake.Plugin (BepInEx 5, Harmony; деплой в BepInEx/plugins/Repkarbinhake)
├── RepkaPlugin              — точка входа: конфиг, ассеты, Harmony, создаёт RepkaBack (DontDestroyOnLoad)
├── RepkaBack                — Back (~220 строк): собирает инструменты, порядок кадра, хуки для патчей, смерть
├── Data
│   ├── SettingScopes        — static-таблица: ключ конфига → на кого действует (для окна F7; неизвестные — «только у тебя»)
│   ├── AnchorRopeSettings   — IRopeSettings верёвки крюка: Stiffness/Damping/MaxPull из [Hook] (AnchorStiffness 300, AnchorDamping 20, AnchorMaxPull 250), остальное — как у обычной; RopeForces и отладка берут для связок с крюком свой TensionSolver
│   ├── RepkaConfig          — ConfigEntry (ModConfig) → IRopeSettings / ISegmentSettings; при запуске удаляет из .cfg записи, которых мод больше не читает (OrphanedEntries через AccessTools)
│   ├── LobbyRopeSettings    — настройки с длиной верёвки от хоста
│   ├── VanillaRopeSpool     — обычная (не анти) катушка из ItemDatabase: Item для поз рук, материал верёвки
│   ├── RepkaAssets / WavReader — иконка PNG → Sprite, WAV → AudioClip
│   ├── RepkaTexts           — строки подсказок на 15 языках игры (по LocalizedText.CURRENT_LANGUAGE, иначе английский); окно F7 — только по-русски
│   └── RepkaColors          — палитра цветов связок
├── Network
│   ├── PhotonLinkTransport  — ILinkTransport поверх room properties (LinkPayloadCodec), CAS-удаление
│   ├── AnchorSync           — крюки через свойства комнаты rkb.anchor.<номер>: владелец пишет, все читают; хост удаляет крюк вышедшего; смена локации снимает свой крюк
│   ├── PhotonRoomCallbacks  — база сетевых инструментов: пустые обработчики IInRoomCallbacks/IMatchmakingCallbacks, Enable/Disable (регистрация в Photon, при уже открытой комнате — сразу OnJoinedRoom); RoomLinkSync, AnchorSync, ModVersionCheck, RopePathSync переопределяют только нужное
│   ├── RopePathSync         — синхронизация пути верёвки: хозяин (держащий, иначе меньший номер; RopePathAuthority) раз в 0,1 с рассылает точки касания и длины участков событием Photon 173, остальные ведут путь за ним (RopeWrapPath.Follow), смена хозяина — Lead, вошедшим — полный снимок
│   ├── ModVersionCheck      — своя версия мода в свойства игрока Photon (rkb.ver), лог версий остальных, предупреждение при расхождении; при расхождении — строка в ванильной ленте «вошёл / вышел» (PlayerConnectionLog, экземпляр — RepkaHooks.ConnectionLog из ConnectionLogPatches)
│   └── RoomLinkSync         — колбэки комнаты Photon, снимок при входе, обязанности хоста
├── Links
│   ├── CharacterDirectory   — actor ↔ Character
│   ├── ActorStates          — IFootingSource + IActorLifeSource + точки крепления верёвки (бедро / катушка в руках)
│   ├── TestDummy            — отладочная капсула (Rigidbody 60 кг), актёр −1 000 000: не крюк (AnchorActors — только −1…−9999), локальная (ключи с «-» не публикуются); ActorStates — опора/точка верёвки, LinkTargetPicker — прицел G, RopeForces.ApplyToDummy — тяга, RoomLinkSync — связки с локальными актёрами (не игроки и не крюки) не проверяет на уход из комнаты
│   ├── AnchorBoard          — крюки игроков (владелец → место): связка игрок ↔ крюк (крюк = актёр −номер, верёвка локальная у каждого клиента), точка верёвки на крюке, EnsureLinks; Remove сначала развязывает, потом убирает крюк — чтобы звук отстёгивания нашёл точку
│   └── LinkColors           — цвет каждой связки из палитры RepkaColors (первый свободный при появлении, свой у каждого клиента); TryGetColor — цвет верёвки (у крюков нет)
├── Input
│   ├── LinkHoldInput        — G: цель по взгляду, удержание, связать / отцепить; HasAction — цель есть и это не «держишь верёвку и смотришь на того, с кем связан» (тогда G подтягивает, а не отвязывает)
│   ├── TestDummyInput       — F9 (TestDummyKey): поставить капсулу по лучу до 30 м / убрать; пропала — связка развязывается
│   ├── AnchorInput          — T: удержание на земле (Footing.Grounded; оторвался — прицел сброшен) — призрак крюка в точке прицела (до 10 м, ставится до 5 м, как у ванильной катушки), отпустил — крюк; нажатие при стоящем — снять; снимается при смерти и дальше SnapDistance; пока держишь верёвку и UnwindKey совпадает с AnchorKey — клавиша отдана разматыванию (keyTaken: прицел сброшен, крюк не ставится и не снимается, после отпускания верёвки T ждёт отпускания клавиши)
│   ├── LinkTargetPicker     — на кого смотрит игрок для G, отдаёт actor
│   └── KeyBinding           — InputAction из строки конфига, пересоздаётся при смене
├── Handling
│   ├── RopeInteractible     — IInteractible игры на верёвке (ванильная E, подсказка, подсветка); PartnerName — ник игрока на другом конце (Photon NickName, у крюка нет); Color — цвет связки (Paint из RopePresenterPool.Tint)
│   ├── RopeHands            — взять / отпустить, отпуск при предмете, стене, без сознания
│   ├── RopeRescue           — FreeWhenPulled (bool), FixedUpdate после RopeForces: если тиммейт держит твою верёвку и она натянута (RopeForces.IsPulledByTeammate: не крюк, конец держат, TensionSolver.Load > 0) — как ванильный Reach: IsStuck → UnStick (кактус), SubtractStatus Web / FlyTrap по 1 (паутина, мухоловка)
│   ├── AutoHandUp           — AutoHandUp (bool): пока сматываешь (ReelDriver.IsReelingIn) держимую верёвку, а партнёр Airborne/Unconscious (не мёртв) ближе 3,5 м и до 60° от взгляда, без предмета и не на стене — верёвка отпускается, Target = партнёр, local.data.sincePressReach = 0 каждый кадр (ванильный CharacterGrabbing сам включает isReaching по RPC: анимация, Reach, DragTowards); стоп, когда партнёр стоит / лезет 0,3 с (без сознания — ближе 1,5 м), дальше 4 м, вне 60° или через 3 с
│   ├── ReelDriver           — колесо / кнопки → скорость смотки сглажена как у RopeSpool (Lerp ×4/с, −1…5), стамина, публикация ~10 Гц; PullKey (по умолчанию G, удержание, пока держишь верёвку и нет HasAction) — смотка как кнопкой (ButtonScroll); UnwindKey (по умолчанию T, удержание, пока держишь верёвку) — размотка как кнопкой (+ButtonScroll), PullKey важнее при обеих нажатых
│   ├── AnchorRopes          — длина верёвки крюка: свой — AnchorLength (выдаётся следом до HookLength — локальная настройка, не от хоста; на срыве запирается), чужие — по пути без потолка (у владельца свой HookLength); длины пишутся в HookLengths, а LinkLengths отдаёт RopeForces / RopePresenterPool / отладке длину связки с крюком оттуда, остальным — RopeReel.EffectiveLength; конец крюка «держится», чтобы длина шла через смотку; после пойманного падения (AnchorLength.IsLocked) своя верёвка крюка сматывается со скоростью HookRetractSpeed (AnchorLength.Retract) до ~1 м от крюка, пока не встанешь / не схватишься
│   ├── RopeKnockdown        — сильный рывок (RopeSnatch ≥ KnockdownSpeed) по верёвке, которую ты НЕ держишь, роняет тебя в регдолл ванильным Character.Fall (по сети)
│   ├── RopeForces           — силы своему персонажу, обрыв, висящий без нарастающей гравитации, страховка; LedgeAssist (Core) — застрял под уступом (в воздухе/без сознания, скорость вдоль тяги < 0,3 м/с дольше 0,3 с, стена в 0,6 м по тяге) → тяга без вдавливания в стену + толчок от стены LedgePush; MantleAssist (Core) — вытягивают на край: партнёр держит натянутую верёвку, ты не на земле, кромка (первая точка касания со своей стороны) от −0,5 до 1,5 м над тобой, скорость < 0,5 м/с дольше 0,4 с → от кромки шаг MantleStep к тянущему, луч вниз ищет пол (нормаль ≥ 0,7) → сила MantlePush к точке над полом, пока не встал / 2 с; начатый толчок доводится до конца, даже если верёвка соскочила с кромки (пока партнёр держит); если верёвка не касается кромки (тянущий у самого края), кромкой считается точка на 1 м ниже рук тянущего, пол ищется за ним — в сторону от висящего; MantleAssist работает и для верёвки крюка (кромка без касания = сама точка крюка)
│   ├── RopeHoldPoses        — кто держит верёвку (все игроки) + угол катушки по изменению Reel (200°/м); показываемый угол догоняет целевой (сглаживание чужих рывков сети)
│   └── RopeHoldPose         — actor + угол вращения катушки
├── Visual
│   ├── RopePresenterPool    — верёвки на сцене: создание, отрисовка (невидимые и дальше 40 м от камеры — провис без столкновений точек), путь (span), наведение, материал; Tint — материал верёвки в цвете связки из ESC (RopeRecolor), доля RopeColor (Visual, 0,85)
│   ├── RopeRecolor          — копия материала верёвки на цвет (кэш по цвету). Ванильный M_Rope (шейдер W/Peak_Standard): текстура rope_lmao серая, цвет даёт HDR _BaseColor (3,03; 1,83; 0,16) — ему ставятся оттенок и насыщенность цвета связки при исходной яркости; акценты _Color1/_Color2 — только оттенок
│   ├── RopePresenter        — одна верёвка: RopeWrapPath + RopeSag + RopeLooseness + RopeTubeView + RopeInteractible; при пропаже конца путь сбрасывается
│   ├── SpoolProps           — визуальная копия обычной катушки в руках каждого держащего: центр её Hand_R/Hand_L ставится между физическими кистями, источник точки выхода верёвки; материал катушки (M_RopeKnot, шейдер W/Peak_Rope) перекрашивается в цвет удерживаемой связки через свой RopeRecolor (RopeColor > 0)
│   ├── SpoolProp            — MonoBehaviour на корне копии (ссылки на колесо и точки выхода переносятся при Instantiate из шаблона, собранного один раз): колесо (ropeSpoolTf) вращается при смотке, Paint(color, material) / Unpaint — материал всех рендереров копии (исходный хранится в шаблоне); верёвка как у ванильной: видимый путь ropeStart → ropeBase → физическая часть (ActorStates: ropeBase — точка физики, TryGetRopeLead — ropeStart для отрисовки)
│   ├── CarabinerSounds / LinkSoundFeedback — SFX_Instance → SFX_Player игры; связка с тиммейтом — звук в середине верёвки; крюк — те же щелчки на тоне HookPitch 0,88 и громкости HookVolume 60% из точки крюка (PlayHookLock / PlayHookUnlock)
│   ├── RopeColliderFilter        — верёвка проходит сквозь сундуки (Luggage, в т.ч. LuggageCursed, RespawnChest, и MirageLuggage) и пушку скаута (ScoutCannon), результат кэшируется по коллайдеру; ShellOf — коллайдеры подвесных мостов (BreakableBridge: цепочка тонких BoxCollider под ColliderRoot) → Top; невыпуклый MeshCollider: с материалом «Foliage» → Round (кроны тропиков: Mesh_TreeCrown, Treecrown, PalmMesh — коллайдер = меш листвы); с «Floor» / «maze» в имени и толщиной меньше трети ширины → Slab (полы цитадели Gloom Temple: Floor Solid / Floor Half, генерируемые Outer Floor, Entrance Floor, Central Tower maze); кэш по коллайдеру
│   ├── AnchorVisuals        — модель крюка из ванильной катушки: при наведении — полупрозрачный RopeTier.anchorPreview, у поставленных — в превью вместо прозрачных частей непрозрачный normalPart из RopeTier.anchorPrefab (сам сетевой префаб не создаётся); точка верёвки — центр узла (SphereCollider ребёнка Knot; у ванили AnchorPoint ниже, на уровне кольца), коллайдеры копии удаляются
│   ├── CarabinerHud         — прицел-карабин (материал ванильного прицела, пульсирует при появлении как ванильные — копия ScaleOnEnable с ReticleRope: time и easeType) и подсказки: «Привязать / Отцепить» или «[E] Отпустить верёвку» (клавиша из CharacterInput.action_interact; прячется, если ванильная подсказка уже показывает нашу верёвку), пока держишь верёвку — «[G] Подтянуть» и справа «[T] Размотать»; ник партнёра цветом верёвки справа от ванильного «Верёвка» при наведении, само «Верёвка» тоже красится в этот цвет (исходный цвет interactNameText запоминается и возвращается, когда ника нет) (копия interactNameText вне раскладки — LayoutElement.ignoreLayout, PartnerGap 16; HudPatches берёт currentInteractable); индикатор крюка — HookIndicatorView
│   ├── HudPrompt            — копия ванильной подсказки UI_interactPrompt_E (иконка клавиши + текст в первый активный TMP, не в скрытый PromptHold); строки стопкой раскладывает сам родитель ванильных подсказок, поэтому позиция по строкам не задаётся; Show(line) / Hide; LeaveLayout — вывести из раскладки; PlaceRightOf(other, gap) — «Размотать» каждый кадр ставится на PairGap 40 правее текста «Подтянуть» (пара не центрирована)
│   ├── HudPlacement         — static RightOf(anchor, anchorLast, moved, movedFirst, gap): ставит виджет так, чтобы его первый текст начинался на gap правее конца текста якоря (по textBounds после ForceMeshUpdate); общий для «Размотать» и ника партнёра
│   ├── HookIndicatorView    — индикатор крюка слева снизу от прицела (Assets/hook_indicator.png, 22 px, смещение −26/−26), виден, пока твой крюк стоит; цвет — прицела, оранжевый на полной длине (RepkaColors.HookAtMax), красный, пока верёвка крюка зафиксировала длину при падении (RepkaColors.HookLocked); под иконкой — целые метры шрифтом interactNameText без «м», того же цвета; слева — красная двойная стрелка вверх (Assets/hook_retract.png), пока зафиксированный крюк сматывает тебя к себе; всё приходит одной HookIndicatorState (RepkaHooks.Hook от RepkaHookFeed: AnchorRopes.IsOwnLocked / IsOwnAtMax / IsOwnRetracting / OwnMeters)
│   ├── PlayerListBadges     — ряды цветных карабинов в списке игроков (ESC), проверка наложения на громкость / кик
│   ├── LinkBadgeRow         — ряд цветных карабинов под аватаркой в списке игроков (ESC), материал прицела, проверка наложения на громкость / кик
│   ├── RopeDebugReport      — строки отладки
│   ├── IDebugTools / DebugTools / NoDebugTools — вся отладка за одним инструментом: окно отладки, капсула F9; RepkaBack берёт DebugTools, если BuildFlavor.HasDebugTools, иначе пустой NoDebugTools (релизная конфигурация Public, символ REPKA_PUBLIC; ключи ShowDebugOverlay и TestDummyKey тогда не создаются)
│   ├── ConfigWindowInput    — клавиша окна настроек (ConfigEditorKey) во всех сборках: debug — F7, релиз — по умолчанию пусто (KeyBinding с пустым путём не срабатывает); пока окно открыто, RepkaBack не тикает G/T
│   ├── ConfigEditorWindow   — View (IMGUI): ConfigEditorKey — окно всех записей конфига: bool — галочка, enum — кнопка-перебор, остальное — текст; рядом метка, на кого действует (SettingScopes: на тебя / когда тянешь / хост → всем / только у тебя); «Применить» пишет SetSerializedValue и сохраняет .cfg, «Сбросить» возвращает текущие; пока открыто — курсор свободен, ввод игры и G/T заблокированы
│   └── RopeDebugOverlay     — View: текст отладки (ShowDebugOverlay; содержимое строит RopeDebugReport — ты, лобби, по каждой своей верёвке: растяжение, нагрузка/потолок MaxPull, тяга на тебя, оценка на партнёра, сматывание с помощниками; описание в DEBUG.md)
└── Patches
    ├── RepkaHooks           — статический мост Back → патчи
    ├── RepkaHookFeed        — каждый кадр пишет в RepkaHooks данные для патчей (цель G, подсказка, держимая / наведённая верёвка, длина), Clear при выходе
    ├── InteractionPatches   — Interaction.DoInteractableRaycasts: верёвка под прицелом — только если игра ничего не нашла и вызов обычный (минимальный приоритет, порядок — в PLAN.md); при цели G верёвка не наводится
    ├── HudPatches           — тонкий патч: GUIManager.Start / UpdateReticle → CarabinerHud, кольцо удержания G (UI_UseItemProgress); если текст ванильной подсказки E по нашей верёвке устарел (игра обновляет его только при смене объекта) — RefreshInteractablePrompt, UpdateWindowStatus — курсор и блок ввода при открытом окне настроек
    ├── PlayerListPatches    — тонкий патч: AudioLevels.Update → PlayerListBadges
    ├── RopeGaugePatches     — GUIManager.UpdateRope: пока держишь нашу верёвку — ванильный UI_Rope, текущая длина (HeldRopeLength от Back), цвет всех картинок индикатора с шейдером Scouts/UI_Rope (верёвка и её конец) смешивается с цветом связки по RopeColor (RopeGaugeTint, у каждой картинки свой исходный цвет; спрайты серые, Update меняет только альфу), иначе исходные цвета возвращаются
    ├── CameraPatches        — MainCameraMovement.CharacterCam (постфикс): при StableCamera и привязке к любой верёвке поворот камеры = взгляд + тряска Gamefeel, без подмешивания вращения головы рэгдолла (ragdollCam); позиция ванильная
    ├── HandUpPatches        — CharacterGrabbing.TargetCanBeHelped(target, helper) (постфикс): true, если у тебя AutoHandUp тянет этого партнёра (RepkaHooks.HandUpTarget), или ты сам беспомощен (LocalHelpless) и helper привязан к тебе верёвкой (TiedPartners) — тогда ванильная рука тянет тебя и без карабканья
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
| `RopeTubeView` | `src/Repkarbinhake.Unity/RopeTubeView.cs` | View верёвки | `Initialize(filter, renderer, material, radius, highlightEmission)`, `Render(points)`, `Highlight`, `Unhighlight`, `ChangeMaterial(material)` |
| `PhysicsRopeObstacles` | `src/Repkarbinhake.Unity/PhysicsRopeObstacles.cs` | Обход рельефа точками верёвки: проверка по ходу движения + выталкивание из геометрии | ctor `(layerMask, probe SphereCollider)`; `Resolve(from, to, radius)` |
| `RopeSegmentation` | `src/Repkarbinhake.Core/Visual/RopeSegmentation.cs` | Кол-во костей: `clamp(ceil(length / SegmentLength), 4, Cap)`; `Cap` = `FixedSegments` или `SegmentBudget / кол-во верёвок` в пределах `AutoMaxSegments` | `Count(length, ropeCount)`, `Cap(ropeCount)` |

### Plugin
| Class | File | Role | Key API |
|---|---|---|---|
| `RepkaConfig` | `src/Repkarbinhake.Plugin/Data/RepkaConfig.cs` | BepInEx config, редактируется ModConfig | `LinkKey`, `LinkAimAngle`, `Thickness` + `IRopeSettings` + `ISegmentSettings` (секция `Performance`) |
| `CharacterDirectory` | `src/Repkarbinhake.Plugin/Links/CharacterDirectory.cs` | Поиск персонажа по Photon actor (кэш, обновляется раз в кадр) | `Refresh`, `LocalActor`, `TryFind`, static `ActorOf(character)` (нужен и патчам), `IsActive` |
| `LinkTargetPicker` | `src/Repkarbinhake.Plugin/Input/LinkTargetPicker.cs` | Ближайший к прицелу игрок в радиусе взаимодействия игры (`Interaction.distance`, как подобрать нокнутого) | ctor `(config, dummy)`; `TryPick(self, view, out int targetActor)` |
| `AutoHandUp` | `src/Repkarbinhake.Plugin/Handling/AutoHandUp.cs` | Авто-рука: отпускает верёвку и держит ванильный reach за тебя, пока беспомощный тиммейт не встанет | `Tick(local, actor, reelingIn, dt)`, `Stop`, `Target`, static `IsHelpless(footing)` |
| `RopeRescue` | `src/Repkarbinhake.Plugin/Handling/RopeRescue.cs` | Натянутая тиммейтом верёвка вытаскивает из кактуса, паутины, мухоловки | `Tick(local, pulledByTeammate)` |
| `HookLengths` / `LinkLengths` | `src/Repkarbinhake.Plugin/Links/` | Своя длина верёвки крюка (HookLength) поверх общей длины связки | `HookLengths.Set/TryGet/Clear`; `LinkLengths.EffectiveLength(link)` |
| `HookIndicatorView` | `src/Repkarbinhake.Plugin/Visual/HookIndicatorView.cs` | Иконка крюка, метры, стрелка смотки | `Attach(gui)`, `Show(HookIndicatorState)` |
| `HudPrompt` / `HudPlacement` | `src/Repkarbinhake.Plugin/Visual/` | Копия ванильной подсказки; выравнивание текста справа от другого | `Show(line)`, `Hide`, `LeaveLayout`, `PlaceRightOf(other, gap)`; `HudPlacement.RightOf(...)` |

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
