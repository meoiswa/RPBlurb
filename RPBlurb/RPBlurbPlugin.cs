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
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;

namespace RPBlurb
{
  public sealed class RPBlurbPlugin : IDalamudPlugin
  {
    public string Name => "RPBlurb";

    private const string commandName = "/rpb";
    private const string overlayCommandName = "/rpbo";

    public DalamudPluginInterface PluginInterface { get; init; }
    public CommandManager CommandManager { get; init; }
    public ChatGui ChatGui { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public Condition Condition { get; init; }
    public ClientState ClientState { get; init; }
    public TargetManager TargetManager { get; init; }
    public RPBlurbUI Window { get; init; }

    public RPBlurbOverlay Overlay { get; set; }

    private ICharacterRoleplayData? selfCharacter = null;
    public ICharacterRoleplayData? SelfCharacter
    {
      get
      {
        if (ClientState.LocalPlayer != null && ClientState.LocalPlayer.HomeWorld.GameData != null)
        {
          var world = ClientState.LocalPlayer.HomeWorld.GameData.Name.ToString();
          var user = ClientState.LocalPlayer.Name.ToString();

          if (selfCharacter != null && (selfCharacter.World != world || selfCharacter.User != user))
          {
            selfCharacter.Dispose();
            selfCharacter = null;
          }

          if (selfCharacter == null)
          {
            selfCharacter = CharacterRoleplayDataService.GetCharacter(world, user, false);

            selfCharacter.World = world;
            selfCharacter.User = user;
          }
        }
        return selfCharacter;
      }
    }
    public ICharacterRoleplayData? TargetCharacterRoleplayData { get; set; } = null;

    public CloudCharacterRoleplayDataService CharacterRoleplayDataService { get; init; }
    public FirebaseAuthService FirebaseAuthService { get; init; }

    public RPBlurbPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] ChatGui chatGui)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;
      ChatGui = chatGui;

      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Configuration.Initialize(this);

      WindowSystem = new("RPBlurbPlugin");
      CharacterRoleplayDataService = new CloudCharacterRoleplayDataService();

      FirebaseAuthService = new FirebaseAuthService(this); 

      ClientState = Service.ClientState;

      Condition = Service.Condition;
      TargetManager = Service.TargetManager;
      Service.Framework.Update += OnUpdate;

      Window = new RPBlurbUI(this)
      {
        IsOpen = Configuration.IsVisible
      };

      Overlay = new RPBlurbOverlay(this);

      WindowSystem.AddWindow(Window);
      WindowSystem.AddWindow(Overlay);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "Opens the configuration window"
      });

      CommandManager.AddHandler(overlayCommandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "toggles the overlay"
      });

      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
      CharacterRoleplayDataService.Dispose();
      SelfCharacter?.Dispose();

      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
      Service.Framework.Update -= OnUpdate;

      WindowSystem.RemoveAllWindows();

      CommandManager.RemoveHandler(commandName);
      CommandManager.RemoveHandler(overlayCommandName);
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


    private void SetOverlayVisible(bool isVisible)
    {
      Configuration.OverlayVisible = isVisible;
      Configuration.Save();
    }

    private void OnCommand(string command, string args)
    {
      if (command == overlayCommandName)
      {
        PluginLog.Debug("Overlay toggle");
        SetOverlayVisible(!Configuration.OverlayVisible);
      }
      else
      {
        PluginLog.Debug("Configuration toggle");
        SetVisible(!Configuration.IsVisible);
      }
    }

    private void DrawUI()
    {
      if (Configuration != null)
      {
        WindowSystem.Draw();
      }
    }

    private void DrawConfigUI()
    {
      if (Configuration != null)
      {
        SetVisible(!Configuration.IsVisible);
      }
    }

    public void OnUpdate(Framework framework)
    {
      if (Configuration.Enabled && TargetManager.Target is PlayerCharacter && FirebaseAuthService.State == FirebaseAuthState.LoggedIn)
      {
        var chara = TargetManager.Target as PlayerCharacter;
        if (chara != null)
        {
          var charaUser = chara.Name.ToString();
          var charaWorld = chara.HomeWorld.GameData!.Name.ToString();

          TargetCharacterRoleplayData = CharacterRoleplayDataService.GetCharacter(charaWorld, charaUser);
        }
      }
      else
      {
        TargetCharacterRoleplayData = null;
      }
    }
  }
}
