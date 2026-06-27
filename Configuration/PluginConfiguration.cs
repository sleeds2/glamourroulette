using Dalamud.Configuration;
using Dalamud.Plugin;

namespace GlamourRoulette.Configuration;

public sealed class PluginConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool EnableChatMessages { get; set; } = true;

    public List<int> EnabledPlateNumbers { get; set; } = Enumerable.Range(1, 20).ToList();

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface dalamudPluginInterface)
    {
        this.pluginInterface = dalamudPluginInterface;
    }

    public void Save()
    {
        this.pluginInterface?.SavePluginConfig(this);
    }
}
