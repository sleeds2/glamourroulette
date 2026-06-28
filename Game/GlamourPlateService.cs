using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using GlamourRoulette.Configuration;
using GlamourRoulette.Services;

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
            .Select(plate => new GlamourPlateInfo(plate, $"Plate {plate}"))
            .ToList();
    }

    public IReadOnlyList<GlamourPlateInfo> GetEligiblePlates()
    {
        return this.GetAllPlates()
            .Where(plate => this.configuration.IsPlateEligible(plate.Number))
            .ToList();
    }

    public GlamourPlateInfo? SelectRandomPlate()
    {
        var plates = this.GetEligiblePlates();
        return plates.Count == 0 ? null : plates[this.random.Next(plates.Count)];
    }

    public GlamourPlateInfo? PickRandomPlate()
    {
        return this.SelectRandomPlate();
    }

    public ApplyGlamourPlateResult ApplyRandomPlate()
    {
        if (!this.services.ClientState.IsLoggedIn || this.services.ObjectTable.LocalPlayer is null)
        {
            return ApplyGlamourPlateResult.Failed("A character must be logged in before choosing a glamour plate.");
        }

        var plate = this.SelectRandomPlate();
        if (plate is null)
        {
            return ApplyGlamourPlateResult.Failed(NoEligiblePlatesMessage);
        }

        return this.glamourPlateApplier.Apply(plate);
    }

    public ApplyGlamourPlateResult OpenGlamourPlateUi()
    {
        if (!this.services.ClientState.IsLoggedIn || this.services.ObjectTable.LocalPlayer is null)
        {
            return ApplyGlamourPlateResult.Failed("A character must be logged in before opening the Glamour Plate UI.");
        }

        if (!this.IsInSanctuary())
        {
            return ApplyGlamourPlateResult.Failed("Glamour plates can only be applied in sanctuaries, cities, residential areas, and inns.");
        }

        if (this.services.Condition[ConditionFlag.Mounted])
        {
            return ApplyGlamourPlateResult.Failed("Glamour plates cannot be applied while mounted.");
        }

        try
        {
            unsafe
            {
                var agent = AgentMiragePrismMiragePlate.Instance();
                if (agent is null)
                {
                    return ApplyGlamourPlateResult.Failed("The Glamour Plate UI is unavailable; please try again after changing areas.");
                }

                agent->Show();
                return ApplyGlamourPlateResult.Succeeded(null, "Opened the Glamour Plate UI.");
            }
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to open the Glamour Plate UI");
            return ApplyGlamourPlateResult.Failed("Failed to open the Glamour Plate UI: game API call failed.");
        }
    }

    private bool IsInSanctuary()
    {
        unsafe
        {
            var territoryInfo = TerritoryInfo.Instance();
            return territoryInfo is not null && territoryInfo->InSanctuary;
        }
    }
}

internal sealed record GlamourPlateInfo(int Number, string Name);

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
