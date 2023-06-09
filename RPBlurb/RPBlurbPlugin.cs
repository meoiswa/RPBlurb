﻿using System.IO;
using Dalamud.Game;
using Dalamud.Game.Command;
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
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }

    public RPBlurbUI Window { get; init; }
    public RPBlurbOverlay Overlay { get; set; }

    private CharacterRoleplayData? selfCharacter = null;
    public CharacterRoleplayData? SelfCharacter
    {
      get
      {
        if (Service.ClientState.LocalPlayer != null && Service.ClientState.LocalPlayer.HomeWorld.GameData != null)
        {
          var world = Service.ClientState.LocalPlayer.HomeWorld.GameData.Name.ToString();
          var user = Service.ClientState.LocalPlayer.Name.ToString();

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
    public CharacterRoleplayData? TargetCharacterRoleplayData { get; set; }

    public CharacterRoleplayDataService CharacterRoleplayDataService { get; init; }

    public RPBlurbPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;

      WindowSystem = new("RPBlurbPlugin");
      CharacterRoleplayDataService = new CharacterRoleplayDataService();

      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Configuration.Initialize(this);

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
      WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
      SetVisible(!Configuration.IsVisible);
    }

    public void OnUpdate(Framework framework)
    {
      if (Configuration.Enabled && Service.TargetManager.Target is PlayerCharacter)
      {
        var chara = Service.TargetManager.Target as PlayerCharacter;
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
