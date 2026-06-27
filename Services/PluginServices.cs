using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace GlamourRoulette.Services;

internal sealed class PluginServices
{
    private const string ChatPrefix = "[Glamour Roulette]";

    public PluginServices(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        ICondition condition,
        IObjectTable objectTable,
        IDataManager dataManager,
        IChatGui chatGui,
        IFramework framework,
        IGameGui gameGui,
        ISigScanner sigScanner,
        IGameInteropProvider gameInteropProvider,
        IPluginLog log)
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        this.ClientState = clientState;
        this.Condition = condition;
        this.ObjectTable = objectTable;
        this.DataManager = dataManager;
        this.ChatGui = chatGui;
        this.Framework = framework;
        this.GameGui = gameGui;
        this.SigScanner = sigScanner;
        this.GameInteropProvider = gameInteropProvider;
        this.Log = log;
    }

    internal IDalamudPluginInterface PluginInterface { get; }

    internal ICommandManager CommandManager { get; }

    internal IClientState ClientState { get; }

    internal ICondition Condition { get; }

    internal IObjectTable ObjectTable { get; }

    internal IDataManager DataManager { get; }

    internal IChatGui ChatGui { get; }

    internal IFramework Framework { get; }

    internal IGameGui GameGui { get; }

    internal ISigScanner SigScanner { get; }

    internal IGameInteropProvider GameInteropProvider { get; }

    internal IPluginLog Log { get; }

    internal void ReportErrorToChat(Exception exception, string action)
    {
        this.Log.Error(exception, "{Action} failed", action);

        var message = string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;

        this.ChatGui.Print($"{ChatPrefix} {action} failed: {message}");
    }
}
