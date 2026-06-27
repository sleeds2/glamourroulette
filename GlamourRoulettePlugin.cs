using Dalamud.Plugin;
using GlamourRoulette.Commands;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using GlamourRoulette.UI;

namespace GlamourRoulette;

public sealed class GlamourRoulettePlugin : IDalamudPlugin
{
    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;
    private readonly PluginUi ui;
    private readonly CommandHandler commandHandler;

    public GlamourRoulettePlugin(IDalamudPluginInterface pluginInterface)
    {
        this.services = pluginInterface.Create<PluginServices>();

        this.configuration = this.services.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        this.configuration.Initialize(this.services.PluginInterface);

        this.glamourPlateService = new GlamourPlateService(this.services, this.configuration);
        this.ui = new PluginUi(this.configuration, this.glamourPlateService);
        this.commandHandler = new CommandHandler(this.services, this.configuration, this.glamourPlateService, this.ui);

        this.services.PluginInterface.UiBuilder.Draw += this.ui.Draw;
        this.services.PluginInterface.UiBuilder.OpenMainUi += this.ui.ToggleMainWindow;
    }

    public void Dispose()
    {
        this.services.PluginInterface.UiBuilder.OpenMainUi -= this.ui.ToggleMainWindow;
        this.services.PluginInterface.UiBuilder.Draw -= this.ui.Draw;

        this.commandHandler.Dispose();
        this.ui.Dispose();
    }
}
