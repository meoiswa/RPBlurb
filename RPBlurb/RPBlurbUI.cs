using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;

namespace RPBlurb
{
  public unsafe class RPBlurbUI : RPBlurbUIBase
  {
    private bool pendingSave;
    private bool pendingModified;

    public RPBlurbUI(RPBlurbPlugin plugin)
      : base(
        plugin,
        "RPBlurb##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public override void Dispose()
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

      var overlayInCombat = plugin.Configuration.OverlayShownInCombat;
      if (ImGui.Checkbox("Show Overlay in combat", ref overlayInCombat))
      {
        plugin.Configuration.OverlayShownInCombat = overlayInCombat;
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
