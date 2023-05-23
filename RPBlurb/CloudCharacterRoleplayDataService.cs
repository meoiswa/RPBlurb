using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RPBlurb
{
  public class CloudCharacterRoleplayDataService : IDisposable
  {
    private static readonly string setCharacterFunctionUrl = "https://us-central1-gwhet-box.cloudfunctions.net/setCharacter";

    private readonly HttpClient client = new();
    private readonly FirestoreDb db;
    private readonly Dictionary<string, ICharacterRoleplayData> Cache = new();

    public CloudCharacterRoleplayDataService()
    {
      var jsonString = File.ReadAllText(Path.Join(new FileInfo(GetType().Assembly.Location).DirectoryName, "gwhet-box-c9b5ebd697a7.json"));
      var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
      db = FirestoreDb.Create("gwhet-box", builder.Build());
    }

    public async Task<bool> SetCharacterAsync(ICharacterRoleplayData data, Action<bool> callback, CancellationToken cancellationToken = default)
    {
      PluginLog.LogDebug($"SetCharacterAsync: {data.User}@{data.World}");

      using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
      using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

      using var request = new HttpRequestMessage(HttpMethod.Post, setCharacterFunctionUrl);

      var job = new JObject()
      {
        ["World"] = data.World,
        ["User"] = data.User,
        ["Name"] = data.Name,
        ["NameStyle"] = data.NameStyle,
        ["Title"] = data.Title,
        ["Description"] = data.Description,
        ["Alignment"] = data.Alignment,
        ["Status"] = data.Status
      };

      var json = JsonConvert.SerializeObject(job);
      request.Content = new StringContent(json);

      PluginLog.LogDebug($"SetCharacterAsync: {data.User}@{data.World}: {json}");

      try
      {
        using var response = await client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedToken.Token);
        response.EnsureSuccessStatusCode();
      }
      catch (Exception ex)
      {
        PluginLog.LogError(ex.ToString());
        callback(false);
        return false;
      }

      callback(true);
      return true;
    }

    public DocumentReference GetCharacterDocRef(string world, string user)
    {
      var docRef = db.Collection("rp").Document(world).Collection("characters").Document(user);
      return docRef;
    }

    public ICharacterRoleplayData GetCharacter(string world, string user, bool cache = true)
    {
      if (cache && Cache.TryGetValue($"{user}@{world}", out ICharacterRoleplayData? value))
      {
        return value;
      }
      else
      {
        PluginLog.LogDebug("GetCharacter: " + user + "@" + world);
        value = new CloudCharacterRoleplayData(GetCharacterDocRef(world, user));
        if (cache)
        {
          Cache[$"{user}@{world}"] = value;
        }
        return value;
      }
    }

    public void ClearCache()
    {
      Cache.Clear();
    }

    internal void RemoveCharacterRequestCache(string world, string user)
    {
      var uniqueKey = $"{user}@{world}";
      Cache.Remove(uniqueKey);
    }

    public void Dispose()
    {
      foreach (var key in Cache.Keys)
      {
        Cache[key].Dispose();
      }

      Cache.Clear();

      GC.SuppressFinalize(this);
    }
  }
}
