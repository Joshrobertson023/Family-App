using DBAccessLibrary.Models;
using DBAccessLibrary;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace FamilyApp.Components.Pages;
public partial class CreateAccount
{
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
    private int retryCount { get; set; } = 0;
    Random rand = new Random();

    private void ToggleHelp()
    {
        help = !help;
    }

    private void TogglePasswordHelp()
    {
        passwordHelp = !passwordHelp;
    }

    private async Task PrivacyPolicy()
    {
        if (help) help = !help;
        if (passwordHelp) passwordHelp = !passwordHelp;
        await JSRuntime.InvokeAsync<object>("open", "privacypolicy", "_blank");
    }

    private void GenerateUsername()
    {
        if (string.IsNullOrEmpty(username))
            username = firstName + lastName + rand.Next(1000);
    }

    private void ClearEmail()
    {
        email = "";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            loaded = false;
            try
            {
                //await userservice.GetAllUsernamesDBAsync();
                loaded = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                loaded = true;
            }
        }
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

            message = "";
            loading = true;
            StateHasChanged();
            userservice.user = new UserModel();
            userservice.user.Username = username.Trim();
            userservice.user.FirstName = firstName.Trim().ToLower();
            userservice.user.LastName = lastName.Trim().ToLower();
            if (email != null)
                userservice.user.Email = email.Trim();
            userservice.user.PasswordHash = PasswordHash.CreateHash(password.Trim());

            await userservice.AddUserDBAsync(userservice.user);

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
