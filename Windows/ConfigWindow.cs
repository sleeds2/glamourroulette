using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using System.Numerics;

namespace GlamourRoulette.Windows;

internal sealed class ConfigWindow : Window
{
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;
    private readonly PluginNotifier notifier;

    public ConfigWindow(PluginConfiguration configuration, GlamourPlateService glamourPlateService, PluginNotifier notifier)
        : base("Glamour Roulette Settings##ConfigWindow")
    {
        this.configuration = configuration;
        this.glamourPlateService = glamourPlateService;
        this.notifier = notifier;
        this.Size = new Vector2(360, 560);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        if (ImGui.Button("Roll random glamour plate"))
        {
            this.ApplyRandomPlateFromConfigWindow();
        }

        ImGui.SameLine();
        if (ImGui.Button("Open Glamour Plate UI"))
        {
            var result = this.glamourPlateService.OpenGlamourPlateUi();
            if (!result.Success)
            {
                this.notifier.PrintResult(result);
            }
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Glamour plates");
        ImGui.TextWrapped("Enable the glamour plates that should be included when Glamour Roulette randomly selects a plate.");
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.2f, 0.2f, 1.0f));
        ImGui.TextWrapped("Please disable any empty glamour plates.");
        ImGui.PopStyleColor();

        if (ImGui.BeginTable("##GlamourPlateSettingsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("Plate", ImGuiTableColumnFlags.WidthStretch, 2.0f);
            ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 85.0f);
            ImGui.TableHeadersRow();

            foreach (var plate in this.glamourPlateService.GetAvailablePlates())
            {
                var enabled = this.configuration.IsPlateEligible(plate.Number);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(string.IsNullOrWhiteSpace(plate.Name) ? $"Plate {plate.Number}" : plate.Name);

                ImGui.TableNextColumn();
                ImGui.PushID(plate.Number);
                if (ImGui.Checkbox("##PlateEnabled", ref enabled))
                {
                    this.configuration.SetPlateEligibility(plate.Number, enabled);
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }

    private void ApplyRandomPlateFromConfigWindow()
    {
        try
        {
            var result = this.glamourPlateService.ApplyRandomPlate();
            this.notifier.PrintResult(result);
        }
        catch (Exception ex)
        {
            this.notifier.PrintUnexpectedError(ex, "applying a random glamour plate from the config window");
        }
    }
}
