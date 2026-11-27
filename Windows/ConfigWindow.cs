using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using System.Numerics;

namespace GlamourRoulette.Windows;

internal sealed class ConfigWindow : Window
{
    private static readonly PlateRole[] Roles = [PlateRole.Tank, PlateRole.Healer, PlateRole.Melee, PlateRole.Ranged, PlateRole.Magic];

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;

    public ConfigWindow(PluginServices services, PluginConfiguration configuration, GlamourPlateService glamourPlateService)
        : base("Glamour Roulette Settings##ConfigWindow")
    {
        this.services = services;
        this.configuration = configuration;
        this.glamourPlateService = glamourPlateService;
        this.Size = new Vector2(440, 560);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        if (ImGui.Button("Roll random glamour plate"))
        {
            var result = this.glamourPlateService.ApplyRandomPlate();
            this.services.ChatGui.Print(result.Message);
        }

        ImGui.SameLine();
        if (ImGui.Button("Open Glamour Plate UI"))
        {
            var result = this.glamourPlateService.OpenGlamourPlateUi();
            if (!result.Success)
            {
                this.services.ChatGui.Print(result.Message);
            }
        }

        ImGui.Separator();
        ImGui.TextWrapped("Enable the glamour plate slots that should be included when Glamour Roulette randomly selects a plate.");
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.2f, 0.2f, 1.0f));
        ImGui.TextWrapped("Please disable any empty glamour plates.");
        ImGui.PopStyleColor();

        var advancedMode = this.configuration.AdvancedMode;
        if (ImGui.Checkbox("Advanced Mode", ref advancedMode))
        {
            this.configuration.AdvancedMode = advancedMode;
            this.configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Tag plates by job role. Advanced Mode rolls only enabled plates tagged for your current job's role.\nIf no enabled plate matches, the roll is skipped. With Advanced Mode off, roulette uses all enabled plates.");
        }

        var columnCount = advancedMode ? 3 + Roles.Length : 2;
        var tableSizing = advancedMode
            ? ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY
            : ImGuiTableFlags.SizingStretchProp;
        var tableId = advancedMode ? "##AdvancedGlamourPlateSettingsTable" : "##GlamourPlateSettingsTable";
        if (ImGui.BeginTable(tableId, columnCount, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | tableSizing))
        {
            if (advancedMode)
            {
                ImGui.TableSetupScrollFreeze(0, 2);
                ImGui.TableSetupColumn("Plate");
                ImGui.TableSetupColumn("Enabled");
                ImGui.TableSetupColumn("All Roles");
                foreach (var role in Roles)
                {
                    ImGui.TableSetupColumn(role.ToString());
                }
            }
            else
            {
                ImGui.TableSetupColumn("Plate", ImGuiTableColumnFlags.WidthStretch, 2.0f);
                ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 85.0f);
            }

            ImGui.TableHeadersRow();

            var plates = this.glamourPlateService.GetAllPlates();
            if (advancedMode)
            {
                this.DrawRoleColumnToggles(plates);
            }

            foreach (var plate in plates)
            {
                var enabled = this.configuration.IsPlateEligible(plate.Number);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"Plate {plate.Number}");

                ImGui.TableNextColumn();
                ImGui.PushID(plate.Number);
                if (ImGui.Checkbox("##PlateEnabled", ref enabled))
                {
                    this.configuration.SetPlateEligibility(plate.Number, enabled);
                }

                if (advancedMode)
                {
                    var roles = this.configuration.GetPlateRoles(plate.Number);
                    ImGui.TableNextColumn();
                    var allChecked = roles == PlateRole.All;
                    if (DrawBulkRoleCheckbox("##AllRoles", $"all role tags for plate {plate.Number}", roles != PlateRole.None, ref allChecked))
                    {
                        roles = allChecked ? PlateRole.All : PlateRole.None;
                        this.configuration.SetPlateRoles(plate.Number, roles);
                    }

                    foreach (var role in Roles)
                    {
                        ImGui.TableNextColumn();
                        var tagged = (roles & role) != 0;
                        if (ImGui.Checkbox($"##{role}", ref tagged))
                        {
                            roles = tagged ? roles | role : roles & ~role;
                            this.configuration.SetPlateRoles(plate.Number, roles);
                        }
                    }
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }

    private void DrawRoleColumnToggles(IReadOnlyList<GlamourPlateInfo> plates)
    {
        var rolesCheckedOnAllPlates = PlateRole.All;
        var rolesCheckedOnAnyPlate = PlateRole.None;
        foreach (var plate in plates)
        {
            var roles = this.configuration.GetPlateRoles(plate.Number);
            rolesCheckedOnAllPlates &= roles;
            rolesCheckedOnAnyPlate |= roles;
        }

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("All Plates");

        ImGui.PushID("AllPlates");
        for (var index = 0; index < Roles.Length; index++)
        {
            var role = Roles[index];
            ImGui.TableSetColumnIndex(3 + index);
            var allChecked = (rolesCheckedOnAllPlates & role) != 0;
            var anyChecked = (rolesCheckedOnAnyPlate & role) != 0;
            if (DrawBulkRoleCheckbox($"##{role}", $"{role} tags for all plates", anyChecked, ref allChecked))
            {
                this.configuration.SetRoleForAllPlates(role, allChecked);
            }
        }

        ImGui.PopID();
    }

    private static bool DrawBulkRoleCheckbox(string id, string description, bool anyChecked, ref bool allChecked)
    {
        ImGuiP.PushItemFlag(ImGuiItemFlags.MixedValue, anyChecked && !allChecked);
        var changed = ImGui.Checkbox(id, ref allChecked);
        ImGuiP.PopItemFlag();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"{(allChecked ? "Clear" : "Check")} {description}.");
        }

        return changed;
    }
}
