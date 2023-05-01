using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface;

namespace RPBlurb
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class RPBlurbUI : Window, IDisposable
  {
    private readonly RPBlurbPlugin plugin;
    private bool pendingSave;
    private bool pendingModified;
    private readonly GameFontHandle jupiterStyleLarge;
    private readonly GameFontHandle axisStyleLarge;
    private readonly GameFontHandle trumpGothicStyleLarge;
    private readonly GameFontHandle titleStyle;
    private readonly GameFontHandle itallicsStyle;

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

    private void DrawSectionToggles()
    {
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }
    }

    private void DrawSectionOverlay()
    {
      var overlay = plugin.Configuration.OverlayVisible;
      if (ImGui.Checkbox("Overlay", ref overlay))
      {
        plugin.Configuration.OverlayVisible = overlay;
        plugin.Configuration.Save();
      }

      ImGui.SameLine();
      ImGui.Text("(Draggable while this window is open)");

      var growUp = plugin.Configuration.OverlayGrowUp;
      if (ImGui.Checkbox("Resize Upwards", ref growUp))
      {
        plugin.Configuration.OverlayGrowUp = growUp;
        plugin.Configuration.Save();
      }

      ImGui.SameLine();
      ImGui.Text("(Experimental)");
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
          if (ImGui.InputText("##Name", ref name, 256))
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
          if (ImGui.InputText("##Title", ref title, 256))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Title = title;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Alignment");

          ImGui.TableNextColumn();
          var alignment = plugin.SelfCharacter.Alignment ?? "";
          ImGui.SetNextItemWidth(370);
          if (ImGui.InputText("##Alignment", ref alignment, 256))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Alignment = alignment;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Status");

          ImGui.TableNextColumn();
          var status = plugin.SelfCharacter.Status ?? "";
          ImGui.SetNextItemWidth(370);
          if (ImGui.InputText("##Status", ref status, 256))
          {
            plugin.SelfCharacter.Modified = true;
            plugin.SelfCharacter.Status = status;
          }

          ImGui.TableNextColumn();
          ImGui.Text("Description");

          ImGui.TableNextColumn();
          var description = plugin.SelfCharacter.Description ?? "";
          if (ImGui.InputTextMultiline("##Description", ref description, 512, new Vector2(370, 100)))
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
          _ = plugin.CharacterRoleplayDataService.SetCharacterAsync(plugin.SelfCharacter, (result) =>
          {
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
      // ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
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
      // ImGui.PopStyleVar();
    }

    public override void Draw()
    {
      DrawSectionToggles();

      ImGui.Separator();

      DrawSectionOverlay();

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
  }
}
