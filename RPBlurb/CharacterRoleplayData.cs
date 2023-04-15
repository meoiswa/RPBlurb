using System.Threading.Tasks;
using Dalamud.Logging;
using Google.Cloud.Firestore;

namespace RPBlurb
{
  public class CharacterRoleplayData
  {
    private readonly DocumentReference docRef;
    private readonly FirestoreChangeListener listener;

    public CharacterRoleplayData(DocumentReference docRef)
    {
      this.docRef = docRef;
      listener = CreateListener();
    }

    ~CharacterRoleplayData() 
    {
      PluginLog.LogDebug($"CharacterRoleplayData destructor called: {World}@{User}");
      listener.StopAsync().Wait();
    }

    private FirestoreChangeListener CreateListener()
    {
      return docRef.Listen(snapshot =>
      { 
        if (snapshot.Exists)
        { 
          if (snapshot.TryGetValue<string>("World", out var world))
          {
            World = world;
          }
          if (snapshot.TryGetValue<string>("User", out var user))
          {
            User = user;
          }
          if (snapshot.TryGetValue<string>("Name", out var name))
          {
            Name = name;
          }
          if (snapshot.TryGetValue<string>("Description", out var description))
          {
            Description = description;
          }
          if (snapshot.TryGetValue<string>("Alignment", out var alignment))
          {
            Alignment = alignment;
          }
          if (snapshot.TryGetValue<string>("Status", out var status))
          {
            Status = status;
          }

          Modified = false;
        }
        else
        {
          Invalid = true;
        }
      });
    }

    public Task StopAsync()
    {
      return listener.StopAsync();
    }

    public string? World { get; set; }
    public string? User { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Alignment { get; set; }
    public string? Status { get; set; }
    public bool Invalid { get; set; }
    public bool Modified { get; set; }
  }
}
