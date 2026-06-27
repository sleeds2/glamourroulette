using Dalamud.Interface.Windowing;
using GlamourRoulette.Configuration;
using GlamourRoulette.Game;
using Dalamud.Bindings.ImGui;

namespace GlamourRoulette.UI;

internal sealed class PluginUi : IDisposable
{
    private readonly WindowSystem windowSystem = new("GlamourRoulette");
    private readonly MainWindow mainWindow;

    public PluginUi(PluginConfiguration configuration, GlamourPlateService glamourPlateService)
    {
        this.mainWindow = new MainWindow(configuration, glamourPlateService);
        this.windowSystem.AddWindow(this.mainWindow);
    }

    public void Draw()
    {
        this.windowSystem.Draw();
    }

    public void ToggleMainWindow()
    {
        this.mainWindow.Toggle();
    }

    public void Dispose()
    {
        this.windowSystem.RemoveAllWindows();
    }

    private sealed class MainWindow : Window
    {
        private readonly PluginConfiguration configuration;
        private readonly GlamourPlateService glamourPlateService;

        public MainWindow(PluginConfiguration configuration, GlamourPlateService glamourPlateService)
            : base("Glamour Roulette##MainWindow")
        {
            this.configuration = configuration;
            this.glamourPlateService = glamourPlateService;
            this.Size = new System.Numerics.Vector2(420, 500);
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
            ImGui.TextUnformatted("Enabled glamour plates");

            for (var plate = 1; plate <= 20; plate++)
            {
                var enabled = this.configuration.EnabledPlateNumbers.Contains(plate);
                if (ImGui.Checkbox($"Plate {plate}", ref enabled))
                {
                    if (enabled)
                    {
                        this.configuration.EnabledPlateNumbers.Add(plate);
                    }
                    else
                    {
                        this.configuration.EnabledPlateNumbers.Remove(plate);
                    }

                    this.configuration.EnabledPlateNumbers = this.configuration.EnabledPlateNumbers.Distinct().Order().ToList();
                    this.configuration.Save();
                }
            }

            ImGui.Separator();
            if (ImGui.Button("Roll glamour plate"))
            {
                _ = this.glamourPlateService.ApplyRandomPlate();
            }
        }
    }
}
