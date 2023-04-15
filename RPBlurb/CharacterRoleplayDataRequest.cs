using System.Threading.Tasks;
using Dalamud.Logging;

namespace RPBlurb
{
  public class CharacterRoleplayDataRequest
  {
    public CharacterRoleplayData? Data { get; private set; }
    public bool IsCompleted { get; private set; }

    public CharacterRoleplayDataRequest(Task<CharacterRoleplayData?> request)
    {
      IsCompleted = false;
      Data = null;

      request.ContinueWith((task) =>
      {
        PluginLog.LogError($"Error fetching data: {task.Exception?.Message}");
        IsCompleted = true;
      }, TaskContinuationOptions.OnlyOnFaulted);

      request.ContinueWith(async (data) =>
      {
        Data = await data;
        IsCompleted = true;
      }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }
  }
}
