using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using System.Net.Http;
using System.Collections.Generic;
using System.Web;

namespace RPBlurb
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class RPBlurbUI : Window, IDisposable
  {
    private readonly RPBlurbPlugin plugin;
    private bool pendingSave;
    private bool pendingModified;

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
      if (plugin.SelfCharacter != null && plugin.SelfCharacter.User != null && plugin.SelfCharacter.World != null)
      {
        ImGui.Text($"{plugin.SelfCharacter.User} @ {plugin.SelfCharacter.World}");

        ImGui.Text("Name: ");
        ImGui.SameLine();
        var name = plugin.SelfCharacter.Name ?? "";
        if (ImGui.InputText("##Dame", ref name, 255))
        {
          plugin.SelfCharacter.Modified = true;
          plugin.SelfCharacter.Name = name;
        }

        ImGui.Text("Description: ");
        ImGui.SameLine();
        var description = plugin.SelfCharacter.Description ?? "";
        if (ImGui.InputText("##Description", ref description, 255))
        {
          plugin.SelfCharacter.Modified = true;
          plugin.SelfCharacter.Description = description;
        }

        ImGui.Text("Alignment: ");
        ImGui.SameLine();
        var alignment = plugin.SelfCharacter.Alignment ?? "";
        if (ImGui.InputText("##Alignment", ref alignment, 255))
        {
          plugin.SelfCharacter.Modified = true;
          plugin.SelfCharacter.Alignment = alignment;
        }

        ImGui.Text("Status: ");
        ImGui.SameLine();
        var status = plugin.SelfCharacter.Status ?? "";
        if (ImGui.InputText("##Status", ref status, 255))
        {
          plugin.SelfCharacter.Modified = true;
          plugin.SelfCharacter.Status = status;
        }

        // Add a button to save the Self Character data
        if (pendingSave || !plugin.SelfCharacter.Modified)
        {
          ImGui.BeginDisabled();
        }
        if (ImGui.Button("Save"))
        {
          pendingSave = true;
          _ = plugin.CharacterRoleplayDataService.SetCharacterAsync(plugin.SelfCharacter).ContinueWith((result) => {
            pendingModified = true;
          });
        }

        if (pendingModified && !plugin.SelfCharacter.Modified)
        {
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

    public void DrawTargetForm()
    {
      var tcrd = plugin.TargetCharacterRoleplayData;
      if (tcrd != null)
      {
        if (!tcrd.Invalid)
        {
          if (tcrd.World != null && tcrd.User != null)
          {
            ImGui.Text(tcrd.User + " @ " + tcrd.World);
            ImGui.Text("Name: " + tcrd.Name);
            ImGui.Text("Description: " + tcrd.Description);
            ImGui.Text("Alignment: " + tcrd.Alignment);
            ImGui.Text("Status: " + tcrd.Status);
          }
          else
          {
            ImGui.Text("Loading...");
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

      if (ImGui.Button("Refresh"))
      {
        plugin.ForceCacheClearForTargetCharacter = true;
      }
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      if (ImGui.BeginTabBar("tabs"))
      {
        if (ImGui.BeginTabItem("Target"))
        {
          DrawTargetForm();
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Self"))
        {
          DrawSelfForm();
          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
      }
    }
  }
}
