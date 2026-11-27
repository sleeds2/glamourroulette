using FFXIVClientStructs.FFXIV.Client.Game;
using GlamourRoulette.Configuration;
using GlamourRoulette.Services;
using Lumina.Excel.Sheets;

namespace GlamourRoulette.Game;

internal sealed class GlamourPlateService
{
    private const string NoEligiblePlatesMessage = "No glamour plates are enabled in the Glamour Roulette configuration.";

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateApplier glamourPlateApplier;
    private readonly Random random = new();

    public GlamourPlateService(PluginServices services, PluginConfiguration configuration)
    {
        this.services = services;
        this.configuration = configuration;
        this.glamourPlateApplier = new GlamourPlateApplier(services);
    }

    public IReadOnlyList<GlamourPlateInfo> GetAllPlates()
    {
        return Enumerable.Range(1, PluginConfiguration.MaxGlamourPlateCount)
            .Select(plate => new GlamourPlateInfo(plate))
            .ToList();
    }

    public ApplyGlamourPlateResult ApplyRandomPlate()
    {
        if (!this.services.ClientState.IsLoggedIn || this.services.ObjectTable.LocalPlayer is not { } player)
        {
            return ApplyGlamourPlateResult.Failed("A character must be logged in before choosing a glamour plate.");
        }

        var plates = this.GetAllPlates()
            .Where(plate => this.configuration.IsPlateEligible(plate.Number))
            .ToList();
        if (plates.Count == 0)
        {
            return ApplyGlamourPlateResult.Failed(NoEligiblePlatesMessage);
        }

        if (this.configuration.AdvancedMode && plates.Any(plate => this.configuration.GetPlateRoles(plate.Number) != PlateRole.None))
        {
            var role = GetPlateRole(player.ClassJob.ValueNullable);
            if (role == PlateRole.None)
            {
                return ApplyGlamourPlateResult.Failed("Your current class or job has no role for Advanced Mode.");
            }

            plates.RemoveAll(plate => (this.configuration.GetPlateRoles(plate.Number) & role) == 0);
            if (plates.Count == 0)
            {
                return ApplyGlamourPlateResult.Failed($"No enabled glamour plates are tagged for your current role ({role}).");
            }
        }

        return this.glamourPlateApplier.Apply(plates[this.random.Next(plates.Count)]);
    }

    private static PlateRole GetPlateRole(ClassJob? classJob)
    {
        const int intelligence = 4;

        return classJob switch
        {
            { Role: 1 } => PlateRole.Tank,
            { Role: 2 } => PlateRole.Melee,
            { Role: 3, PrimaryStat: intelligence } => PlateRole.Magic,
            { Role: 3 } => PlateRole.Ranged,
            { Role: 4 } => PlateRole.Healer,
            _ => PlateRole.None,
        };
    }

    public ApplyGlamourPlateResult OpenGlamourPlateUi()
    {
        if (!this.services.ClientState.IsLoggedIn || this.services.ObjectTable.LocalPlayer is null)
        {
            return ApplyGlamourPlateResult.Failed("A character must be logged in before opening the Glamour Plate UI.");
        }

        try
        {
            unsafe
            {
                var actionManager = ActionManager.Instance();
                if (actionManager is null)
                {
                    return ApplyGlamourPlateResult.Failed("The Glamour Plate UI is unavailable; please try again after changing areas.");
                }

                if (!actionManager->UseAction(ActionType.GeneralAction, 0x19))
                {
                    return ApplyGlamourPlateResult.Failed("The Glamour Plate UI cannot be opened right now.");
                }

                return ApplyGlamourPlateResult.Succeeded(null, "Opened the Glamour Plate UI.");
            }
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to open the Glamour Plate UI");
            return ApplyGlamourPlateResult.Failed("Failed to open the Glamour Plate UI: game API call failed.");
        }
    }
}

internal sealed record GlamourPlateInfo(int Number);

internal sealed record ApplyGlamourPlateResult(bool Success, GlamourPlateInfo? Plate, string Message)
{
    internal const string ChatPrefix = "[Glamour Roulette]";

    public static ApplyGlamourPlateResult Succeeded(GlamourPlateInfo? plate, string message) => new(true, plate, ForChat(message));

    public static ApplyGlamourPlateResult Failed(string message) => new(false, null, ForChat(message));

    private static string ForChat(string message)
    {
        return message.StartsWith(ChatPrefix, StringComparison.Ordinal)
            ? message
            : $"{ChatPrefix} {message}";
    }
}
