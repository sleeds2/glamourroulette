using GlamourRoulette.Configuration;
using GlamourRoulette.Services;

namespace GlamourRoulette.Game;

internal sealed class GlamourPlateService
{
    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly Random random = new();

    public GlamourPlateService(PluginServices services, PluginConfiguration configuration)
    {
        this.services = services;
        this.configuration = configuration;
    }

    public IReadOnlyList<GlamourPlateInfo> GetConfiguredPlates()
    {
        return this.configuration.GetEligiblePlateNumbers()
            .Select(static plate => new GlamourPlateInfo(plate, $"Plate {plate}"))
            .ToList();
    }

    public GlamourPlateInfo PickRandomPlate()
    {
        var plates = this.GetConfiguredPlates();
        return plates.Count == 0 ? null : plates[this.random.Next(plates.Count)];
    }

    public ApplyGlamourPlateResult ApplyRandomPlate()
    {
        if (!this.services.ClientState.IsLoggedIn || this.services.ObjectTable.LocalPlayer is null)
        {
            return ApplyGlamourPlateResult.Failed("A character must be logged in before choosing a glamour plate.");
        }

        var plate = this.PickRandomPlate();
        if (plate is null)
        {
            return ApplyGlamourPlateResult.Failed("No glamour plates are enabled in the Glamour Roulette configuration.");
        }

        // Placeholder by request: selecting the plate is wired, but the actual client call that applies
        // the plate is intentionally left for a later change after the desired API surface is finalized.
        this.services.Log.Information("glamour_plate_selected plateNumber={PlateNumber}", plate.Number);
        return ApplyGlamourPlateResult.Succeeded(plate, $"Selected glamour plate {plate.Number}. Application is not implemented yet.");
    }
}

internal sealed record GlamourPlateInfo(int Number, string Name);

internal sealed record ApplyGlamourPlateResult(bool Success, GlamourPlateInfo Plate, string Message)
{
    public static ApplyGlamourPlateResult Succeeded(GlamourPlateInfo plate, string message) => new(true, plate, message);

    public static ApplyGlamourPlateResult Failed(string message) => new(false, null, message);
}
