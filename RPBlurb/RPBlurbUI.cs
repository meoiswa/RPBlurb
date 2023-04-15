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
      var enabled = plugin.Configuration.MasterEnable;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.MasterEnable = enabled;
        plugin.Configuration.Save();
      }
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("Debug Options"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Debug Options\nUse these to test your settings.");

        if (ImGui.Button("Delete Cache"))
        {
          CharacterRoleplayDataService.ClearCache();
        }

        var debugMessages = plugin.Configuration.DebugMessages;
        if (ImGui.Checkbox("Debug Messages", ref debugMessages))
        {
          plugin.Configuration.DebugMessages = debugMessages;
          plugin.Configuration.Save();
        }

        ImGui.Unindent();
      }
    }

    public void DrawSelfForm()
    {
      if (plugin.SelfCharacter != null)
      {
        ImGui.Indent();

        ImGui.Text("Name: ");
        ImGui.SameLine();
        var name = plugin.SelfCharacter.Name ?? "";
        if (ImGui.InputText("##Name", ref name, 255))
        {
          plugin.SelfCharacter.Name = name;
        }

        ImGui.Text("World: ");
        ImGui.SameLine();
        var world = plugin.SelfCharacter.World ?? "";
        if (ImGui.InputText("##World", ref world, 255))
        {
          plugin.SelfCharacter.World = world;
        }

        ImGui.Text("Description: ");
        ImGui.SameLine();
        var description = plugin.SelfCharacter.Description ?? "";
        if (ImGui.InputText("##Description", ref description, 255))
        {
          plugin.SelfCharacter.Description = description;
        }

        ImGui.Text("Alignment: ");
        ImGui.SameLine();
        var alignment = plugin.SelfCharacter.Alignment ?? "";
        if (ImGui.InputText("##Alignment", ref alignment, 255))
        {
          plugin.SelfCharacter.Alignment = alignment;
        }

        ImGui.Text("Status: ");
        ImGui.SameLine();
        var status = plugin.SelfCharacter.Status ?? "";
        if (ImGui.InputText("##Status", ref status, 255))
        {
          plugin.SelfCharacter.Status = status;
        }

        // Add a button to save the Self Character data
        if (ImGui.Button("Save"))
        {
          _ = CharacterRoleplayDataService.SetCharacterAsync(plugin.SelfCharacter);
        }

        ImGui.Unindent();
      }
    }

    public void DrawTargetForm()
    {
      ImGui.Indent();

      if (plugin.TargetCharacterRequest != null)
      {
        var tcr = plugin.TargetCharacterRequest;
        if (tcr.IsCompleted)
        {
          if (tcr.Data != null)
          {
            ImGui.Text("Name: " + tcr.Data.Name);
            ImGui.Text("World: " + tcr.Data.World);
            ImGui.Text("Description: " + tcr.Data.Description);
            ImGui.Text("Alignment: " + tcr.Data.Alignment);
            ImGui.Text("Status: " + tcr.Data.Status);
          }
          else
          {
            ImGui.Text("No Roleplay Sheet");
          }
        }
        else
        {
          ImGui.Text("Loading...");
        }
      }
      else
      {
        ImGui.Text("No target");
      }

      ImGui.Unindent();
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      ImGui.Separator();

      DrawTargetForm();

      ImGui.Separator();

      DrawSelfForm();

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
