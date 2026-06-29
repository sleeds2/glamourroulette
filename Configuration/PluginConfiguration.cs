using Dalamud.Configuration;
using Dalamud.Plugin;

namespace GlamourRoulette.Configuration;

public sealed class PluginConfiguration : IPluginConfiguration
{
    public const int MaxGlamourPlateCount = 20;

    public int Version { get; set; } = 1;

    /// <summary>
    /// Explicit per-plate eligibility settings keyed by glamour plate number.
    /// Missing plate numbers use the default behavior: enabled.
    /// </summary>
    public Dictionary<int, bool> PlateEligibility { get; set; } = new();

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface dalamudPluginInterface)
    {
        this.pluginInterface = dalamudPluginInterface;
        this.RemoveInvalidPlateEligibilityEntries();
    }

    public bool IsPlateEligible(int plateNumber)
    {
        if (!IsValidPlateNumber(plateNumber))
        {
            return false;
        }

        return !this.PlateEligibility.TryGetValue(plateNumber, out var enabled) || enabled;
    }

    public void SetPlateEligibility(int plateNumber, bool enabled)
    {
        if (!IsValidPlateNumber(plateNumber))
        {
            return;
        }

        if (this.PlateEligibility.TryGetValue(plateNumber, out var current) && current == enabled)
        {
            return;
        }

        this.PlateEligibility[plateNumber] = enabled;
        this.Save();
    }

    public IEnumerable<int> GetEligiblePlateNumbers()
    {
        for (var plateNumber = 1; plateNumber <= MaxGlamourPlateCount; plateNumber++)
        {
            if (this.IsPlateEligible(plateNumber))
            {
                yield return plateNumber;
            }
        }
    }

    public void Save()
    {
        this.RemoveInvalidPlateEligibilityEntries();
        this.pluginInterface?.SavePluginConfig(this);
    }

    private static bool IsValidPlateNumber(int plateNumber)
    {
        return plateNumber is >= 1 and <= MaxGlamourPlateCount;
    }

    private void RemoveInvalidPlateEligibilityEntries()
    {
        foreach (var plateNumber in this.PlateEligibility.Keys.Where(static plateNumber => !IsValidPlateNumber(plateNumber)).ToList())
        {
            this.PlateEligibility.Remove(plateNumber);
        }
    }
}
