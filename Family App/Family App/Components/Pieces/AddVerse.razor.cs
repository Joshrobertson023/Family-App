using DBAccessLibrary;
using Microsoft.AspNetCore.Components;

namespace VerseApp.Components.Pieces
{
    public partial class AddVerse : ComponentBase
    {
        [Inject]
        NavigationManager nav { get; set; }
        [Inject]
        UserService userservice { get; set; }
        [Inject]
        VerseService verseservice { get; set; }
        private string _book;
        private int _chapter;
        private int _verse;

        private string errorMessage { get; set; }
        private string newCategory { get; set; }

        private string book
        {
            get => _book;
            set
            {
                if (_book != value)
                {
                    _book = value;
                    OnBookChange();
                }
            }
        }

        private int chapter
        {
            get => _chapter;
            set
            {
                if (_chapter != value)
                {
                    _chapter = value;
                    OnChapterChange();
                }
            }
        }

        private int verse
        {
            get => _verse;
            set => _verse = value;
        }

        private IEnumerable<int> verses { get; set; } = Array.Empty<int>();
        private string category = "Uncategorized";
        private int numChapters;
        private int numVerses;
        private bool showNewCategoryComponent = false;
        private bool loading = false;
        [Parameter]
        public bool addingVerse { get; set; }
        [Parameter]
        public EventCallback<bool> AddingVerseChange { get; set; }
        [Parameter]
        public EventCallback OnVerseAdded { get; set; }

        private void OnBookChange()
        {
            chapter = 0;
            verses = Array.Empty<int>();

            numChapters = BibleStructure.GetNumberChapters(book);
            numVerses = 0;
        }

        private void OnChapterChange()
        {
            verses = Array.Empty<int>();

            numVerses = BibleStructure.GetNumberVerses(book, chapter);
        }

        private async Task SubmitVerse()
        {
            try
            {
                List<int> submitVerses = verses.ToList();
                submitVerses.Sort();
                loading = true;
                await verseservice.AddNewUserVerseAsync(userservice.currentUser.Id,
                                                        ReferenceParse.ConvertToReferenceString(book, chapter, submitVerses),
                                                        category);
                await userservice.SetUserActiveDBAsync(userservice.currentUser.Id);
                addingVerse = false;
                await OnVerseAdded.InvokeAsync();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                loading = false;
            }
        }

        private void ToggleNewCategory()
        {
            showNewCategoryComponent = !showNewCategoryComponent;
        }

        private void SubmitCategory()
        {
            showNewCategoryComponent = !showNewCategoryComponent;
            category = newCategory;
        }
    }
}
