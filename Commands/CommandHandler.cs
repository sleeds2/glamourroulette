using Dalamud.Game.Command;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using GlamourRoulette.Windows;

namespace GlamourRoulette.Commands;

internal sealed class CommandHandler : IDisposable
{
    private const string MainCommand = "/glamourroulette";
    private const string AliasCommand = "/glamroulette";
    private const string ShortCommand = "/gr";
    private const string UsageMessage = "Usage: /glamourroulette [config|settings]. Aliases: /glamroulette, /gr.";

    private readonly PluginServices services;
    private readonly GlamourPlateService glamourPlateService;
    private readonly ConfigWindow configWindow;

    public CommandHandler(
        PluginServices services,
        GlamourPlateService glamourPlateService,
        ConfigWindow configWindow)
    {
        this.services = services;
        this.glamourPlateService = glamourPlateService;
        this.configWindow = configWindow;

        var mainCommandInfo = new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Choose a random enabled glamour plate.\n"
                        +"   /glamroulette, /gr: Alias for /glamourroulette\n" 
                        +"   /glamourroulette config|settings - Toggle the settings window.",
            ShowInHelp = true,
        };
        
        var aliasCommandInfo = new CommandInfo(this.OnCommand)
        {
            ShowInHelp = false,
        };

        this.services.CommandManager.AddHandler(MainCommand, mainCommandInfo);
        this.services.CommandManager.AddHandler(AliasCommand, aliasCommandInfo);
        this.services.CommandManager.AddHandler(ShortCommand, aliasCommandInfo);
    }

    public void Dispose()
    {
        this.services.CommandManager.RemoveHandler(MainCommand);
        this.services.CommandManager.RemoveHandler(AliasCommand);
        this.services.CommandManager.RemoveHandler(ShortCommand);
    }

    private void OnCommand(string command, string arguments)
    {
        var trimmedArguments = arguments.Trim();
        if (trimmedArguments.Length == 0)
        {
            var result = this.glamourPlateService.ApplyRandomPlate();
            this.services.ChatGui.Print(result.Message);

            return;
        }

        if (trimmedArguments.Equals("config", StringComparison.OrdinalIgnoreCase)
            || trimmedArguments.Equals("settings", StringComparison.OrdinalIgnoreCase))
        {
            this.configWindow.Toggle();
            return;
        }

        this.services.ChatGui.Print($"Unknown Glamour Roulette command '{trimmedArguments}'. {UsageMessage}");
    }
}
