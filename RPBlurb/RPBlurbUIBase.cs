using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface;
using Dalamud.Interface.Raii;

namespace RPBlurb
{
  public abstract class RPBlurbUIBase : Window, IDisposable
  {
    protected readonly RPBlurbPlugin plugin;
    protected readonly GameFontHandle jupiterStyleLarge;
    protected readonly GameFontHandle axisStyleLarge;
    protected readonly GameFontHandle trumpGothicStyleLarge;
    protected readonly GameFontHandle titleStyle;
    protected readonly GameFontHandle itallicsStyle;

    public RPBlurbUIBase(RPBlurbPlugin plugin, string title, ImGuiWindowFlags flags)
      : base(
        title,
        flags
      )
    {
      this.plugin = plugin;

      jupiterStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Jupiter, 36));
      axisStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 36));
      trumpGothicStyleLarge = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 36));

      titleStyle = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 20));
      itallicsStyle = plugin.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 14) { Italic = true });
    }

    public virtual void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
    }

    public void DrawDataForm(CharacterRoleplayData? data)
    {
      if (data != null)
      {
        if (data.Loading)
        {
          ImGui.Text("Loading...");
        }
        else if (!data.Invalid)
        {
          {
            using var _ = (data.NameStyle ?? 0) switch
            {
              0 => ImRaii.PushFont(jupiterStyleLarge.ImFont),
              1 => ImRaii.PushFont(axisStyleLarge.ImFont),
              2 => ImRaii.PushFont(trumpGothicStyleLarge.ImFont),
              _ => null
            };

            if (!string.IsNullOrWhiteSpace(data.Name))
            {
              ImGuiHelpers.CenteredText(data.Name);
            }
            else
            {
              ImGuiHelpers.CenteredText(data.User ?? "");
            }
          }

          if (!string.IsNullOrWhiteSpace(data.Title))
          {
            using var _ = ImRaii.PushFont(titleStyle.ImFont);
            ImGuiHelpers.CenteredText(data.Title);
          }

          if (!string.IsNullOrWhiteSpace(data.Alignment))
          {
            using var _ = ImRaii.PushFont(itallicsStyle.ImFont);
            ImGuiHelpers.CenteredText("\u00AB" + data.Alignment + "\u00BB");
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
  }
}
