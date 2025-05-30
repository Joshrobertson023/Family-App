using DBAccessLibrary;
using DBAccessLibrary.Models;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pages
{
    public partial class MyVerses : ComponentBase
    {
        [Inject]
        NavigationManager nav { get; set; }
        [Inject]
        UserService userservice { get; set; }
        [Inject]
        VerseService verseservice { get; set; }
        public bool addingVerse { get; set; }
        private bool loading { get; set; }

        List<Verse> versesInCategory = new List<Verse>();

        private void CategorizeVerses()
        {

        }

        private void AddVerses()
        {
            addingVerse = true;
        }

        private async Task RefreshVerses()
        {
            addingVerse = false;
            loading = true;
            await verseservice.GetUserVersesDBAsync(userservice.currentUser.Id);
            await userservice.GetUserCategoriesDBAsync(userservice.currentUser.Id);
            loading = false;
        }
    }
}
