using DBAccessLibrary;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pages
{
    public partial class Profile : ComponentBase
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

        private void Reload()
        {
            nav.NavigateTo("/profile", forceLoad: true);
        }

        private void FriendsList()
        {
            nav.NavigateTo("/friendslist");
        }

        private void Login()
        {
            nav.NavigateTo("/login", forceLoad: true);
        }

        private void Logout()
        {
            userservice.currentUser = null;
            nav.NavigateTo("/profile", forceLoad: true);
        }
    }
}
