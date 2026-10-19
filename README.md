# Repkarbinhake

Мод для PEAK (BepInEx 5): игроки связываются физичной верёвкой, сматывают её, страхуют и вытягивают друг друга.

## Что умеет

- **Связка** — G на тиммейта; верёвка огибает камни, кроны деревьев, полы цитадели и подвесные мосты снаружи.
- **Верёвка в руках** — E взять / отпустить, G подтянуть, T размотать, колесо мыши; держащий на земле стоит устойчиво, тянуть вдвоём-втроём легче.
- **Страховка** — сорвавшийся повисает на верёвке; у края его выталкивает на пол (`MantleStep` / `MantlePush`), подтянутого ближе 3,5 м тянущий сам подхватывает ванильной рукой (`AutoHandUp`); натянутая верёвка вытаскивает из кактуса, паутины и мухоловки (`FreeWhenPulled`).
- **Крюк T** — вбить в стену стоя на земле; верёвка до `HookLength` (своя у каждого) идёт следом, ловит срыв и сматывает обратно к крюку; у прицела значок с метрами и цветом состояния.
- **Удобство** — подсказки клавиш на языке игры, ник партнёра цветом верёвки, цвета связок в списке игроков (ESC), окно настроек в игре, защита от укачивания камерой (`StableCamera`).

Все настройки с описанием — [CONFIG.md](CONFIG.md), отладка — [DEBUG.md](DEBUG.md).

## Структура

| Папка | Что внутри |
|---|---|
| `src/Repkarbinhake.Core` | Логика без Unity: связки, сеть (протокол room properties), физика натяжения, смотка, липкий путь верёвки |
| `src/Repkarbinhake.Unity` | Общий Unity-слой мода и песочницы (меш верёвки, коллизия с рельефом) |
| `src/Repkarbinhake.Plugin` | Сам мод BepInEx: Back, ввод, патчи Harmony, визуал |
| `tests/Repkarbinhake.Core.Tests` | xUnit-тесты Core |
| `Sandbox` | Unity 6000.3.12 проект-песочница для отладки физики и визуала вне игры |

Подробная карта классов — [index.md](index.md), план и решения — [PLAN.md](PLAN.md), подозрительные места — [fix-index.md](fix-index.md).

## Сборка

Нужны .NET SDK 10 и установленная PEAK с BepInEx 5.4.23.

Путь к игре по умолчанию — `G:\SteamLibrary\steamapps\common\PEAK`. Если у тебя другой, передай его параметром:

```bash
dotnet build src/Repkarbinhake.Plugin -p:GameDirectory="D:\Steam\steamapps\common\PEAK"
```

Мод собирается в **один файл** `RopeCarabiner.dll`: исходники `Repkarbinhake.Core` и `Repkarbinhake.Unity` компилируются прямо в плагин, а картинки и звуки из `src/Repkarbinhake.Plugin/Assets` встраиваются в DLL как ресурсы (`Repkarbinhake.Assets.<файл>`). Сборка плагина (кроме `Public`) сама кладёт эту DLL в `BepInEx/plugins/RopeCarabiner` игры и удаляет старую папку `BepInEx/plugins/Repkarbinhake` (там лежали прошлые `Repkarbinhake.dll`, `Repkarbinhake.Core.dll`, `Repkarbinhake.Unity.dll` и `Assets`). Проекты Core и Unity по-прежнему собираются отдельно для тестов и песочницы: сборка `Repkarbinhake.Unity` кладёт DLL в `Sandbox/Assets/Plugins/Repkarbinhake`.

Тесты:

```bash
dotnet test tests/Repkarbinhake.Core.Tests
```

## Песочница

Открыть `Sandbox` в Unity 6000.3.12, сначала собрать `src/Repkarbinhake.Unity` (чтобы появились DLL), затем меню **Repkarbinhake → Build Sandbox Scene**.

## Сборка для тестеров

```powershell
powershell -ExecutionPolicy Bypass -File tools/Pack-Release.ps1
```

Скрипт собирает два варианта мода и кладёт в `dist/` по два архива на каждый, с версией из `RepkaPlugin.Version`:

- **release** — конфигурация `Public` (символ `REPKA_PUBLIC`): без окна отладки и тестовой капсулы F9 (их ключей в конфиге нет); окно настроек есть, но клавиша `ConfigEditorKey` по умолчанию пустая — игрок назначает её сам. Для игроков. Эта сборка не копируется в игру;
- **debug** — конфигурация `Release`: со всей отладкой, та же, что ставится в игру при обычной сборке.

Для каждого варианта:

- `RopeCarabiner-<версия>-<вариант>-full.zip` — BepInEx 5.4.23.5 + мод, распаковать в папку игры (для тех, у кого модов нет);
- `RopeCarabiner-<версия>-<вариант>-plugin.zip` — только `BepInEx/plugins/RopeCarabiner/RopeCarabiner.dll` (если BepInEx уже стоит).

Ещё один архив — `RopeCarabiner-<версия>-thunderstore.zip` для загрузки на Thunderstore: в корне `manifest.json` (версия подставляется), `icon.png` 256×256, `README.md`, `CHANGELOG.md` из `tools/thunderstore/`, мод из релизной сборки — в `plugins/` (менеджер модов кладёт его в `BepInEx/plugins/<команда>-Rope_Carabiner/`; мод везде показывается как Rope Carabiner — имя плагина в BepInEx и логе, окно настроек, предупреждение о версиях, инструкция и имена архивов; GUID `repkarbinhake` (а с ним файл `BepInEx/config/repkarbinhake.cfg`), пространства имён и папки исходников оставлены старыми, чтобы у игроков не сбросились настройки; DLL мода — `RopeCarabiner.dll` в папке `RopeCarabiner`); `website_url` и раздел Links описания ведут на этот репозиторий, картинки страницы лежат в этом же репозитории в `tools/thunderstore/images/` (ссылки raw.githubusercontent на ветку `main`, поэтому картинки должны быть запушены до загрузки пакета). Зависимость — `BepInEx-BepInExPack_PEAK-5.4.75301`. Перед новой версией дополнить `tools/thunderstore/CHANGELOG.md`.

Вручную релизную сборку можно собрать `dotnet build src/Repkarbinhake.Plugin -c Public`.

Во все архивы входит `RopeCarabiner-INSTALL.txt` (установка, управление, что прислать при ошибке) из `tools/INSTALL.txt`. BepInEx берётся из `BepInEx/BepInEx_win_x64_5.4.23.5`, другой путь — параметр `-BepInExDirectory`. У всех игроков должна быть одна версия: мод пишет в лог версии остальных и предупреждает о расхождении.
