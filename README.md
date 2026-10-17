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
