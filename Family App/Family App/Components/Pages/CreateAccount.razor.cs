using DBAccessLibrary.Models;
using DBAccessLibrary;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pages;
public partial class CreateAccount : ComponentBase
{
    [Inject]
    NavigationManager nav { get; set; }
    [Inject]
    UserService userservice { get; set; }
    [Inject]
    VerseService verseservice { get; set; }
    private string? errorMessage;
    private bool loaded = false;
    private string? message;
    private bool loading { get; set; }

    private bool enteringInfo { get; set; } = true;
    private string username { get; set; }
    private string password { get; set; }
    private string repeatPassword { get; set; }
    private string firstName { get; set; }
    private string lastName { get; set; }
    private string email { get; set; }
    private bool help { get; set; } = false;
    private bool passwordHelp { get; set; } = false;
    private bool isKidsAccount { get; set; } = false;
    private int retryCount { get; set; } = 0;
    Random rand = new Random();
    private bool kidInfo { get; set; } = false;

    private void ToggleHelp()
    {
        help = !help;
    }

    private void TogglePasswordHelp()
    {
        passwordHelp = !passwordHelp;
    }

    private void ToggleKidInfo()
    {
        kidInfo = !kidInfo;
    }

    private async Task PrivacyPolicy()
    {
        if (help) help = !help;
        if (passwordHelp) passwordHelp = !passwordHelp;
        //await JSRuntime.InvokeAsync<object>("open", "privacypolicy", "_blank");
    }

    private void GenerateUsername()
    {
        if (string.IsNullOrEmpty(username))
            username = firstName + lastName + rand.Next(100);
    }

    private void ClearEmail()
    {
        email = "";
    }

    private void Reload()
    {
        nav.NavigateTo("/createaccount", forceLoad: true);
    }

    private async Task Next()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Please enter a username.");

            loading = true;
            message = "";
            errorMessage = "";

            await userservice.GetAllUsernamesDBAsync();

            foreach (var _username in userservice.usernames)
            {
                if (username.Trim() == _username.Trim())
                {
                    enteringInfo = true;
                    message = "Username already exists.";
                    loading = false;
                    return;
                }
            }

            message = "";
            enteringInfo = false;
            loading = false;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            if (errorMessage.ToLower().Contains("timed out"))
            {
                errorMessage += "\n (Retry count: " + retryCount + ") Retrying...";
                retryCount++;
                await Next();
            }
        }
    }

    private void Back()
    {
        enteringInfo = true;
    }

    private async Task Register()
    {
        try
        {
            if (String.IsNullOrWhiteSpace(password) || String.IsNullOrWhiteSpace(repeatPassword))
                throw new Exception("Please enter both passwords.");

            if (password != repeatPassword)
                throw new Exception("Passwords do not match.");

            if (password.Length < 12)
                throw new Exception("Password is too short. Please use a password that is at least 12 characters long.");

            message = "";
            loading = true;

            int kidsAccount = isKidsAccount == true ? 1 : 0;
            string? hashedEmail = "EMPTY";
            if (!string.IsNullOrEmpty(email))
                hashedEmail = PasswordHash.CreateHash(email.Trim());
            string hashedPassword = PasswordHash.CreateHash(password.Trim());
            await userservice.AddUserDBAsync(new UserModel(username, firstName, lastName, hashedEmail, hashedPassword, kidsAccount));

            loading = false;
            //SetCookies();

            nav.NavigateTo("/", forceLoad: true);
        }
        catch (Exception ex)
        {
            message = ex.Message;
            loading = false;
        }
    }

    // private async Task SetCookies()
    // {
    //     if (userservice.user.Username != null)
    //         await localStorage.SetItemAsync("cookieName", userservice.user.Username.Trim());
    // }
}
