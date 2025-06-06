﻿using DBAccessLibrary;
using DBAccessLibrary.Models;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pages
{
    public partial class ResetPassword : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery(Name = "token")]
        public string Token { get; set; }
        [Parameter]
        [SupplyParameterFromQuery(Name = "userid")]
        public string UserId { get; set; }
        private string? errorMessage;
        private bool loaded = false;
        private string? message;
        private bool validToken;
        private bool loading { get; set; }
        private string password { get; set; }
        private string repeatPassword { get; set; }
        private bool passwordIsReset { get; set; }

        protected override async Task OnInitializedAsync()
        {
            validToken = await userservice.VerifyTokenDBAsync(Convert.ToInt32(UserId), Token);
        }

        public async Task SubmitNewPassword()
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

                string hashedPassword = PasswordHash.CreateHash(password.Trim());
                await userservice.UpdateUserPasswordDBAsync(Convert.ToInt32(UserId), hashedPassword);

                loading = false;
                userservice.currentUser = null;
                passwordIsReset = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                loading = false;
            }
        }
    }
}
