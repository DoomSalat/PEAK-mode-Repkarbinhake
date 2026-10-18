# Repkarbinhake

Мод для PEAK (BepInEx 5): игроки связываются физичной верёвкой, сматывают её, страхуют и вытягивают друг друга.

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

Сборка плагина сама кладёт DLL и `Assets` в `BepInEx/plugins/Repkarbinhake` игры, а сборка `Repkarbinhake.Unity` — DLL в `Sandbox/Assets/Plugins/Repkarbinhake`.

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

- `Repkarbinhake-<версия>-<вариант>-full.zip` — BepInEx 5.4.23.5 + мод, распаковать в папку игры (для тех, у кого модов нет);
- `Repkarbinhake-<версия>-<вариант>-plugin.zip` — только `BepInEx/plugins/Repkarbinhake` (если BepInEx уже стоит).

Ещё один архив — `Repkarbinhake-<версия>-thunderstore.zip` для загрузки на Thunderstore: в корне `manifest.json` (версия подставляется), `icon.png` 256×256, `README.md`, `CHANGELOG.md` из `tools/thunderstore/`, мод из релизной сборки — в `plugins/` (менеджер модов кладёт его в `BepInEx/plugins/<команда>-Rope_Carabiner/`; на Thunderstore пакет называется Rope Carabiner, внутреннее имя мода и файла настроек осталось repkarbinhake). Зависимость — `BepInEx-BepInExPack_PEAK-5.4.75301`. Перед новой версией дополнить `tools/thunderstore/CHANGELOG.md`.

Вручную релизную сборку можно собрать `dotnet build src/Repkarbinhake.Plugin -c Public`.

Во все архивы входит `Repkarbinhake-INSTALL.txt` (установка, управление, что прислать при ошибке) из `tools/INSTALL.txt`. BepInEx берётся из `BepInEx/BepInEx_win_x64_5.4.23.5`, другой путь — параметр `-BepInExDirectory`. У всех игроков должна быть одна версия: мод пишет в лог версии остальных и предупреждает о расхождении.
