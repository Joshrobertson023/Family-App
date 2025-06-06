﻿using DBAccessLibrary;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pages
{
    public partial class FriendsList : ComponentBase
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

        private void GoBack()
        {
            nav.NavigateTo("/profile");
        }
    }
}
