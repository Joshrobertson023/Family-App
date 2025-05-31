using DBAccessLibrary.Models;
using DBAccessLibrary;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Mail;

namespace VerseApp.Components.Pages
{
    public partial class Log_in : ComponentBase
    {
        [Inject]
        NavigationManager nav { get; set; }
        [Inject]
        UserService userservice { get; set; }
        [Inject]
        VerseService verseservice { get; set; }
        private string? errorMessage;
        private bool enteringName = true;
        private bool loading = false;
        private string? username;
        private string? password;
        private string? message = "";
        private string? cookieMessage;
        UserModel userLoggingIn;
        private int retryCount { get; set; } = 0;
        private string passwordRecoverUsername { get; set; }

        private bool forgotUsername { get; set; }
        private bool forgotPassword { get; set; }
        private string recoveryEmail { get; set; }
        private string recoveryFirstName { get; set; }
        private string recoveryLastName { get; set; }
        private bool resetLinkSent { get; set; }
        private string emailPassword { get; set; }
        private bool forgotPasswordAndUsername { get; set; }
        private string firstName { get; set; }
        private string lastName { get; set; }

        private void ToggleForgotUsername()
        {
            forgotUsername = !forgotUsername;
        }

        private IConfiguration _config;
        public Log_in(IConfiguration config)
        {
            _config = config;
            emailPassword = _config.GetConnectionString("Email");
        }
        public Log_in() { }

        private void ToggleForgotPassword()
        {
            forgotPassword = !forgotPassword;
            errorMessage = "";
            message = "";
        }

        private void ToggleForgotPasswordUsername()
        {
            forgotPasswordAndUsername = true;
            forgotPassword = true;
            errorMessage = "";
            message = "";
        }

        private void forgotPasswordAndUsernameBack()
        {
            forgotPasswordAndUsername = false;
            forgotPassword = false;
            errorMessage = "";
            message = "";
        }

        private void Reload()
        {
            nav.NavigateTo("/login", forceLoad: true);
        }

        private void BackToLogin()
        {
            nav.NavigateTo("/login", forceLoad: true);
        }

        private async Task ResetPassword()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new Exception("Please enter a username.");
                if (string.IsNullOrWhiteSpace(recoveryEmail))
                    throw new Exception("Please enter an email.");

                loading = true;
                message = "";
                errorMessage = "";
                List<RecoveryInfo> recoveryInfo = await userservice.GetRecoveryInfoDBAsync();
                
                string hashedRecoveryEmail = PasswordHash.CreateHash(recoveryEmail.Trim());

                foreach (var _recoveryInfo in recoveryInfo)
                {
                    if (_recoveryInfo.Username == username && _recoveryInfo.Email == hashedRecoveryEmail)
                    {
                        await SendResetLink(_recoveryInfo);
                        loading = false;
                        return;
                    }
                }

                message = "Your username does not match the email on file.";
                enteringName = true;
                loading = false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        private async Task ResetPasswordFullName()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(firstName))
                    throw new Exception("Please enter your first name.");
                if (string.IsNullOrWhiteSpace(lastName))
                    throw new Exception("Please enter your last name.");
                if (string.IsNullOrWhiteSpace(recoveryEmail))
                    throw new Exception("Please enter an email.");

                loading = true;
                message = "";
                errorMessage = "";
                List<RecoveryInfo> recoveryInfo = await userservice.GetRecoveryInfoDBAsync();

                string recoveryFullName = firstName.Trim().ToLower() + lastName.Trim().ToLower();

                foreach (var _recoveryInfo in recoveryInfo)
                {
                    if (_recoveryInfo.FirstName + _recoveryInfo.LastName == recoveryFullName && PasswordHash.VerifyHash(recoveryEmail.Trim(), _recoveryInfo.Email))
                    {
                        await SendResetLink(_recoveryInfo);
                        loading = false;
                        return;
                    }
                }

                message = "That email does not match the one on file with your name.";
                enteringName = true;
                loading = false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        private async Task SendResetLink(RecoveryInfo userRecovering)
        {
            try
            {
                var email = new MailAddress(recoveryEmail);
                string token = Guid.NewGuid().ToString();
                string resetUrl = $"https://localhost:7093/resetpassword?token={token}&userid={userRecovering.Id}";

                var sendingAddress = new MailAddress("therealjoshrobertson@gmail.com");
                string subject = "Reset Your Password";
                string body = $"Click the link below to reset your password:\n\n{resetUrl}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("therealjoshrobertson@gmail.com", "tofp kaki lkuv nffh")
                };

                using var message = new MailMessage(sendingAddress, email)
                {
                    Subject = subject,
                    Body = body
                };

                await smtp.SendMailAsync(message);
                await userservice.ResetUserPasswordDBAsync(userRecovering.Id, token);
                resetLinkSent = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
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
                        userLoggingIn = await userservice.GetUserDBAsync(username.Trim());
                        enteringName = false;
                        errorMessage = "";
                        loading = false;
                        return;
                    }
                }

                message = "Username does not exist.";
                enteringName = true;
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

        private async Task NextWithNames()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(recoveryFirstName))
                    throw new Exception("Please enter your first name.");
                if (string.IsNullOrWhiteSpace(recoveryLastName))
                    throw new Exception("Please enter your last name.");
                if (string.IsNullOrWhiteSpace(passwordRecoverUsername))
                    throw new Exception("Please enter your password.");

                loading = true;
                message = "";
                errorMessage = "";
                List<RecoveryInfo> recoveryInfo = await userservice.GetRecoveryInfoDBAsync();

                string recoveryFullName = recoveryFirstName.Trim().ToLower() + recoveryLastName.Trim().ToLower();
                string hashedRecoveryPassword = PasswordHash.CreateHash(passwordRecoverUsername.Trim());
                string debug = "";
                foreach (var _recoveryInfo in recoveryInfo)
                {
                    debug = "";
                    if (_recoveryInfo.FirstName + _recoveryInfo.LastName == recoveryFullName && PasswordHash.VerifyHash(passwordRecoverUsername.Trim(), _recoveryInfo.PasswordHash))
                    {
                        loading = false;
                        message = "Your username is " + _recoveryInfo.Username;
                        return;                        
                    }
                    debug += _recoveryInfo.FullName + " | " + recoveryFullName + " Password check: " + PasswordHash.VerifyHash(passwordRecoverUsername.Trim(), _recoveryInfo.PasswordHash).ToString();
                }

                message = "Your name or password is incorrect." + " _recoveryInfo.FullName: " + debug;
                enteringName = true;
                loading = false;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                if (errorMessage.ToLower().Contains("timed out"))
                {
                    errorMessage += "\n (Retry count: " + retryCount + ") Retrying...";
                    retryCount++;
                    await NextWithNames();
                }
            }
        }

        private void Back()
        {
            enteringName = true;
            errorMessage = "";
            message = "";
        }

        private async Task Signin()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new Exception("Please enter your password.");

                if (PasswordHash.VerifyHash(password.Trim(), userLoggingIn.PasswordHash))
                {
                    //SetCookies();
                    loading = true;
                    await userservice.GetUserCategoriesDBAsync(userLoggingIn.Id);
                    await verseservice.GetUserVersesDBAsync(userLoggingIn.Id);
                    await userservice.SetUserActiveDBAsync(userLoggingIn.Id);
                    userservice.currentUser = userLoggingIn;
                    loading = false;

                    nav.NavigateTo("/", forceLoad: true);
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
    }
}
