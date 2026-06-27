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
    private const string UsageMessage = "Usage: /glamourroulette [config|settings|help]. Aliases: /glamroulette, /gr.";

    private readonly PluginServices services;
    private readonly GlamourPlateService glamourPlateService;
    private readonly ConfigWindow configWindow;
    private readonly PluginNotifier notifier;

    public CommandHandler(
        PluginServices services,
        GlamourPlateService glamourPlateService,
        ConfigWindow configWindow,
        PluginNotifier notifier)
    {
        this.services = services;
        this.glamourPlateService = glamourPlateService;
        this.configWindow = configWindow;
        this.notifier = notifier;

        var commandInfo = new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Choose a random enabled glamour plate. Subcommands: config/settings, help.",
            ShowInHelp = true,
        };

        this.services.CommandManager.AddHandler(MainCommand, commandInfo);
        this.services.CommandManager.AddHandler(AliasCommand, commandInfo);
        this.services.CommandManager.AddHandler(ShortCommand, commandInfo);
    }

    public void Dispose()
    {
        this.services.CommandManager.RemoveHandler(MainCommand);
        this.services.CommandManager.RemoveHandler(AliasCommand);
        this.services.CommandManager.RemoveHandler(ShortCommand);
    }

    private void OnCommand(string command, string arguments)
    {
        try
        {
            var trimmedArguments = arguments.Trim();
            if (trimmedArguments.Length == 0)
            {
                var result = this.glamourPlateService.ApplyRandomPlate();
                this.notifier.PrintResult(result);

                return;
            }

            if (trimmedArguments.Equals("config", StringComparison.OrdinalIgnoreCase)
                || trimmedArguments.Equals("settings", StringComparison.OrdinalIgnoreCase))
            {
                this.configWindow.Toggle();
                return;
            }

            if (trimmedArguments.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                this.notifier.Print(UsageMessage);
                return;
            }

            this.notifier.Print($"Unknown Glamour Roulette command '{trimmedArguments}'. {UsageMessage}");
        }
        catch (Exception ex)
        {
            this.notifier.PrintUnexpectedError(ex, "handling a command");
        }
    }
}
