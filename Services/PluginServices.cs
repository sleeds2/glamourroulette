using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace GlamourRoulette.Services;

internal sealed class PluginServices
{
    internal void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.PluginInterface = pluginInterface;
    }

    internal IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal ICondition Condition { get; private set; } = null!;

    [PluginService]
    internal IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    internal IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal IGameGui GameGui { get; private set; } = null!;

    [PluginService]
    internal ISigScanner SigScanner { get; private set; } = null!;

    [PluginService]
    internal IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    [PluginService]
    internal IPluginLog Log { get; private set; } = null!;
}
