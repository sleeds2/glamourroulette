using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using GlamourRoulette.Commands;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using GlamourRoulette.Windows;

namespace GlamourRoulette;

public sealed class GlamourRoulettePlugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly PluginNotifier notifier;
    private readonly GlamourPlateService glamourPlateService;
    private readonly WindowSystem windowSystem = new("GlamourRoulette");
    private readonly ConfigWindow configWindow;
    private readonly CommandHandler commandHandler;

    public GlamourRoulettePlugin()
    {
        this.services = new PluginServices(
            PluginInterface,
            CommandManager,
            ClientState,
            Condition,
            ObjectTable,
            DataManager,
            ChatGui,
            Framework,
            GameGui,
            SigScanner,
            GameInteropProvider,
            Log);

        this.configuration = this.services.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        this.configuration.Initialize(this.services.PluginInterface);
        this.notifier = new PluginNotifier(this.services);

        this.glamourPlateService = new GlamourPlateService(this.services, this.configuration);
        this.configWindow = new ConfigWindow(this.configuration, this.glamourPlateService, this.notifier);
        this.windowSystem.AddWindow(this.configWindow);
        this.commandHandler = new CommandHandler(this.services, this.glamourPlateService, this.configWindow, this.notifier);

        this.services.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.services.PluginInterface.UiBuilder.OpenConfigUi += this.configWindow.Toggle;
    }

    public void Dispose()
    {
        this.services.PluginInterface.UiBuilder.OpenConfigUi -= this.configWindow.Toggle;
        this.services.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;

        this.commandHandler.Dispose();
        this.windowSystem.RemoveAllWindows();
    }
}
