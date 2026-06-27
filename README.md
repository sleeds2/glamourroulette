# Glamour Roulette

Glamour Roulette is a Dalamud plugin scaffold for Final Fantasy XIV. It provides commands and a small ImGui configuration window for selecting a pool of glamour plates and selecting one at random. The actual in-game apply call is currently a placeholder.

## Project layout

- `GlamourRoulette.csproj` - Dalamud SDK project using `Dalamud.NET.Sdk/15.0.0`, matching the compact project style used by current Dalamud plugin repos such as Job Roulette.
- `GlamourRoulette.json` - plugin manifest metadata consumed by DalamudPackager.
- `GlamourRoulettePlugin.cs` - Dalamud entry point that initializes configuration, commands, UI, and game services.
- `Services/PluginServices.cs` - Dalamud service injection for command handling, UI drawing, game/client state, chat, data, and interop APIs.
- `Commands/CommandHandler.cs` - `/glamourroulette` and `/gr` command registration.
- `Configuration/PluginConfiguration.cs` - persisted settings for enabled plates and chat output.
- `UI/PluginUi.cs` - ImGui configuration window for chat output and enabled plate selection.
- `Game/GlamourPlateService.cs` - glamour plate selection service with placeholder application behavior.

## Prerequisites

1. Install XIVLauncher and run Final Fantasy XIV with Dalamud at least once.
2. Install the .NET SDK version expected by your installed Dalamud toolchain.
3. Ensure your Dalamud dev environment can resolve `Dalamud.NET.Sdk/15.0.0` and the dev assemblies supplied by XIVLauncher/Dalamud.

## Build

```bash
dotnet restore

dotnet build -c Debug
```

The debug plugin DLL is written under the project `bin` output folder. Release builds use the Dalamud SDK packaging targets:

```bash
dotnet build -c Release
```

## Load in Dalamud

1. Launch the game through XIVLauncher.
2. Open Dalamud settings with `/xlsettings`.
3. Go to **Experimental** and add the full path to the built `GlamourRoulette.dll` as a dev plugin location.
4. Open `/xlplugins`, switch to dev plugins, and enable **Glamour Roulette**.

## Usage

- `/glamourroulette` or `/gr` - open/close the configuration window.
- `/gr roll` - choose a random enabled glamour plate and report the selected plate. The in-game apply call is intentionally left as a placeholder for now.

Use the configuration window to choose which glamour plate numbers are eligible for roulette. Chat output can be enabled or disabled from the same window.

## Future FFXIVClientStructs integration

Glamour application is currently a placeholder by design. When this is implemented, the most relevant FFXIVClientStructs type to evaluate is `FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule`, especially its gearset inspection members and `EquipGearset(gearsetId, glamourPlateId)` method.
