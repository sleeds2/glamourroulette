using Dalamud.Configuration;
using Dalamud.Plugin;

namespace GlamourRoulette.Configuration;

[Flags]
public enum PlateRole
{
    None = 0,
    Tank = 1 << 0,
    Healer = 1 << 1,
    Melee = 1 << 2,
    Ranged = 1 << 3,
    Magic = 1 << 4,
    All = Tank | Healer | Melee | Ranged | Magic,
}

public sealed class PluginConfiguration : IPluginConfiguration
{
    public const int MaxGlamourPlateCount = 20;

    public int Version { get; set; } = 1;

    /// <summary>
    /// Explicit per-plate eligibility settings keyed by glamour plate number.
    /// Missing plate numbers use the default behavior: enabled.
    /// </summary>
    public Dictionary<int, bool> PlateEligibility { get; set; } = new();

    public bool AdvancedMode { get; set; }

    // Missing entries use all tags. A plate with no tags is saved explicitly.
    public Dictionary<int, PlateRole> PlateRoles { get; set; } = new();

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

    public PlateRole GetPlateRoles(int plateNumber)
    {
        return this.PlateRoles.GetValueOrDefault(plateNumber, PlateRole.All);
    }

    public void SetPlateRoles(int plateNumber, PlateRole roles)
    {
        if (!IsValidPlateNumber(plateNumber) || this.GetPlateRoles(plateNumber) == roles)
        {
            return;
        }

        if (roles == PlateRole.All)
        {
            this.PlateRoles.Remove(plateNumber);
        }
        else
        {
            this.PlateRoles[plateNumber] = roles;
        }

        this.Save();
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
