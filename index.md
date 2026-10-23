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

Repkarbinhake.Plugin (BepInEx 5, Harmony; деплой одним файлом RopeCarabiner.dll в BepInEx/plugins/RopeCarabiner; старая папка plugins/Repkarbinhake при сборке удаляется)
├── RepkaPlugin              — точка входа: конфиг, ассеты, Harmony, создаёт RepkaBack (DontDestroyOnLoad)
├── RepkaBack                — Back (~220 строк): собирает инструменты, порядок кадра, хуки для патчей, смерть
├── Data
│   ├── SettingScopes        — static-таблица: ключ конфига → на кого действует (для окна F7; неизвестные — «только у тебя»); метки — SettingScope, текст на языке игры через RepkaTexts.ScopeName
│   ├── RopeSettingsOverride — IRopeSettings поверх обычной верёвки с заменой Stiffness / Damping / MaxPull (null — как у обычной): верёвка крюка (AnchorStiffness 300, AnchorDamping 20, AnchorMaxPull 250) и подтягивание (HaulStiffness 180); RopeForces и отладка берут под них свои TensionSolver
│   ├── ReelInWatch          — у тянущегося: сматывает ли держащий партнёр верёвку (его HeldReel вырос за кадр — держится 0,5 с, конец публикуется ~10 Гц); RopeForces на это время берёт для связки TensionSolver с HaulStiffness — меньше растяжения при подтягивании
│   ├── RepkaConfig          — ConfigEntry (ModConfig) → IRopeSettings / ISegmentSettings; при запуске удаляет из .cfg записи, которых мод больше не читает (OrphanedEntries через AccessTools)
│   ├── LobbyRopeSettings    — настройки с длиной верёвки от хоста
│   ├── HostFlag             — флаг «хост → всем» (AllowHooks, LockLinks): хост — своя настройка, остальные — значение хоста из свойства комнаты (Key: rkb.cfg.allowHooks / rkb.cfg.lockLinks, "1"/"0"), пока хост не прислал — fallback (крюки true, замок false); Changed(bool) — когда действующее значение сменилось (у клиента от хоста, у хоста при правке своей настройки); RoomLinkSync публикует все флаги при получении роли хоста и при смене настройки, читает их из свойств комнаты
│   ├── VanillaRopeSpool     — обычная (не анти) катушка из ItemDatabase: Item для поз рук, материал верёвки
│   ├── RepkaAssets / WavReader — ресурсы, встроенные в RopeCarabiner.dll (EmbeddedResource «Repkarbinhake.Assets.<файл>» из src/Repkarbinhake.Plugin/Assets): PNG → Sprite, WAV → AudioClip; нет ресурса — исключение. Мод — одна DLL: Core и Unity компилируются в плагин исходниками (Compile Include с LinkBase)
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
│   ├── TestDummy / TestDummies — отладочные капсулы (Rigidbody 60 кг): TestDummies хранит поставленные, актёры −1 000 000, −1 000 001… (IsDummy: ≤ FirstActor; не крюки — AnchorActors только −1…−9999); PlaceRing ставит N штук кольцом радиусом 2,5 м с лучом вниз до земли; ActorStates, RopeForces.ApplyToDummies, LinkTargetPicker и отчёт отладки работают со всеми
│   ├── AnchorBoard          — крюки игроков (владелец → место): связка игрок ↔ крюк (крюк = актёр −номер, верёвка локальная у каждого клиента), точка верёвки на крюке, EnsureLinks; Remove сначала развязывает, потом убирает крюк — чтобы звук отстёгивания нашёл точку
│   └── LinkColors           — цвет каждой связки из палитры RepkaColors (первый свободный при появлении, свой у каждого клиента; номер связки — первый свободный без ограничения (11, 12…), цвет — палитра по кругу (номер mod 10); верёвки крюка T слот не занимают и номера не сдвигают); TryGetColor — цвет верёвки (у крюков нет); CollectFor(actor, List<LinkBadge>) — цвет и номер (слот палитры + 1, одинаковый в строках обоих игроков связки) для списка игроков
├── Input
│   ├── LinkHoldInput        — G: цель по взгляду, удержание, связать / отцепить; HasAction — цель есть и это не «держишь верёвку и смотришь на того, с кем связан» (тогда G подтягивает, а не отвязывает); пока хост закрыл сцепку (LockLinks), цели нет и удержание сброшено — нельзя ни связать, ни отцепить; событие Untied(partner) при ручном отцеплении
│   ├── TestDummyInput       — F9 (TestDummyKey): поставить кольцо из TestDummyCount (по умолчанию 8, до 16) капсул вокруг точки по лучу до 30 м / убрать все; связки с пропавшими капсулами развязываются
│   ├── HostSwitchInput      — клавиши хоста AllowHooksKey / LockLinksKey (по умолчанию пусто): нажатие у хоста переключает AllowHooks / LockLinks, дальше как при смене в настройках — публикация в комнату и сообщение в ленте; у не-хоста ничего не делает; опрашивается в Update, пока окно настроек закрыто
│   ├── AnchorInput          — T: удержание на земле (Footing.Grounded; оторвался — прицел сброшен) — призрак крюка в точке прицела (до 10 м, ставится до 5 м, как у ванильной катушки), отпустил — крюк; нажатие при стоящем — снять; снимается при смерти и дальше SnapDistance; пока держишь верёвку и UnwindKey совпадает с AnchorKey — клавиша отдана разматыванию (keyTaken: прицел сброшен, крюк не ставится и не снимается, после отпускания верёвки T ждёт отпускания клавиши)
│   ├── LinkTargetPicker     — на кого смотрит игрок для G, отдаёт actor
│   └── KeyBinding           — InputAction из строки конфига, пересоздаётся при смене
├── Handling
│   ├── RopeInteractible     — IInteractible игры на верёвке (ванильная E, подсказка, подсветка); PartnerName — ник игрока на другом конце (Photon NickName, у крюка нет); Color — цвет связки (Paint из RopePresenterPool.Tint)
│   ├── RopeHands            — взять / отпустить, отпуск при предмете, стене, без сознания
│   ├── (RopeForces) ClimbHaulForce — ты Climbing, тиммейт держит твою натянутую верёвку и сматывает её (ReelInWatch) → Character.DragTowards(центр + направление верёвки, ClimbHaulForce) раз за FixedUpdate: игра двигает точку лазания climbPos (CharacterClimbing.GetDragged, как ванильная рука), иначе пружина лазания держит тело у стены и сила верёвки его не поднимает
│   ├── RopeRescue           — FreeWhenPulled (bool), FixedUpdate после RopeForces: если тиммейт держит твою верёвку и она натянута (RopeForces.IsPulledByTeammate: не крюк, конец держат, TensionSolver.Load > 0) — как ванильный Reach: IsStuck → UnStick (кактус), SubtractStatus Web / FlyTrap по 1 (паутина, мухоловка)
│   ├── HangRecovery         — RecoverWhenHeld (bool), FixedUpdate после RopeForces: ты Airborne, в рэгдолле (data.fallSeconds > 0) и висишь на натянутой верёвке, которую держит тиммейт (RopeForces.IsPulledByTeammate: не крюк, конец держат, Load > 0) → RPC RPCA_UnFall всем и сразу currentRagdollControll = GetTargetRagdollControll() (без плавного возврата 0,5/с — иначе первые ~0,5 с за скалу не схватиться); никто не держит (свободный конец, крюк) — рэгдолл идёт как в игре; обморок (Unconscious) не трогает; IsHanging (беспомощен + натянутая верёвка) — для камеры
│   ├── AutoHandUp           — AutoHandUp (bool): только пока жмёшь «подтянуть» в этом кадре (ReelDriver.IsReelingIn — целевая скорость смотки < 0: G или колесо на себя, а не затухающая скорость) по держимой верёвке, а партнёр Airborne/Unconscious или Climbing (не мёртв) ближе 3,5 м и до 60° от взгляда, без предмета и не на стене — верёвка отпускается, Target = партнёр, local.data.sincePressReach = 0 каждый кадр (ванильный CharacterGrabbing сам включает isReaching по RPC: анимация, Reach, DragTowards); стоп, когда партнёр стоит на земле 0,3 с (без сознания — ближе 1,5 м), дальше 4 м, вне 60° или через 3 с; если рука сорвалась (дальше 4 м, вне 60° или 3 с), а руки пусты и стойка позволяет — верёвка той же связки снова берётся в руки (GrabBack); смерть партнёра — без повторного захвата
│   ├── ReelDriver           — колесо / кнопки → скорость смотки сглажена как у RopeSpool (Lerp ×4/с, −1…5), стамина — только основная (data.currentStamina, UseStamina без бонусной), кончилась основная — смотка под нагрузкой стоит, публикация ~10 Гц; PullKey (по умолчанию G, удержание, пока держишь верёвку и нет HasAction) — смотка как кнопкой (ButtonScroll); UnwindKey (по умолчанию T, удержание, пока держишь верёвку) — размотка как кнопкой (+ButtonScroll), PullKey важнее при обеих нажатых
│   ├── AnchorRopes          — длина верёвки крюка: свой — AnchorLength (выдаётся следом до HookLength — локальная настройка, не от хоста; на срыве запирается), чужие — по пути без потолка (у владельца свой HookLength); длины пишутся в HookLengths, а LinkLengths отдаёт RopeForces / RopePresenterPool / отладке длину связки с крюком оттуда, остальным — RopeReel.EffectiveLength; конец крюка «держится», чтобы длина шла через смотку; после пойманного падения (AnchorLength.IsLocked) своя верёвка крюка сматывается со скоростью HookRetractSpeed (AnchorLength.Retract) до ~1 м от крюка, пока не встанешь / не схватишься
│   ├── RopeKnockdown        — сильный рывок (RopeSnatch ≥ KnockdownSpeed) по верёвке, которую ты НЕ держишь, когда ты стоишь на земле (data.isGrounded; в воздухе и на стене — нет, иначе подтягиваемого отрывало от скалы: игра бросает лазание при currentRagdollControll < 0,25), роняет тебя в регдолл ванильным Character.Fall (по сети)
│   ├── RopeForces           — силы своему персонажу, обрыв, висящий без нарастающей гравитации, страховка; LedgeAssist (Core) — застрял под уступом (в воздухе/без сознания, скорость вдоль тяги < 0,3 м/с дольше 0,3 с, стена в 0,6 м по тяге) → тяга без вдавливания в стену + толчок от стены LedgePush; MantleAssist (Core) — вытягивают на край: партнёр держит натянутую верёвку, ты не на земле, кромка (первая точка касания со своей стороны) от −0,5 до 1,5 м над тобой, скорость < 0,5 м/с дольше 0,4 с → от кромки шаг MantleStep к тянущему, луч вниз ищет пол (нормаль ≥ 0,7) → сила MantlePush к точке над полом, пока не встал / 2 с; начатый толчок доводится до конца, даже если верёвка соскочила с кромки (пока партнёр держит); если верёвка не касается кромки (тянущий у самого края), кромкой считается точка на 1 м ниже рук тянущего, пол ищется за ним — в сторону от висящего; MantleAssist работает и для верёвки крюка (кромка без касания = сама точка крюка)
│   ├── RopeHoldPoses        — кто держит верёвку (все игроки) + угол катушки по изменению Reel (200°/м); показываемый угол догоняет целевой (сглаживание чужих рывков сети)
│   └── RopeHoldPose         — actor + угол вращения катушки
├── Visual
│   ├── RopePresenterPool    — верёвки на сцене: создание, отрисовка (невидимые и дальше 40 м от камеры — провис без столкновений точек), путь (span), наведение, материал; Tint — материал верёвки в цвете связки из ESC (RopeRecolor), доля RopeColor (Visual, 0,85); перекрашиваются база и все слои игровой верёвки (_Color1, _Color21, _Color3 — раньше вместо _Color21/_Color3 был несуществующий _Color2, и рыжий _Color3 делал все верёвки красными)
│   ├── RopeRecolor          — копия материала верёвки на цвет (кэш по цвету). Ванильный M_Rope (шейдер W/Peak_Standard): текстура rope_lmao серая, цвет даёт HDR _BaseColor (3,03; 1,83; 0,16) — ему ставятся оттенок и насыщенность цвета связки при исходной яркости; акценты _Color1/_Color2 — только оттенок
│   ├── RopePresenter        — одна верёвка: RopeWrapPath + RopeSag + RopeLooseness + RopeTubeView + RopeInteractible; при пропаже конца путь сбрасывается
│   ├── SpoolProps           — визуальная копия обычной катушки в руках каждого держащего: центр её Hand_R/Hand_L ставится между физическими кистями, источник точки выхода верёвки; материал катушки (M_RopeKnot, шейдер W/Peak_Rope) перекрашивается в цвет удерживаемой связки через свой RopeRecolor (RopeColor > 0)
│   ├── SpoolProp            — MonoBehaviour на корне копии (ссылки на колесо и точки выхода переносятся при Instantiate из шаблона, собранного один раз): колесо (ropeSpoolTf) вращается при смотке, Paint(color, material) / Unpaint — материал всех рендереров копии (исходный хранится в шаблоне); верёвка как у ванильной: видимый путь ropeStart → ropeBase → физическая часть (ActorStates: ropeBase — точка физики, TryGetRopeLead — ropeStart для отрисовки)
│   ├── CarabinerSounds / LinkSoundFeedback — SFX_Instance → SFX_Player игры; связка с тиммейтом — звук в середине верёвки; крюк — те же щелчки на тоне HookPitch 0,88 и громкости HookVolume 60% из точки крюка (PlayHookLock / PlayHookUnlock)
│   ├── RopeEndMask          — невидимая маска у тебя: точки твоих не держимых верёвок (в т.ч. страховки T) внутри сферы радиусом 31,5 см вокруг точки выхода у таза сдвигаются на её границу, так концы не видны и не клипаются друг в друга; число точек не меняется (меш не перестраивается); RopePresenterPool.MaskOf передаёт центр только для своих не держимых концов
│   ├── RopeEventLog         — сообщения мода в ленте подключений игры (PlayerConnectionLog) с префиксом [Rope Carabiner] на языке игры (RepkaTexts): «A отцепился от игрока/игроков: B, C» (все отцепления одного игрока за кадр одной строкой, Flush в конце Update) и смена настроек хоста: «Хост разрешил/запретил сцепляться и расцепляться» (LockLinks), «Хост разрешил/запретил ставить страховку (T)» (AllowHooks); разрешение — цветом входа, запрет — цветом выхода; клиент видит их и при входе, если хост уже отличается от значения по умолчанию; то же на английском в лог BepInEx
│   ├── UntieWitness         — кто отцепился по сетевому обновлению: автор записи (PhotonLinkTransport.DispatchingWriter) — один из двух игроков связки, оба живы и в комнате, не дальше 90% SnapDistance друг от друга (иначе это смерть, уход или обрыв); свои ручные отцепления приходят от LinkHoldInput.Untied
│   ├── OwnRopeAim           — выбор своей верёвки взглядом (RopePresenterPool.TryAim): у каждой своей верёвки отрезаются первые 0,6 м от тебя (там все твои верёвки идут пучком от бедра), по оставшемуся — RopeRayPicker с RopeAimReach / RopeAimTolerance; из попавших берётся та, у которой точка в 1,5 м дальше по верёвке лучше совпадает с направлением взгляда (а не ближайшая по лучу) — когда двое висят под тобой, берётся та, куда смотришь
│   ├── BeltCarabiners       — карабины на поясе (BeltCarabiners, вкл.): для каждого игрока и каждой его связки (в т.ч. с крюком) — BeltCarabinerView; порядок по ключу связки, раскладка по таблице мест: верхний ряд — пары ±40° (перед бёдер), ±85°, ±130°; нижний ряд на 11 см ниже, шахматкой между верхними и заполняется в обратную сторону, сзади к переду — ±155°, ±107,5°, ±62,5°, 0° (центр спереди); места заполняются по порядку, но в каждое берётся первая связка, цвет которой не совпадает с уже повешенными соседями ближе 50° (в своём и соседнем ряду), иначе первая по порядку; связки сверх 13 карабина не получают (верёвка идёт от тела); карабин страховки T всегда ровно сзади по центру (180°), это место зарезервировано — по эллипсу кольца талии из BeltRing + 3 см над кожей (с запасом на одежду); каждый карабин — маятник Verlet от точки подвеса (затухание 0,94), середина и низ не проходят внутрь живота (BeltRing.Frame.KeepOutside) и выталкиваются из коллайдеров тела (BodyContacts); низ всегда отклонён наружу минимум на ~25° (не уходит под рубашку); повёрнут плоскостью наружу; RopePresenterPool.Render берёт для не держащего конца точку верёвки у низа карабина (TryGetRopePoint), физика тяги по-прежнему от тела; масштаб 2 (демонстративно крупный); на своём персонаже карабины не создаются (LocalActor пропускается, кроме настройки Visual/ShowOwnCarabiners (по умолчанию выключена) — тогда маска RopeEndMask для них не включается), верёвка выходит из таза
│   ├── BeltRing             — кольцо талии персонажа: кости WaistF1/L1/R1 и их _end из mainRenderer.bones (кэш по персонажу, Prune чистит удалённых); Frame — центр, Up (Hip→Torso), векторы до поверхности спереди/слева/справа, Offset(угол) по эллипсу, KeepOutside (эллиптический цилиндр от −0,3 до +0,12 м); без костей — фиксированный пояс 0,21 × 0,28 м и предупреждение в лог
│   ├── BodyContacts         — выталкивание точки из коллайдеров тела (ragdoll.colliderList) через Physics.ComputePenetration зондом-сферой на слое Ignore Raycast; на физику игры не влияет
│   ├── CarabinerMaterials   — материалы карабина на игровом шейдере W/Peak_Standard (у материала верёвки W/Peak_Rope текстуру не выключить): _BaseTexAmount 0, _VertexColorAmount 0, один цвет на базу и все три слоя; тело — цвет связки с насыщенностью 1 (серые остаются серыми), металличность 0,15, гладкость 0,35 (без белого блика), кэш по цвету; у крюка — отдельный металлический (ForHook: тёмная сталь, металличность 0,8), вне палитры и номеров; защёлка — сталь, муфта — тёмная сталь, ромбики насечки (подмеш KnurlSubmesh) — светлая сталь; текстуры не используются (_BaseTexture этого шейдера на нашем материале не проявилась); у всех материалов _UseUV2 0, болтик тёмный; шейдера нет — URP Lit и предупреждение в лог
│   ├── CarabinerModel / ObjMeshReader — модель из встроенного Assets/carabiner.obj (исходник tools/models/carabiner.blend, ~1070 треугольников): OBJ в Mesh с переводом в левостороннюю систему (−X, обратный обход), подмеши по материалам: тело Carabiner_Metal (0), защёлка Carabiner_Gate (1), муфта Carabiner_Sleeve (2) и болтик Carabiner_Bolt (3); точка подвеса сдвинута в верх карабина, HangLength — до низа
│   ├── RopeColliderFilter        — верёвка проходит сквозь сундуки (Luggage, в т.ч. LuggageCursed, RespawnChest, и MirageLuggage) и пушку скаута (ScoutCannon), результат кэшируется по коллайдеру; ShellOf — коллайдеры подвесных мостов → Top: BreakableBridge или JungleVine с colliderType = Box (цепочка тонких BoxCollider под ColliderRoot; так ловятся и неразрушаемые мосты без BreakableBridge — подвесной мост Надира «HangBridge Void» и мосты между платформами на деревьях «HangBridgeTreePlatformForest»; лианы и цепи Надира — JungleVine с капсулами-триггерами, их правило не трогает); невыпуклый MeshCollider: с материалом «Leaves» → Round (только кроны с листвой: Mesh_TreeCrown, Treecrown — M_FoliagePack TreeLeaves, 6–16 м; раньше признак был «Foliage», и под него попадали кора стволов M_FoliagePack Bark до 32 м, кактусы, стволы пальм и мангровое дерево биома Мгла (Gloom, коллайдер Roots ~60 м, M_Foliage_MangroveTree) — верёвку выталкивало на их внешнюю сторону; теперь они огибаются как камень); с «Floor» / «maze» в имени и толщиной меньше трети ширины → Slab (полы цитадели Gloom Temple: Floor Solid / Floor Half, генерируемые Outer Floor, Entrance Floor, Central Tower maze); кэш по коллайдеру
│   ├── AnchorVisuals        — модель крюка из ванильной катушки: при наведении — полупрозрачный RopeTier.anchorPreview, у поставленных — в превью вместо прозрачных частей непрозрачный normalPart из RopeTier.anchorPrefab (сам сетевой префаб не создаётся); точка верёвки — центр узла (SphereCollider ребёнка Knot; у ванили AnchorPoint ниже, на уровне кольца), коллайдеры копии удаляются
│   ├── CarabinerHud         — прицел-карабин (материал ванильного прицела, пульсирует при появлении как ванильные — копия ScaleOnEnable с ReticleRope: time и easeType) и подсказки: «Привязать / Отцепить» или «[E] Отпустить верёвку» (клавиша из CharacterInput.action_interact; прячется, если ванильная подсказка уже показывает нашу верёвку), пока держишь верёвку — «[G] Подтянуть» и справа «[T] Размотать»; ник партнёра цветом верёвки справа от ванильного «Верёвка» при наведении, само «Верёвка» тоже красится в этот цвет (исходный цвет interactNameText запоминается и возвращается, когда ника нет) (копия interactNameText вне раскладки — LayoutElement.ignoreLayout, PartnerGap 16; HudPatches берёт currentInteractable); индикатор крюка — HookIndicatorView
│   ├── HudPrompt            — копия ванильной подсказки UI_interactPrompt_E (иконка клавиши + текст в первый активный TMP, не в скрытый PromptHold); строки стопкой раскладывает сам родитель ванильных подсказок, поэтому позиция по строкам не задаётся; Show(line) / Hide; LeaveLayout — вывести из раскладки; PlaceRightOf(other, gap) — «Размотать» каждый кадр ставится на PairGap 40 правее текста «Подтянуть» (пара не центрирована)
│   ├── HudPlacement         — static RightOf(anchor, anchorLast, moved, movedFirst, gap): ставит виджет так, чтобы его первый текст начинался на gap правее конца текста якоря (по textBounds после ForceMeshUpdate); общий для «Размотать» и ника партнёра
│   ├── HookIndicatorView    — индикатор крюка слева снизу от прицела (Assets/hook_indicator.png, 22 px, смещение −26/−26), виден, пока твой крюк стоит; цвет — прицела, оранжевый на полной длине (RepkaColors.HookAtMax), красный, пока верёвка крюка зафиксировала длину при падении (RepkaColors.HookLocked); под иконкой — целые метры шрифтом interactNameText без «м», того же цвета; слева — красная двойная стрелка вверх (Assets/hook_retract.png), пока зафиксированный крюк сматывает тебя к себе; всё приходит одной HookIndicatorState (RepkaHooks.Hook от HudFeed: AnchorRopes.IsOwnLocked / IsOwnAtMax / IsOwnRetracting / OwnMeters)
│   ├── PlayerListBadges     — ряды цветных карабинов в списке игроков (ESC), проверка наложения на громкость / кик
│   ├── LinkBadgeRow         — ряд цветных карабинов под аватаркой в списке игроков (ESC), материал прицела, проверка наложения на громкость / кик; поверх каждой иконки — номер связки цифрами цвета иконки с тонким чёрным контуром (0,1) шрифтом и материалом текста громкости той же строки (slider.percent — материал HUD-шрифта в этом canvas не рисуется) (до 17 px, автоуменьшение до 9 для двузначных, выравнивание Midline, иконка компенсирует зеркальную аватарку-родителя (вместе с цифрой), контур через ключевое слово OUTLINE_ON материала, текст меняется только при смене номера)
│   ├── RopeDebugReport      — строки отладки
│   ├── IDebugTools / DebugTools / NoDebugTools — вся отладка за одним инструментом: окно отладки, капсула F9; RepkaBack берёт DebugTools, если BuildFlavor.HasDebugTools, иначе пустой NoDebugTools (релизная конфигурация Public, символ REPKA_PUBLIC; ключи ShowDebugOverlay и TestDummyKey тогда не создаются)
│   ├── ConfigWindowInput    — клавиша окна настроек (ConfigEditorKey) во всех сборках: debug — F7, релиз — по умолчанию пусто (KeyBinding с пустым путём не срабатывает); пока окно открыто, RepkaBack не тикает G/T
│   ├── ConfigEditorWindow   — View (IMGUI): ConfigEditorKey — окно всех записей конфига: bool — галочка, enum — кнопка-перебор, остальное — текст; рядом метка, на кого действует (SettingScopes: на тебя / когда тянешь / хост → всем / только у тебя); «Применить» пишет SetSerializedValue и сохраняет .cfg, «Сбросить» возвращает текущие; пока открыто — курсор свободен, ввод игры и G/T заблокированы; заголовок, кнопки и подсказка внизу — на языке игры (RepkaTexts)
│   └── RopeDebugOverlay     — View: текст отладки (ShowDebugOverlay; содержимое строит RopeDebugReport — ты, лобби, по каждой своей верёвке: растяжение, нагрузка/потолок MaxPull, тяга на тебя, оценка на партнёра, сматывание с помощниками; описание в DEBUG.md)
└── Patches
    ├── RepkaHooks           — статический мост Back → патчи
    ├── RepkaHookFeed        — каждый кадр пишет в RepkaHooks данные для патчей через три фида, Clear при выходе:
    │   ├── HudFeed          — подсказки (связать/развязать/отпустить/подтянуть/размотать и их клавиши), прицел-карабин и прогресс удержания, индикатор крюка
    │   ├── HeldRopeFeed     — держимая и наведённая верёвка, её длина и цвет, список привязанных партнёров
    │   └── AssistFeed       — стабильная камера (StableCamera, CameraHanging) и авто-рука (HandUpTarget, LocalNeedsHand)
    ├── InteractionPatches   — Interaction.DoInteractableRaycasts: верёвка под прицелом — только если игра ничего не нашла и вызов обычный (минимальный приоритет, порядок — в PLAN.md); при цели G верёвка не наводится
    ├── HudPatches           — тонкий патч: GUIManager.Start / UpdateReticle → CarabinerHud; префикс SetReticle подменяет любой ненулевой ванильный прицел на карабин, пока ShowCarabinerReticle (иначе прицел каждый кадр выключался и включался, и ScaleOnEnable держал его в масштабе 0), кольцо удержания G (UI_UseItemProgress); если текст ванильной подсказки E по нашей верёвке устарел (игра обновляет его только при смене объекта) — RefreshInteractablePrompt, UpdateWindowStatus — курсор и блок ввода при открытом окне настроек
    ├── PlayerListPatches    — тонкий патч: AudioLevels.Update → PlayerListBadges
    ├── RopeGaugePatches     — GUIManager.UpdateRope: пока держишь нашу верёвку — ванильный UI_Rope, текущая длина (HeldRopeLength от Back), цвет всех картинок индикатора с шейдером Scouts/UI_Rope (верёвка и её конец) смешивается с цветом связки по RopeColor (RopeGaugeTint, у каждой картинки свой исходный цвет; спрайты серые, Update меняет только альфу), иначе исходные цвета возвращаются
    ├── CameraPatches        — MainCameraMovement.CharacterCam (постфикс) → HangingCamera.Apply: при StableCamera и привязке к любой верёвке поворот камеры = взгляд (без вращения головы рэгдолла, ragdollCam) + тряска Gamefeel; пока висишь на натянутой верёвке (HangRecovery.IsHanging → RepkaHooks.CameraHanging) — без тряски, а позиция сглаживается SmoothDamp 0,18 с с отставанием не больше 0,6 м (ванильная камера держится в 0,1 м от головы/торса и передаёт все рывки); после висения сглаживание доводит камеру до ванильной позиции и отключается
    ├── HandUpPatches        — CharacterGrabbing.TargetCanBeHelped(target, helper) (постфикс): true, если у тебя AutoHandUp тянет этого партнёра (RepkaHooks.HandUpTarget), или ты сам беспомощен или лезешь по стене (LocalNeedsHand) и helper привязан к тебе верёвкой (TiedPartners) — тогда ванильная рука тянет тебя и без карабканья
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
| `AutoHandUp` | `src/Repkarbinhake.Plugin/Handling/AutoHandUp.cs` | Авто-рука: отпускает верёвку и держит ванильный reach за тебя, пока беспомощный тиммейт не встанет; при срыве берёт верёвку обратно | `Tick(local, actor, reelingIn, dt)`, `Stop`, `Target`, static `IsHelpless(footing)` |
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
