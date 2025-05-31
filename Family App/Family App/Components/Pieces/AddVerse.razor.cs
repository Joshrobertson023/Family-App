using DBAccessLibrary;
using DBAccessLibrary.Models;
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
        private string message { get; set; } = "";
        private string readableReference { get; set; }
        public List<VerseModel> previewVerses;

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

        private IEnumerable<int> verses;

        private IEnumerable<int> Verses { get { return verses; } set { verses = value; VersesChanged(); } }

        private List<VerseModel> displayVerses { get; set; }

        private string category = "Uncategorized";
        private int numChapters;
        private int numVerses;
        private bool showNewCategoryComponent = false;
        private bool loading = false;
        private bool loadingVerses = true;
        [Parameter]
        public bool addingVerse { get; set; }
        [Parameter]
        public EventCallback<bool> AddingVerseChange { get; set; }
        [Parameter]
        public EventCallback OnVerseAdded { get; set; }

        private void OnBookChange()
        {
            chapter = 0;
            Verses = Array.Empty<int>();

            numChapters = BibleStructure.GetNumberChapters(book);
            numVerses = 0;
            errorMessage = "";
        }

        private void OnChapterChange()
        {
            Verses = Array.Empty<int>();

            numVerses = BibleStructure.GetNumberVerses(book, chapter);
            errorMessage = "";
        }

        private async Task SubmitVerse()
        {
            try
            {
                errorMessage = "";
                List<int> submitVerses = Verses.ToList();
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
            errorMessage = "";
        }

        private void SubmitCategory()
        {
            showNewCategoryComponent = !showNewCategoryComponent;
            category = newCategory;
            errorMessage = "";
        }

        private async Task VersesChanged()
        {
            message = "";
            List<int> verses = Verses.ToList<int>();
            if (verses == null || verses.Count <= 0)
                return;
            if (verses.Count >= 10)
            {
                // Show a ... after last 8 verses
                // Or just optimize this somehow
                return;
            }

            displayVerses = new List<VerseModel>();
            loadingVerses = true;
            errorMessage = "";

            try
            {
                //readableReference = ReferenceParse.ConvertToReferenceString(book, chapter, verses); // = ReferenceParse.ConvertToReadableReference(book, chapter, verses);
                foreach (var _verse in Verses)
                {
                    displayVerses.Add(await BibleAPI.GetAPIVerseAsync(ReferenceParse.ConvertToReferenceString(book, chapter, _verse), "kjv"));
                }
                displayVerses.Sort();
                loadingVerses = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("enumeration operation"))
                    errorMessage = ex.Message;
            }
        }
    }
}
