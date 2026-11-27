using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using GlamourRoulette.Services;
using System.Numerics;

namespace GlamourRoulette.Windows;

internal sealed class ConfigWindow : Window
{
    private const int MaxPlateNameUtf8Bytes = 256;
    private static readonly PlateRole[] Roles = [PlateRole.Tank, PlateRole.Healer, PlateRole.Melee, PlateRole.Ranged, PlateRole.Magic];

    private readonly PluginServices services;
    private readonly PluginConfiguration configuration;
    private readonly GlamourPlateService glamourPlateService;
    private int? editingPlateNumber;
    private string plateNameDraft = string.Empty;
    private bool focusPlateNameInput;

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
        // Keep Dalamud's window close hotkey disabled until the Escape press is released.
        if (this.editingPlateNumber.HasValue)
        {
            this.RespectCloseHotkey = false;
            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) && ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            {
                this.editingPlateNumber = null;
            }
        }
        else if (!ImGui.IsKeyDown(ImGuiKey.Escape))
        {
            this.RespectCloseHotkey = true;
        }

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
            this.focusPlateNameInput = this.editingPlateNumber.HasValue;
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
                ImGui.TableSetupColumn("Plate", ImGuiTableColumnFlags.WidthFixed, 160.0f * ImGuiHelpers.GlobalScale);
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

            var style = ImGui.GetStyle();
            var disabledRowAlpha = style.Alpha * style.DisabledAlpha;
            foreach (var plate in this.glamourPlateService.GetAllPlates())
            {
                var enabled = this.configuration.IsPlateEligible(plate.Number);
                var dimRow = !enabled;

                ImGui.TableNextRow();
                if (dimRow)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, disabledRowAlpha);
                }

                ImGui.PushID(plate.Number);
                ImGui.TableNextColumn();
                this.DrawPlateName(plate.Number);

                ImGui.TableNextColumn();
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
                if (dimRow)
                {
                    ImGui.PopStyleVar();
                }
            }

            ImGui.EndTable();
        }
    }

    public override void OnClose()
    {
        this.editingPlateNumber = null;
        this.RespectCloseHotkey = true;
    }

    private void DrawPlateName(int plateNumber)
    {
        var plateLabel = $"Plate {plateNumber}";
        if (this.editingPlateNumber == plateNumber)
        {
            ImGui.TextUnformatted(plateLabel);
            if (this.focusPlateNameInput)
            {
                ImGui.SetKeyboardFocusHere();
                this.focusPlateNameInput = false;
            }

            ImGui.SetNextItemWidth(-1);
            var submitted = ImGui.InputTextWithHint("##PlateName", "Optional name", ref this.plateNameDraft,
                MaxPlateNameUtf8Bytes, ImGuiInputTextFlags.EnterReturnsTrue);
            if (ImGui.Button("Save") || submitted)
            {
                this.configuration.SetPlateName(plateNumber, this.plateNameDraft);
                this.editingPlateNumber = null;
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                this.editingPlateNumber = null;
            }

            return;
        }

        var name = this.configuration.GetPlateName(plateNumber);
        if (ImGuiComponents.IconButton("RenamePlate", FontAwesomeIcon.PencilAlt))
        {
            this.editingPlateNumber = plateNumber;
            this.plateNameDraft = name;
            this.focusPlateNameInput = true;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Rename {plateLabel}");
        }

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.PushTextWrapPos(0);
        ImGui.TextUnformatted(name.Length == 0 ? plateLabel : $"{plateLabel} · {name}");
        ImGui.PopTextWrapPos();
    }
}
