using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using GlamourRoulette.Configuration;
using GlamourRoulette.Services;

namespace GlamourRoulette.Game;

internal sealed class GlamourPlateService
{
    private const string NoEligiblePlatesMessage = "No glamour plates are enabled in the Glamour Roulette configuration.";

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly Func<IReadOnlyList<GlamourPlateInfo>> savedPlateProvider;
    private readonly GlamourPlateApplier glamourPlateApplier;
    private readonly Random random = new();

    public GlamourPlateService(PluginServices services, PluginConfiguration configuration)
        : this(services, configuration, null)
    {
    }

    internal GlamourPlateService(
        PluginServices services,
        PluginConfiguration configuration,
        Func<IReadOnlyList<GlamourPlateInfo>>? savedPlateProvider)
    {
        this.services = services;
        this.configuration = configuration;
        this.savedPlateProvider = savedPlateProvider ?? this.EnumerateSavedGlamourPlates;
        this.glamourPlateApplier = new GlamourPlateApplier(services);
    }

    public IReadOnlyList<GlamourPlateInfo> GetAvailablePlates()
    {
        return this.GetSavedPlates();
    }

    public IReadOnlyList<GlamourPlateInfo> GetSavedPlates()
    {
        try
        {
            return this.savedPlateProvider();
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to enumerate saved glamour plates");
            return Array.Empty<GlamourPlateInfo>();
        }
    }

    public IReadOnlyList<GlamourPlateInfo> GetConfiguredPlates()
    {
        return this.GetEligiblePlates();
    }

    public IReadOnlyList<GlamourPlateInfo> GetEligiblePlates()
    {
        return this.GetSavedPlates()
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
        try
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
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Unexpected error while applying a random glamour plate");
            return ApplyGlamourPlateResult.Failed("An unexpected error occurred while applying a random glamour plate. Check /xllog for details.");
        }
    }

    public ApplyGlamourPlateResult OpenGlamourPlateUi()
    {
        if (!this.glamourPlateApplier.TryGetCurrentGearsetId(out var gearsetId, out var stateFailure))
        {
            return ApplyGlamourPlateResult.Failed(stateFailure);
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

                agent->OpenForGearset(gearsetId, 0);
                return ApplyGlamourPlateResult.Succeeded(null, "Opened the Glamour Plate UI.");
            }
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to open the Glamour Plate UI");
            return ApplyGlamourPlateResult.Failed("Failed to open the Glamour Plate UI: game API call failed.");
        }
    }

    private IReadOnlyList<GlamourPlateInfo> EnumerateSavedGlamourPlates()
    {
        return Enumerable.Range(1, PluginConfiguration.MaxGlamourPlateCount)
            .Select(plate => new GlamourPlateInfo(plate, $"Plate {plate}", this.IsGlamourPlateEmpty(plate)))
            .ToList();
    }

    private bool IsGlamourPlateEmpty(int plateNumber)
    {
        if (plateNumber is < 1 or > PluginConfiguration.MaxGlamourPlateCount)
        {
            return true;
        }

        try
        {
            unsafe
            {
                var agent = AgentMiragePrismMiragePlate.Instance();
                if (agent is null || agent->Data is null)
                {
                    return false;
                }

                var plateIndex = plateNumber - 1;
                var glamourPlates = agent->Data->GlamourPlates;
                if (plateIndex >= glamourPlates.Length)
                {
                    return true;
                }

                foreach (var item in glamourPlates[plateIndex].Items)
                {
                    if (item.ItemId != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            this.services.Log.Warning(ex, "Failed to read glamour plate {PlateNumber}; leaving it enabled", plateNumber);
            return false;
        }
    }
}

internal sealed record GlamourPlateInfo(int Number, string Name, bool IsEmpty = false);

internal sealed record ApplyGlamourPlateResult(bool Success, GlamourPlateInfo? Plate, string Message)
{
    internal const string ChatPrefix = "[Glamour Roulette]";

    public static ApplyGlamourPlateResult Succeeded(GlamourPlateInfo? plate, string message) => new(true, plate, ForChat(message));

    public static ApplyGlamourPlateResult Failed(string message) => new(false, null, ForChat(message));

    internal static string ForChat(string message)
    {
        return message.StartsWith(ChatPrefix, StringComparison.Ordinal)
            ? message
            : $"{ChatPrefix} {message}";
    }
}
