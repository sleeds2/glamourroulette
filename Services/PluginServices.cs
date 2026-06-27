using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace GlamourRoulette.Services;

public sealed class PluginServices
{
    internal void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.PluginInterface = pluginInterface;
    }

    public IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    public ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    public IClientState ClientState { get; private set; } = null!;

    [PluginService]
    public ICondition Condition { get; private set; } = null!;

    [PluginService]
    public IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    public IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    public IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    public IFramework Framework { get; private set; } = null!;

    [PluginService]
    public IGameGui GameGui { get; private set; } = null!;

    [PluginService]
    public ISigScanner SigScanner { get; private set; } = null!;

    [PluginService]
    public IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    [PluginService]
    public IPluginLog Log { get; private set; } = null!;
}
