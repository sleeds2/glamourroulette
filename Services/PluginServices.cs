using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace GlamourRoulette.Services;

internal sealed class PluginServices
{
    public PluginServices(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        ICondition condition,
        IObjectTable objectTable,
        IChatGui chatGui,
        IPluginLog log)
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        this.ClientState = clientState;
        this.Condition = condition;
        this.ObjectTable = objectTable;
        this.ChatGui = chatGui;
        this.Log = log;
    }

    internal IDalamudPluginInterface PluginInterface { get; }

    internal ICommandManager CommandManager { get; }

    internal IClientState ClientState { get; }

    internal ICondition Condition { get; }

    internal IObjectTable ObjectTable { get; }

    internal IChatGui ChatGui { get; }

    internal IPluginLog Log { get; }
}
