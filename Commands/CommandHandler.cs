using Dalamud.Game.Command;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using GlamourRoulette.UI;

namespace GlamourRoulette.Commands;

internal sealed class CommandHandler : IDisposable
{
    private const string MainCommand = "/glamourroulette";
    private const string ShortCommand = "/gr";

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;
    private readonly PluginUi ui;

    public CommandHandler(
        PluginServices services,
        PluginConfiguration configuration,
        GlamourPlateService glamourPlateService,
        PluginUi ui)
    {
        this.services = services;
        this.configuration = configuration;
        this.glamourPlateService = glamourPlateService;
        this.ui = ui;

        var commandInfo = new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Open Glamour Roulette, or use /gr roll to choose a configured glamour plate.",
            ShowInHelp = true,
        };

        this.services.CommandManager.AddHandler(MainCommand, commandInfo);
        this.services.CommandManager.AddHandler(ShortCommand, commandInfo);
    }

    public void Dispose()
    {
        this.services.CommandManager.RemoveHandler(MainCommand);
        this.services.CommandManager.RemoveHandler(ShortCommand);
    }

    private void OnCommand(string command, string arguments)
    {
        if (arguments.Trim().Equals("roll", StringComparison.OrdinalIgnoreCase))
        {
            var result = this.glamourPlateService.ApplyRandomPlate();
            if (this.configuration.EnableChatMessages)
            {
                this.services.ChatGui.Print(result.Message);
            }

            return;
        }

        this.ui.ToggleMainWindow();
    }
}
