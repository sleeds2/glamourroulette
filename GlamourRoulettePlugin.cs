using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GlamourRoulette.Commands;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using GlamourRoulette.Windows;

namespace GlamourRoulette;

public sealed class GlamourRoulettePlugin : IDalamudPlugin
{
    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;
    private readonly WindowSystem windowSystem = new("GlamourRoulette");
    private readonly ConfigWindow configWindow;
    private readonly CommandHandler commandHandler;

    public GlamourRoulettePlugin(IDalamudPluginInterface pluginInterface)
    {
        this.services = pluginInterface.Create<PluginServices>()
            ?? throw new InvalidOperationException("Failed to create Glamour Roulette plugin services.");
        this.services.Initialize(pluginInterface);

        this.configuration = this.services.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        this.configuration.Initialize(this.services.PluginInterface);

        this.glamourPlateService = new GlamourPlateService(this.services, this.configuration);
        this.configWindow = new ConfigWindow(this.configuration, this.glamourPlateService);
        this.windowSystem.AddWindow(this.configWindow);
        this.commandHandler = new CommandHandler(this.services, this.configuration, this.glamourPlateService, this.configWindow);

        this.services.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.services.PluginInterface.UiBuilder.OpenMainUi += this.configWindow.Toggle;
    }

    public void Dispose()
    {
        this.services.PluginInterface.UiBuilder.OpenMainUi -= this.configWindow.Toggle;
        this.services.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;

        this.commandHandler.Dispose();
        this.windowSystem.RemoveAllWindows();
    }
}
