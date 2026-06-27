using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace GlamourRoulette.Services;

internal sealed class PluginServices
{
    [PluginService]
    internal IDalamudPluginInterface PluginInterface { get; private init; } = null!;

    [PluginService]
    internal ICommandManager CommandManager { get; private init; } = null!;

    [PluginService]
    internal IClientState ClientState { get; private init; } = null!;

    [PluginService]
    internal IObjectTable ObjectTable { get; private init; } = null!;

    [PluginService]
    internal IDataManager DataManager { get; private init; } = null!;

    [PluginService]
    internal IChatGui ChatGui { get; private init; } = null!;

    [PluginService]
    internal IFramework Framework { get; private init; } = null!;

    [PluginService]
    internal IGameGui GameGui { get; private init; } = null!;

    [PluginService]
    internal ISigScanner SigScanner { get; private init; } = null!;

    [PluginService]
    internal IGameInteropProvider GameInteropProvider { get; private init; } = null!;

    [PluginService]
    internal IPluginLog Log { get; private init; } = null!;
}
