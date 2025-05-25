using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary
{
    public class VerseService
    {
        public Verse Verse;

        public async Task GetVerse(string book, string chapter, string verses, string translation="kjv")
        {
            Verse = new Verse();

            HttpClient httpClient = new();
            using HttpResponseMessage response 
                = await httpClient.GetAsync($"https://bible-api.com/{book} {chapter}{verses}?translation={translation}");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Verse.json = jsonResponse;
        }
    }

    public class Verse
    {
        public string json { get; set; }
        public string book { get; set; }
        public string chapter { get; set; }
        public string verses { get; set; }
        public string text { get; set; }
        public string translation { get; set; }
    }
}
