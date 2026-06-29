using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using GlamourRoulette.Services;

namespace GlamourRoulette.Game;

internal static class GlamourPlateConditionValidator
{
    internal static bool IsBlockedByCondition(PluginServices services, out string failureMessage)
    {
        if (!IsInSanctuary())
        {
            failureMessage = "Glamour plates can only be applied in sanctuaries, cities, residential areas, and inns.";
            return true;
        }

        if (services.Condition[ConditionFlag.Mounted])
        {
            failureMessage = "Glamour plates cannot be applied while mounted.";
            return true;
        }

        if (services.Condition[ConditionFlag.Occupied]
            || services.Condition[ConditionFlag.OccupiedInQuestEvent]
            || services.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || services.Condition[ConditionFlag.WatchingCutscene])
        {
            failureMessage = "Glamour plates cannot be applied while the character is occupied.";
            return true;
        }

        failureMessage = string.Empty;
        return false;
    }

    private static bool IsInSanctuary()
    {
        unsafe
        {
            var territoryInfo = TerritoryInfo.Instance();
            return territoryInfo is not null && territoryInfo->InSanctuary;
        }
    }
}
