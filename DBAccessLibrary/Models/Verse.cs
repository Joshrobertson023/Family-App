using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class Verse
    {
        public int Id { get; set; }
        public string? Json { get; set; }
        public string Reference { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public string Category { get; set; }
        public int? UserId { get; set; }
        public int? VerseId { get; set; }
        public float? ProgressPercent { get; set; }
        public string? LastPracticed { get; set; }
        public int? TimesReviewed { get; set; }
        public int? TimesMemorized { get; set; }
        public string? DateMemorized { get; set; }

        public Verse() { }

        public Verse(int id, string reference, string text, string translation, string category)
        {
            this.Id = id;
            this.Reference = reference;
            this.Text = text;
            this.Translation = translation;
            this.Category = category;
        }
    }
}
