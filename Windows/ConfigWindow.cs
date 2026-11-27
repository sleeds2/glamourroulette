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
        this.Size = new Vector2(400, 560);
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

        var columnCount = advancedMode ? 2 + Roles.Length : 2;
        var tableSizing = advancedMode
            ? ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY
            : ImGuiTableFlags.SizingStretchProp;
        var tableId = advancedMode ? "##AdvancedGlamourPlateSettingsTable" : "##GlamourPlateSettingsTable";
        if (ImGui.BeginTable(tableId, columnCount, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | tableSizing))
        {
            if (advancedMode)
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn("Plate");
                ImGui.TableSetupColumn("Enabled");
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

            foreach (var plate in this.glamourPlateService.GetAllPlates())
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
}
