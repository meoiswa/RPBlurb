using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;

namespace RPBlurb
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class RPBlurbOverlay : RPBlurbWindowBase
  {
    private float LastSizeY = 0;

    public RPBlurbOverlay(RPBlurbPlugin plugin)
      : base(
        plugin,
        "RPBlurb##OverlayWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoDecoration
        | ImGuiWindowFlags.NoInputs
        | ImGuiWindowFlags.NoBringToFrontOnFocus
        | ImGuiWindowFlags.NoFocusOnAppearing
        | ImGuiWindowFlags.NoNavFocus)
    {
      ForceMainWindow = true;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public override void PreOpenCheck()
    {
      IsOpen = plugin.Configuration.Enabled
        && plugin.Configuration.OverlayVisible
        && (plugin.TargetCharacterRoleplayData != null || plugin.Configuration.IsVisible)
        && (plugin.Configuration.OverlayShownInCombat || !plugin.Condition[ConditionFlag.InCombat])
        && (plugin.FirebaseAuthService.State == FirebaseAuthState.LoggedIn);
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
