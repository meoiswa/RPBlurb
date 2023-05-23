using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface;

namespace RPBlurb
{
  public abstract class RPBlurbWindowBase : Window, IDisposable
  {
    protected readonly RPBlurbPlugin plugin;
    protected readonly GameFontHandle jupiterStyleLarge;
    protected readonly GameFontHandle axisStyleLarge;
    protected readonly GameFontHandle trumpGothicStyleLarge;
    protected readonly GameFontHandle titleStyle;
    protected readonly GameFontHandle itallicsStyle;

    public RPBlurbWindowBase(RPBlurbPlugin plugin, string title, ImGuiWindowFlags flags) : base(title, flags)
    {
      this.plugin = plugin;

      jupiterStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Jupiter, 36));
      axisStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 36));
      trumpGothicStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 36));

      titleStyle = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 20));
      itallicsStyle = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 14) { Italic = true });
    }

    public void DrawDataForm(ICharacterRoleplayData? data)
    {
      if (data != null)
      {
        if (data.Loading)
        {
          ImGui.Text("Loading...");
        }
        else if (!data.Invalid)
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
            ImGuiHelpers.SafeTextWrapped(data.Description);
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


    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }
  }
}
