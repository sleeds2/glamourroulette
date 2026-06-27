using GlamourRoulette.Game;

namespace GlamourRoulette.Services;

internal sealed class PluginNotifier
{
    private readonly PluginServices services;

    public PluginNotifier(PluginServices services)
    {
        this.services = services;
    }

    public void Print(string message)
    {
        var chatMessage = ApplyGlamourPlateResult.ForChat(message);

        try
        {
            this.services.ChatGui.Print(chatMessage);
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to print chat message: {Message}", chatMessage);
        }
    }

    public void PrintResult(ApplyGlamourPlateResult result)
    {
        this.Print(result.Message);
    }

    public void PrintUnexpectedError(Exception ex, string action)
    {
        this.services.Log.Error(ex, "Unexpected Glamour Roulette error while {Action}", action);
        this.Print($"An unexpected error occurred while {action}. Check /xllog for details.");
    }
}
