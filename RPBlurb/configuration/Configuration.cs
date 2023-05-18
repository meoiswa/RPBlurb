using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace RPBlurb
{
  [Serializable]
  public class Configuration : IPluginConfiguration
  {
    public virtual int Version { get; set; } = 0;

    public bool Enabled { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public bool OverlayVisible { get; set; } = true;

    public bool OverlayGrowUp { get; set; } = false;

    public bool OverlayShownInCombat { get; set; } = false;

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
