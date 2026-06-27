using Dalamud.Game.Command;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using GlamourRoulette.Windows;

namespace GlamourRoulette.Commands;

internal sealed class CommandHandler : IDisposable
{
    private const string MainCommand = "/glamourroulette";
    private const string AliasCommand = "/glamroulette";
    private const string ShortCommand = "/gr";

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;
    private readonly ConfigWindow configWindow;

    public CommandHandler(
        PluginServices services,
        PluginConfiguration configuration,
        GlamourPlateService glamourPlateService,
        ConfigWindow configWindow)
    {
        this.services = services;
        this.configuration = configuration;
        this.glamourPlateService = glamourPlateService;
        this.configWindow = configWindow;

        var commandInfo = new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Choose a random enabled glamour plate, or use 'config' or 'settings' to open Glamour Roulette settings.",
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
        var trimmedArguments = arguments.Trim();
        if (trimmedArguments.Length == 0 || trimmedArguments.Equals("roll", StringComparison.OrdinalIgnoreCase))
        {
            var result = this.glamourPlateService.ApplyRandomPlate();
            if (this.configuration.EnableChatMessages)
            {
                this.services.ChatGui.Print(result.Message);
            }

            return;
        }

        if (trimmedArguments.Equals("config", StringComparison.OrdinalIgnoreCase)
            || trimmedArguments.Equals("settings", StringComparison.OrdinalIgnoreCase))
        {
            this.configWindow.IsOpen = true;
        }
    }
}
