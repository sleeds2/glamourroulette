using GlamourRoulette.Configuration;
using GlamourRoulette.Services;

namespace GlamourRoulette.Game;

internal sealed class GlamourPlateService
{
    private const string NoEligiblePlatesMessage = "No saved, non-empty glamour plates are enabled in the Glamour Roulette configuration.";

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
        return this.savedPlateProvider();
    }

    public IReadOnlyList<GlamourPlateInfo> GetConfiguredPlates()
    {
        return this.GetEligiblePlates();
    }

    public IReadOnlyList<GlamourPlateInfo> GetEligiblePlates()
    {
        return this.GetSavedPlates()
            .Where(plate => !plate.IsEmpty && this.configuration.IsPlateEligible(plate.Number, plate.IsEmpty))
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

    private IReadOnlyList<GlamourPlateInfo> EnumerateSavedGlamourPlates()
    {
        return Enumerable.Range(1, PluginConfiguration.MaxGlamourPlateCount)
            .Select(plate => new GlamourPlateInfo(plate, $"Plate {plate}", this.IsGlamourPlateEmpty(plate)))
            .ToList();
    }

    private bool IsGlamourPlateEmpty(int plateNumber)
    {
        // The default implementation keeps the plugin's current plate discovery behavior until the
        // native glamour plate storage reader is wired in.  The selection pipeline still treats this
        // value as authoritative, so injected or future real empty-plate data is always excluded even
        // when the user has enabled the plate in configuration.
        return false;
    }
}

internal sealed record GlamourPlateInfo(int Number, string Name, bool IsEmpty = false);

internal sealed record ApplyGlamourPlateResult(bool Success, GlamourPlateInfo? Plate, string Message)
{
    public static ApplyGlamourPlateResult Succeeded(GlamourPlateInfo plate, string message) => new(true, plate, message);

    public static ApplyGlamourPlateResult Failed(string message) => new(false, null, message);
}
