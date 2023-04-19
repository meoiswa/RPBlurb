using ImGuiNET;

namespace RPBlurb
{
  public static class ImGuiHelper
  {
    public static void TextCentered(string text)
    {
      var windowWidth = ImGui.GetWindowSize().X;
      var textWidth = ImGui.CalcTextSize(text).X;

      ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
      ImGui.Text(text);
    }
  }
}
