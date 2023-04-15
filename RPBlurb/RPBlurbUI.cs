using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using System;
using System.Numerics;
using System.Linq;
using Dalamud.Game.ClientState;
using System.Net.Http;
using System.Collections.Generic;
using System.Web;
using Dalamud.Interface.GameFonts;

namespace RPBlurb
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class RPBlurbUI : Window, IDisposable
  {
    private readonly RPBlurbPlugin plugin;
    private bool pendingSave;
    private bool pendingModified;
    private GameFontHandle jupiterStyleLarge;
    private GameFontHandle axisStyleLarge;
    private GameFontHandle trumpGothicStyleLarge;
    private GameFontHandle titleStyle;
    private GameFontHandle itallicsStyle;

    public RPBlurbUI(RPBlurbPlugin plugin)
      : base(
        "RPBlurb##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.plugin = plugin;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };

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

    public override void OnClose()
    {
      base.OnClose();
      plugin.Configuration.IsVisible = false;
      plugin.Configuration.Save();
    }

    private void DrawSectionMasterEnable()
    {
      // can't ref a property, so use a local copy
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }
    }

    public void DrawSelfForm()
    {
      if (plugin.SelfCharacter != null)
      {
        if (plugin.SelfCharacter.Loading)
        {
          ImGui.Text("Loading...");
          return;
        }

        ImGui.Text($"{plugin.SelfCharacter.User} @ {plugin.SelfCharacter.World}");

        if (ImGui.BeginTable("##SelfTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
        {
          ImGui.TableNextColumn();
          ImGui.Text("Name");

          ImGui.TableNextColumn();
          var name = plugin.SelfCharacter.Name ?? "";
          ImGui.SetNextItemWidth(370);
          if (ImGui.InputText("##Name", ref name, 255))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Name = name;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Style");

          ImGui.TableNextColumn();
          var style = plugin.SelfCharacter.NameStyle ?? 0;
          ImGui.SetNextItemWidth(370);
          if (ImGui.Combo("##NameStyle", ref style, "Jupiter\0Axis\0Trump Gothic\0"))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.NameStyle = style;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Title");

          ImGui.TableNextColumn();
          var title = plugin.SelfCharacter.Title ?? "";
          ImGui.SetNextItemWidth(370);
          if (ImGui.InputText("##Title", ref title, 255))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Title = title;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Alignment");

          ImGui.TableNextColumn();
          var alignment = plugin.SelfCharacter.Alignment ?? "";
          ImGui.SetNextItemWidth(370);
          if (ImGui.InputText("##Alignment", ref alignment, 255))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Alignment = alignment;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Status");

          ImGui.TableNextColumn();
          var status = plugin.SelfCharacter.Status ?? "";
          ImGui.SetNextItemWidth(370);
          if (ImGui.InputText("##Status", ref status, 255))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Status = status;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Description");

          ImGui.TableNextColumn();
          var description = plugin.SelfCharacter.Description ?? "";
          if (ImGui.InputTextMultiline("##Description", ref description, 255, new Vector2(370, 100)))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Description = description;
          }

          ImGui.EndTable();
        }


        // Add a button to save the Self Character data
        if (pendingSave || !plugin.SelfCharacter.Modified)
        {
          ImGui.BeginDisabled();
        }
        if (ImGui.Button("Save"))
        {
          pendingSave = true;
          _ = plugin.CharacterRoleplayDataService.SetCharacterAsync(plugin.SelfCharacter, (result) => {
            PluginLog.LogDebug($"SetCharacterAsync result: {result}");
            pendingModified = true;
          });
        }

        if (pendingModified && plugin.SelfCharacter.Modified)
        {
          plugin.SelfCharacter.Modified = false;
          pendingModified = false;
          pendingSave = false;
        }

        if (pendingSave)
        {
          ImGui.SameLine();
          ImGui.Text("Saving...");
        }

        if (pendingSave || !plugin.SelfCharacter.Modified)
        {
          ImGui.EndDisabled();
        }

      }
      else
      {
        ImGui.Text("Loading...");
      }
    }

    public void DrawDataForm(CharacterRoleplayData? data)
    {
      ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
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
            TextCentered(data.Name);
          }
          else
          {
            TextCentered(data.User ?? "");
          }

          if (fontPushed)
          {
            ImGui.PopFont();
          }

          if (!string.IsNullOrWhiteSpace(data.Title))
          {
            ImGui.PushFont(titleStyle.ImFont);
            TextCentered(data.Title);
            ImGui.PopFont();
          }

          if (!string.IsNullOrWhiteSpace(data.Alignment))
          {
            ImGui.PushFont(itallicsStyle.ImFont);
            TextCentered("\u00AB" + data.Alignment + "\u00BB");
            ImGui.PopFont();
          }

          if (!string.IsNullOrWhiteSpace(data.Status))
          {
            ImGui.Separator();
            TextCentered(data.Status);
          }

          if (!string.IsNullOrWhiteSpace(data.Description))
          {
            ImGui.Separator();
            ImGui.Text(data.Description);
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
      ImGui.PopStyleVar();
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      if (ImGui.BeginTabBar("tabs"))
      {
        if (ImGui.BeginTabItem("Target"))
        {
          DrawDataForm(plugin.TargetCharacterRoleplayData);
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Self"))
        {
          DrawSelfForm();
          ImGui.Separator();
          ImGui.Text("Preview: ");
          ImGui.Separator();
          DrawDataForm(plugin.SelfCharacter);
          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
      }
    }

    private void TextCentered(string text)
    {
      var windowWidth = ImGui.GetWindowSize().X;
      var textWidth = ImGui.CalcTextSize(text).X;

      ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
      ImGui.Text(text);
    }

    private void TextMultilineCentered(string text, Vector4? color = null)
    {
      var win_width = ImGui.GetWindowSize().X;
      var text_width = ImGui.CalcTextSize(text).X;

      var text_indentation = (win_width - text_width) * 0.5f;

      var min_indentation = 20.0f;
      if (text_indentation <= min_indentation)
      {
        text_indentation = min_indentation;
      }

      ImGui.NewLine();
      ImGui.SameLine(text_indentation);
      ImGui.PushTextWrapPos(win_width - text_indentation);
      if (color != null)
      {
        ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
      }
      ImGui.TextWrapped(text);
      if (color != null)
      {
        ImGui.PopStyleColor();
      }
      ImGui.PopTextWrapPos();
    }
  }
}
