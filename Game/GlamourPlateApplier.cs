using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using GlamourRoulette.Configuration;
using GlamourRoulette.Services;

namespace GlamourRoulette.Game;

internal sealed class GlamourPlateApplier
{
    private readonly PluginServices services;

    public GlamourPlateApplier(PluginServices services)
    {
        this.services = services;
    }

    public ApplyGlamourPlateResult Apply(GlamourPlateInfo plate)
    {
        if (!this.IsPlateNumberAvailable(plate.Number))
        {
            return this.Fail($"Glamour plate {plate.Number} is unavailable.");
        }

        if (plate.IsEmpty)
        {
            return this.Fail($"Glamour plate {plate.Number} is empty and cannot be applied.");
        }

        if (!this.TryGetCurrentGearsetId(out var gearsetId, out var stateFailure))
        {
            return this.Fail(stateFailure);
        }

        var gearsetNumber = gearsetId + 1;
        try
        {
            var result = this.EquipGearsetWithPlate(gearsetId, plate.Number);
            if (result != 0)
            {
                this.services.Log.Warning(
                    "glamour_plate_apply_failed plateNumber={PlateNumber} gearsetId={GearsetId} gearsetNumber={GearsetNumber} result={Result}",
                    plate.Number,
                    gearsetId,
                    gearsetNumber,
                    result);

                return ApplyGlamourPlateResult.Failed(
                    $"Failed to apply glamour plate {plate.Number}: the game rejected the gearset change.");
            }

            this.services.Log.Information(
                "glamour_plate_applied plateNumber={PlateNumber} gearsetId={GearsetId} gearsetNumber={GearsetNumber}",
                plate.Number,
                gearsetId,
                gearsetNumber);

            return ApplyGlamourPlateResult.Succeeded(
                plate,
                $"Applied glamour plate {plate.Number} to gearset {gearsetNumber}.");
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to apply glamour plate {PlateNumber}", plate.Number);
            return ApplyGlamourPlateResult.Failed($"Failed to apply glamour plate {plate.Number}: game API call failed.");
        }
    }

    internal bool TryGetCurrentGearsetId(out int gearsetId, out string failureMessage)
    {
        gearsetId = -1;
        failureMessage = string.Empty;

        if (!this.services.ClientState.IsLoggedIn || this.services.ObjectTable.LocalPlayer is null)
        {
            failureMessage = "A character must be logged in before applying a glamour plate.";
            return false;
        }

        if (this.IsBlockedByCondition(out failureMessage))
        {
            return false;
        }

        try
        {
            unsafe
            {
                var gearsetModule = RaptureGearsetModule.Instance();
                if (gearsetModule is null)
                {
                    failureMessage = "The gearset module is unavailable; please try again after changing areas or opening the gearset list.";
                    return false;
                }

                gearsetId = gearsetModule->CurrentGearsetIndex;
                if (!gearsetModule->IsValidGearset(gearsetId))
                {
                    failureMessage = "No active gearset is available for applying a glamour plate.";
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            this.services.Log.Error(ex, "Failed to read the current gearset before applying a glamour plate");
            failureMessage = "Could not determine the current gearset for applying a glamour plate.";
            return false;
        }

        return true;
    }

    private int EquipGearsetWithPlate(int gearsetId, int plateNumber)
    {
        unsafe
        {
            var gearsetModule = RaptureGearsetModule.Instance();
            if (gearsetModule is null)
            {
                return -1;
            }

            return gearsetModule->EquipGearset(gearsetId, (byte)plateNumber);
        }
    }

    private bool IsBlockedByCondition(out string failureMessage)
    {
        if (!this.IsInSanctuary())
        {
            failureMessage = "Glamour plates can only be applied in sanctuaries, cities, residential areas, and inns.";
            return true;
        }

        if (this.services.Condition[ConditionFlag.Occupied]
            || this.services.Condition[ConditionFlag.OccupiedInQuestEvent]
            || this.services.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || this.services.Condition[ConditionFlag.WatchingCutscene])
        {
            failureMessage = "Glamour plates cannot be applied while the character is occupied.";
            return true;
        }

        failureMessage = string.Empty;
        return false;
    }

    private bool IsInSanctuary()
    {
        unsafe
        {
            var territoryInfo = TerritoryInfo.Instance();
            return territoryInfo is not null && territoryInfo->InSanctuary;
        }
    }

    private bool IsPlateNumberAvailable(int plateNumber)
    {
        return plateNumber is >= 1 and <= PluginConfiguration.MaxGlamourPlateCount;
    }

    private ApplyGlamourPlateResult Fail(string message)
    {
        this.services.Log.Warning("glamour_plate_apply_failed message={Message}", message);
        return ApplyGlamourPlateResult.Failed(message);
    }
}
