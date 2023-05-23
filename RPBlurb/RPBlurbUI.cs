using Dalamud.Logging;
using ImGuiNET;
using System.Numerics;

namespace RPBlurb
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class RPBlurbUI : RPBlurbWindowBase
  {
    private bool pendingSave;
    private bool pendingModified;
    
    public string Password { get; set; } = "";

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

      var hidden = plugin.Configuration.HideEmail;
      if (ImGui.Checkbox("Hide Email", ref hidden))
      {
        plugin.Configuration.HideEmail = hidden;
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

    public void DrawAuthSection()
    {
      var state = plugin.FirebaseAuthService.State;
      if (state == FirebaseAuthState.LoggedOut || state == FirebaseAuthState.Error)
      {
        ImGui.Text("Logging in is required to use this plugin.");
        ImGui.Text("Visit the website rpblurb.meoiswa.cat to create an account.");

        string email = plugin.Configuration.Email;
        if (ImGui.InputText("##Email", ref email, 256))
        {
          plugin.Configuration.Email = email;
          plugin.Configuration.Save();
        }

        string password = Password;
        if (ImGui.InputText("##Password", ref password, 256, ImGuiInputTextFlags.Password))
        {
          Password = password;
          plugin.Configuration.Save();
        }

        if (ImGui.Button("Login"))
        {
          plugin.FirebaseAuthService.LoginWithEmailAndPassword(plugin.Configuration.Email, Password);
        }
      }
      else if (state == FirebaseAuthState.LoggingIn)
      {
        ImGui.Text("Logging in...");
      }
      else if (state == FirebaseAuthState.LoggedIn)
      {
        var email = plugin.Configuration.Email;
        if (plugin.Configuration.HideEmail)
        {
          // replace all but the first letter of the username and domains with asterisks
          var parts = email.Split('@');
          var username = parts[0];
          var domain = parts[1];
          username = username[0] + new string('*', username.Length - 1);
          domain = domain[0] + new string('*', domain.Length - 1);
          email = $"{username}@{domain}";
        }
        ImGui.Text($"Logged in as {email}");
        ImGui.SameLine();
        if (ImGui.Button("Logout"))
        {
          plugin.FirebaseAuthService.Logout();
        }
      }
    }

    public override void Draw()
    {
      DrawAuthSection();

      if (plugin.FirebaseAuthService.State == FirebaseAuthState.LoggedIn)
      {
        ImGui.Separator();

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
}
