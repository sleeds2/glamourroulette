using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using System.Numerics;

namespace GlamourRoulette.Windows;

internal sealed class ConfigWindow : Window
{
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;

    public ConfigWindow(PluginConfiguration configuration, GlamourPlateService glamourPlateService)
        : base("Glamour Roulette Settings##ConfigWindow")
    {
        this.configuration = configuration;
        this.glamourPlateService = glamourPlateService;
        this.Size = new Vector2(460, 560);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var enableChatMessages = this.configuration.EnableChatMessages;
        if (ImGui.Checkbox("Post results to chat", ref enableChatMessages))
        {
            this.configuration.EnableChatMessages = enableChatMessages;
            this.configuration.Save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Glamour plates");
        ImGui.TextWrapped("Enable the saved, non-empty glamour plates that should be included when Glamour Roulette randomly selects a plate. Empty glamour plates are never selected.");

        if (ImGui.BeginTable("##GlamourPlateSettingsTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("Plate", ImGuiTableColumnFlags.WidthStretch, 2.0f);
            ImGui.TableSetupColumn("Random selection", ImGuiTableColumnFlags.WidthStretch, 1.4f);
            ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 85.0f);
            ImGui.TableHeadersRow();

            foreach (var plate in this.glamourPlateService.GetAvailablePlates())
            {
                var enabled = this.configuration.IsPlateEligible(plate.Number, plate.IsEmpty);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(string.IsNullOrWhiteSpace(plate.Name) ? $"Plate {plate.Number}" : plate.Name);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(plate.IsEmpty ? "Empty" : enabled ? "Enabled" : "Disabled");

                ImGui.TableNextColumn();
                ImGui.PushID(plate.Number);
                if (plate.IsEmpty)
                {
                    ImGui.BeginDisabled();
                }

                if (ImGui.Checkbox("##PlateEnabled", ref enabled))
                {
                    this.configuration.SetPlateEligibility(plate.Number, enabled);
                }

                if (plate.IsEmpty)
                {
                    ImGui.EndDisabled();
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }
}
