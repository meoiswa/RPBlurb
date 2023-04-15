using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace RPBlurb
{
  [Serializable]
  public class ConfigurationBase : IPluginConfiguration
  {
    public virtual int Version { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private RPBlurbPlugin? plugin;
    public void Initialize(RPBlurbPlugin plugin) => this.plugin = plugin;
    public void Save()
    {
      plugin!.SaveConfiguration();
    }
  }
}
