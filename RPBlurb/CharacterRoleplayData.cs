using System;
using Dalamud.Logging;
using Google.Cloud.Firestore;

namespace RPBlurb
{
  public class CharacterRoleplayData : IDisposable
  {
    private readonly DocumentReference docRef;
    private readonly FirestoreChangeListener listener;
    private bool disposed = false;

    public CharacterRoleplayData(DocumentReference docRef)
    {
      this.docRef = docRef;
      listener = CreateListener();
    }

    private FirestoreChangeListener CreateListener()
    {
      return docRef.Listen(snapshot =>
      {
        PluginLog.LogDebug("Document Snapshot: " + snapshot.Id);
        Loading = false;
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
          if (snapshot.TryGetValue<int?>("NameStyle", out var nameStyle))
          {
            NameStyle = nameStyle ?? 0;
          }
          if (snapshot.TryGetValue<string>("Title", out var title))
          {
            Title = title;
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

          Invalid = false;
          Modified = false;
        }
        else
        {
          Invalid = true;
        }
      });
    }

    public void Dispose()
    {
      if (disposed) return;
      PluginLog.LogDebug($"CharacterRoleplayData Dispose called: {User}@{World}");
      disposed = true;
      listener.StopAsync().Wait();
      GC.SuppressFinalize(this);
    }

    public string? World { get; set; }
    public string? User { get; set; }
    public string? Name { get; set; }
    public int? NameStyle { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Alignment { get; set; }
    public string? Status { get; set; }
    public bool Loading { get; set; } = true;
    public bool Invalid { get; set; }
    public bool Modified { get; set; }
  }
}
