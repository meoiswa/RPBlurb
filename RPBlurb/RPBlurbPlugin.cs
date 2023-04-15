using System.IO;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace RPBlurb
{
  public sealed class RPBlurbPlugin : IDalamudPlugin
  {
    public string Name => "RPBlurb";

    private const string commandName = "/RPBlurb";

    private PlayerCharacter? previousTarget;

    public DalamudPluginInterface PluginInterface { get; init; }
    public CommandManager CommandManager { get; init; }
    public ChatGui ChatGui { get; init; }
    public ConfigurationMKII Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public Condition Condition { get; init; }
    public ClientState ClientState { get; init; }
    public TargetManager TargetManager { get; init; }
    public RPBlurbUI Window { get; init; }

    public CharacterRoleplayData? SelfCharacter { get; set; }
    public CharacterRoleplayDataRequest? TargetCharacterRequest { get; set; }
    public bool ForceCacheClearForTargetCharacter { get; set; }

    public CharacterRoleplayDataService CharacterRoleplayDataService { get; init; }

    public RPBlurbPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] ChatGui chatGui)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;
      ChatGui = chatGui;
      WindowSystem = new("RPBlurbPlugin");
      CharacterRoleplayDataService = new CharacterRoleplayDataService();

      Configuration = LoadConfiguration();
      Configuration.Initialize(this);

      ClientState = Service.ClientState;

      SelfCharacter = new()
      {
        Name = ClientState.LocalPlayer?.Name.ToString(),
        World = ClientState.LocalPlayer?.HomeWorld.GameData?.Name.ToString()
      };

      Condition = Service.Condition;
      TargetManager = Service.TargetManager;
      Service.Framework.Update += OnUpdate;

      Window = new RPBlurbUI(this)
      {
        IsOpen = Configuration.IsVisible
      };

      WindowSystem.AddWindow(Window);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "opens the configuration window"
      });

      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
      Service.Framework.Update -= OnUpdate;

      WindowSystem.RemoveAllWindows();

      CommandManager.RemoveHandler(commandName);
    }

    private ConfigurationMKII LoadConfiguration()
    {
      JObject? baseConfig = null;
      if (File.Exists(PluginInterface.ConfigFile.FullName))
      {
        var configJson = File.ReadAllText(PluginInterface.ConfigFile.FullName);
        baseConfig = JObject.Parse(configJson);
      }

      if (baseConfig != null)
      {
        if ((int?)baseConfig["Version"] == 0)
        {
          var configmki = baseConfig.ToObject<ConfigurationMKI>();
          if (configmki != null)
          {
            return ConfigurationMKII.FromConfigurationMKI(configmki);
          }
        }
        else if ((int?)baseConfig["Version"] == 1)
        {
          return baseConfig.ToObject<ConfigurationMKII>() ?? new ConfigurationMKII();
        }
      }

      return new ConfigurationMKII();
    }

    public void SaveConfiguration()
    {
      var configJson = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
      File.WriteAllText(PluginInterface.ConfigFile.FullName, configJson);
    }

    private void SetVisible(bool isVisible)
    {
      Configuration.IsVisible = isVisible;
      Configuration.Save();

      Window.IsOpen = Configuration.IsVisible;
    }

    private void OnCommand(string command, string args)
    {
      SetVisible(!Configuration.IsVisible);
    }

    private void DrawUI()
    {
      WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
      SetVisible(!Configuration.IsVisible);
    }

    public void OnUpdate(Framework framework)
    {
      if (TargetManager.Target is PlayerCharacter)
      {
        var chara = (TargetManager.Target as PlayerCharacter);
        if (chara != null)
        {
          var charaName = chara.Name.ToString();
          var charaWorld = chara.HomeWorld.GameData!.Name.ToString();

          if (ForceCacheClearForTargetCharacter)
          {
            ForceCacheClearForTargetCharacter = false;
            CharacterRoleplayDataService.RemoveCharacterRequestCache(charaWorld, charaName);
          }

          TargetCharacterRequest = CharacterRoleplayDataService.GetCharacterRequest(charaWorld, charaName);
        }
      }
      else
      {
        TargetCharacterRequest = null;
      }
    }
  }
}
