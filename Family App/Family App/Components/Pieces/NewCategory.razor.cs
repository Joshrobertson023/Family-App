using DBAccessLibrary;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pieces
{
    public partial class NewCategory : ComponentBase
    {
        [Inject]
        NavigationManager nav { get; set; }
        [Inject]
        UserService userservice { get; set; }
        [Inject]
        VerseService verseservice { get; set; }
        private string category;
        [Parameter]
        public string newCategory { get; set; }
        [Parameter]
        public EventCallback OnCategoryChange { get; set; }

        private async Task AddCategory()
        {
            userservice.currentUserCategories.Add(category);
            await OnCategoryChange.InvokeAsync();
        }
    }
}
