using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Logging;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;

namespace RPBlurb
{
  public enum FirebaseAuthState
  {
    LoggedOut,
    LoggingIn,
    LoggedIn,
    Error
  }

  public class FirebaseAuthService : IDisposable
  {
    private readonly FirebaseAuthClient authClient;
    private readonly RPBlurbPlugin plugin;

    private UserCredential? userCredential = null;

    public FirebaseAuthState State { get; private set; } = FirebaseAuthState.LoggedOut;
    public AuthErrorReason? ErrorReason { get; private set; }

    public FirebaseAuthService(RPBlurbPlugin plugin)
    {
      this.plugin = plugin;
      authClient = new FirebaseAuthClient(new FirebaseAuthConfig()
      {
        ApiKey = "AIzaSyCZviXRGPHLDZP-nb-GawfVvmOOJxq0uk0",
        AuthDomain = "gwhet-box.firebaseapp.com",
        UserRepository = new FileUserRepository(Path.Join(plugin.PluginInterface.GetPluginConfigDirectory(), "rpblurb")),
        Providers = new FirebaseAuthProvider[]
        {
          new GoogleProvider(),
          new EmailProvider()
        }
      });

      authClient.AuthStateChanged += AuthClient_AuthStateChanged;
    }

    private void AuthClient_AuthStateChanged(object? sender, UserEventArgs e)
    {
      PluginLog.LogDebug($"AuthClient_AuthStateChanged: {e.User?.Uid}");
      if (e.User != null) {
        State = FirebaseAuthState.LoggedIn; 
      } else {
        State = FirebaseAuthState.LoggedOut;
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public async Task LoginWithEmailAndPassword(string email, string password)
    {
      ErrorReason = null;
      State = FirebaseAuthState.LoggingIn;
      try
      {
        userCredential = await authClient.SignInWithEmailAndPasswordAsync(email, password);
        if (userCredential != null)
        {
          PluginLog.LogDebug($"Logged in with Email and Password: {userCredential.User.Uid}");
          State = FirebaseAuthState.LoggedIn;
        }
        else
        {
          State = FirebaseAuthState.Error;
          ErrorReason = AuthErrorReason.Unknown;
        }
      }
      catch (FirebaseAuthHttpException ex)
      {
        PluginLog.LogError($"Error whilst logging in with Email and Password: {ex.Reason}");
        State = FirebaseAuthState.Error;
        ErrorReason = ex.Reason;
      }
    }

    public async Task Logout()
    {
      authClient.SignOut();
      State = FirebaseAuthState.LoggedOut;
      ErrorReason = null;
    }
  }
}
