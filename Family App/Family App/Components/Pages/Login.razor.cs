using DBAccessLibrary.Models;
using DBAccessLibrary;
using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using DBAccessLibrary.Models;

namespace FamilyApp.Components.Pages;

public partial class Login
{
    private string? errorMessage;
    private bool enteringName = true;
    private bool registering = false;
    private string? username;
    private string? password;
    private string? repeatPassword;
    private string? message;
    private string? cookieMessage;

    private UserService userservice;
    private NavigationManager nav;

    public Login(UserService userservice, NavigationManager nav)
    {
        this.userservice = userservice;
        this.nav = nav;
    }

    private async Task Next()
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            errorMessage = "Please enter your username.";
            errorMessage = null;
            return;
        }

        message = "Loading...";

        foreach (var user in userservice.users)
        {
            if (username.Trim() == user.Username.Trim())
            {
                userservice.user = user;
                enteringName = false;
                message = null;
                return;
            }
        }
        registering = true;
        enteringName = false;
        message = null;
    }

    private void Back()
    {
        enteringName = true;
        registering = false;
        message = null;
    }

    private async Task Signin()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Please enter your password.");

            if (Verify(password.Trim(), userdata.User.UserPassword))
            {
                SetCookies();

                await userdata.GetUserCategories();
                await verseservice.GetUserVerses(userdata.User.UserId);

                NavigationManager.NavigateTo("/home", forceLoad: true);
            }
            else
            {
                errorMessage = "Password is incorrect.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task Register()
    {
        try
        {
            if (String.IsNullOrWhiteSpace(password) || String.IsNullOrWhiteSpace(repeatPassword))
                throw new Exception("Please enter both passwords.");

            if (password != repeatPassword)
                throw new Exception("Passwords do not match.");

            userdata.User = new UserModel();
            userdata.User.UserId = await GenerateNextUserId();
            userdata.User.Username = username;
            userdata.User.UserPassword = CreateHash(password.Trim());

            await userdata.AddUserAsync(userdata.User);

            SetCookies();

            NavigationManager.NavigateTo("/home", forceLoad: true);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task SetCookies()
    {
        if (userdata.User.Username != null)
            await localStorage.SetItemAsync("cookieName", userdata.User.Username.Trim());
    }

    private async Task<int> GenerateNextUserId()
    {
        try
        {
            int nextId = await userdata.GetNextUserId();
            return nextId;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        return 0;
    }

    private string CreateHash(string password)
    {
        byte[] salt = new byte[16];
        new RNGCryptoServiceProvider().GetBytes(salt);

        const int iterations = 100000;
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
        byte[] hash = pbkdf2.GetBytes(20);

        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        string base64Hash = Convert.ToBase64String(hashBytes);
        return $"{base64Hash}";
    }


    public static bool Verify(string password, string hashedPassword)
    {
        int iterations = 100000;
        string base64Hash = hashedPassword;

        byte[] hashBytes = Convert.FromBase64String(base64Hash);

        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
        byte[] hash = pbkdf2.GetBytes(20);

        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false;
        }

        return true;
    }

}