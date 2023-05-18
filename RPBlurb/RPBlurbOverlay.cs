using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Windowing;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Conditions;

namespace RPBlurb
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class RPBlurbOverlay : Window, IDisposable
  {
    private readonly RPBlurbPlugin plugin;
    private readonly GameFontHandle jupiterStyleLarge;
    private readonly GameFontHandle axisStyleLarge;
    private readonly GameFontHandle trumpGothicStyleLarge;
    private readonly GameFontHandle titleStyle;
    private readonly GameFontHandle itallicsStyle;

    private float LastSizeY = 0;

    public RPBlurbOverlay(RPBlurbPlugin plugin)
      : base("RPBlurb##OverlayWindow")
    {
      ForceMainWindow = true;
      Flags |=
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoDecoration
        | ImGuiWindowFlags.NoInputs
        | ImGuiWindowFlags.NoBringToFrontOnFocus
        | ImGuiWindowFlags.NoFocusOnAppearing
        | ImGuiWindowFlags.NoNavFocus;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };

      this.plugin = plugin;

      jupiterStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Jupiter, 36));
      axisStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 36));
      trumpGothicStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 36));

      titleStyle = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 20));
      itallicsStyle = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 14) { Italic = true });
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void PreOpenCheck()
    {
      IsOpen = plugin.Configuration.Enabled &&
        plugin.Configuration.OverlayVisible &&
        (plugin.TargetCharacterRoleplayData != null || plugin.Configuration.IsVisible) &&
        (plugin.Configuration.OverlayShownInCombat || !plugin.Condition[ConditionFlag.InCombat]);
    }

    public void DrawDataForm(CharacterRoleplayData? data)
    {
      if (data != null)
      {
        if (data.Loading)
        {
          ImGui.Text("Loading...");
          return;
        }
        if (!data.Invalid)
        {
          var fontPushed = false;
          switch (data.NameStyle ?? 0)
          {
            case 0:
              ImGui.PushFont(jupiterStyleLarge.ImFont);
              fontPushed = true;
              break;
            case 1:
              ImGui.PushFont(axisStyleLarge.ImFont);
              fontPushed = true;
              break;
            case 2:
              ImGui.PushFont(trumpGothicStyleLarge.ImFont);
              fontPushed = true;
              break;
            default:
              break;
          }

          if (!string.IsNullOrWhiteSpace(data.Name))
          {
            ImGuiHelpers.CenteredText(data.Name);
          }
          else
          {
            ImGuiHelpers.CenteredText(data.User ?? "");
          }

          if (fontPushed)
          {
            ImGui.PopFont();
          }

          if (!string.IsNullOrWhiteSpace(data.Title))
          {
            ImGui.PushFont(titleStyle.ImFont);
            ImGuiHelpers.CenteredText(data.Title);
            ImGui.PopFont();
          }

          if (!string.IsNullOrWhiteSpace(data.Alignment))
          {
            ImGui.PushFont(itallicsStyle.ImFont);
            ImGuiHelpers.CenteredText("\u00AB" + data.Alignment + "\u00BB");
            ImGui.PopFont();
          }

          if (!string.IsNullOrWhiteSpace(data.Status))
          {
            ImGui.Separator();
            ImGuiHelpers.CenteredText(data.Status);
          }

          if (!string.IsNullOrWhiteSpace(data.Description))
          {
            ImGui.Separator();
            ImGui.TextWrapped(data.Description);
          }
        }
        else
        {
          ImGui.Text("No Roleplay Sheet");
        }
      }
      else
      {
        ImGui.Text("No target");
      }
    }

    public override void Draw()
    {
      if (!plugin.Configuration.OverlayGrowUp)
      {
        if (LastSizeY != 0)
        {
          var windowPos = ImGui.GetWindowPos();
          ImGui.SetWindowPos(new Vector2(windowPos.X, windowPos.Y + LastSizeY));
        }
        LastSizeY = 0;
      }
      else
      {
        var calculatedHeight = 0.0f;
        if (plugin.TargetCharacterRoleplayData != null)
        {
          if (!string.IsNullOrWhiteSpace(plugin.TargetCharacterRoleplayData.Title))
          {
            calculatedHeight += 24.0f;
          }
          if (!string.IsNullOrWhiteSpace(plugin.TargetCharacterRoleplayData.Alignment))
          {
            calculatedHeight += 18.0f;
          }
          if (!string.IsNullOrWhiteSpace(plugin.TargetCharacterRoleplayData.Status))
          {
            calculatedHeight += 25.0f;
          }
          if (!string.IsNullOrWhiteSpace(plugin.TargetCharacterRoleplayData.Description))
          {
            var size = ImGui.CalcTextSize(plugin.TargetCharacterRoleplayData.Description, 468.0f);
            calculatedHeight += 27.0f + size.Y;
          }
        }

        if (calculatedHeight != LastSizeY)
        {
          var delta = calculatedHeight - LastSizeY;
          LastSizeY = calculatedHeight;
          var windowPos = ImGui.GetWindowPos();
          ImGui.SetWindowPos(new Vector2(windowPos.X, windowPos.Y - delta));
        }
      }

      if (plugin.Configuration.IsVisible)
      {
        Flags &= ~ImGuiWindowFlags.NoInputs;
      }
      else
      {
        Flags |= ImGuiWindowFlags.NoInputs;
      }

      DrawDataForm(plugin.TargetCharacterRoleplayData);
    }
  }
}
