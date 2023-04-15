using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace RPBlurb
{
  public class CharacterRoleplayDataService
  {
    private static readonly HttpClient client = new();
    private static readonly string setCharacterFunctionUrl = "https://us-central1-gwhet-box.cloudfunctions.net/setCharacter";
    private static readonly string getCharacterFunctionUrl = "https://us-central1-gwhet-box.cloudfunctions.net/getCharacter";

    private static readonly Dictionary<string, CharacterRoleplayDataRequest> Cache = new();

    public static async Task<bool> SetCharacterAsync(CharacterRoleplayData character, CancellationToken cancellationToken = default)
    {
      using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
      using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

      using var request = new HttpRequestMessage(HttpMethod.Post, setCharacterFunctionUrl);

      var json = JsonConvert.SerializeObject(character, new JsonSerializerSettings()
      {
        ContractResolver = new DefaultContractResolver()
        {
          NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.Indented
      });
      request.Content = new StringContent(json);

      PluginLog.Log(json);

      using var response = await client
          .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedToken.Token)
          .ConfigureAwait(false);
      response.EnsureSuccessStatusCode();
      return true;
    }

    public static CharacterRoleplayDataRequest GetCharacterRequest(string world, string name)
    {
      var uniqueKey = $"{name}@{world}";
      var isCached = Cache.TryGetValue(uniqueKey, out CharacterRoleplayDataRequest? value);

      if (isCached)
      {
        return value!;
      }

      PluginLog.Log($"Fetching data from server");
      Cache[uniqueKey] = new CharacterRoleplayDataRequest(GetCharacterAsync(world, name));
      return Cache[uniqueKey];
    }

    public static async Task<CharacterRoleplayData?> GetCharacterAsync(string world, string name, CancellationToken cancellationToken = default)
    {
      using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
      using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

      var job = new JObject
      {
        ["world"] = world,
        ["name"] = name
      };

      var jsonContent = JsonConvert.SerializeObject(job);

      PluginLog.Log(jsonContent);

      var content = new StringContent(jsonContent);

      using var request = new HttpRequestMessage(HttpMethod.Post, getCharacterFunctionUrl);
      request.Content = content;

      using var response = await client
          .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedToken.Token)
          .ConfigureAwait(false);

      if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
      {
        return null;
      }
      else
      {
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(combinedToken.Token);
        return JsonConvert.DeserializeObject<CharacterRoleplayData>(json);
      }
    }

    public static void ClearCache()
    {
      Cache.Clear();
    }
    public static void ClearCacheElement(string key)
    {
      Cache.Remove(key);
    }
  }
}
