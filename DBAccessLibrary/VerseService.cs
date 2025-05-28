using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Web.Helpers;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DBAccessLibrary
{
    public class VerseService
    {
        private string connectionString;

        public Verse verse;
        public List<Verse> userVerses = new List<Verse>();
        public List<Verse> otherUserVerses = new List<Verse>();
        //public Dictionary<Verse, string> userVerses = new Dictionary<Verse, string>(); // Verse, category
        //public Dictionary<Verse, string> otherUserVerses = new Dictionary<Verse, string>(); // Verse, category

        public VerseService(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }

        private IConfiguration _config;


        /*
         *      ------------------------ Bible-API Method ------------------------
         */

        public async Task<Verse> GetAPIVerseAsync(string reference, string translation)
        {
            Verse returnVerse = new Verse();

            string baseUrl = "https://bible-api.com/";
            string url = $"{baseUrl}{reference}?translation={translation}";

            using HttpClient httpClient = new();

            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Critical error getting data from Bible API.");

            string json = await response.Content.ReadAsStringAsync();

            Root root = JsonSerializer.Deserialize<Root>(json);
            returnVerse.Reference = root.Reference;
            foreach (var verse in root.Verses)
            {
                JsonVerse jsonVerse = new JsonVerse();
                returnVerse.Text += verse.Text;
                returnVerse.Translation = verse.Translation;
            }

            return returnVerse;
        }


        /*
         *      ------------------------ Methods to get number of chapters and verses ------------------------
         */

        public int GetNumberChapters(string book)
        {
            int numChapters = 0;

            Dictionary<string, Dictionary<string, int>> numChaptersVerses;

            using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "kjv_book_chapter_verse_counts.json")))
            {
                string json = reader.ReadToEnd();
                numChaptersVerses = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
            }

            if (numChaptersVerses.ContainsKey(book))
            {
                numChapters = numChaptersVerses[book].Count;
            }

            return numChapters;
        }

        public int GetNumberVerses(string book, int chapter)
        {
            int numVerses = 0;

            Dictionary<string, Dictionary<string, int>> numChaptersVerses;

            using (StreamReader reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "kjv_book_chapter_verse_counts.json")))
            {
                string json = reader.ReadToEnd();
                numChaptersVerses = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
            }

            if (numChaptersVerses.ContainsKey(book))
            {
                if (numChaptersVerses[book].ContainsKey(chapter.ToString()))
                {
                    numVerses = numChaptersVerses[book][chapter.ToString()];
                }
            }

            return numVerses;
        }

        #region Json Classes
        public class Root
        {
            [JsonPropertyName("reference")]
            public string Reference { get; set; }

            [JsonPropertyName("verses")]
            public List<JsonVerse> Verses { get; set; }
        }

        public class JsonVerse
        {
            [JsonPropertyName("book_id")]
            public string BookId { get; set; }

            [JsonPropertyName("book_name")]
            public string BookName { get; set; }

            [JsonPropertyName("chapter")]
            public int Chapter { get; set; }

            [JsonPropertyName("verse")]
            public int VerseNumber { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("translation_id")]
            public string Translation { get; set; }

            [JsonPropertyName("translation_name")]
            public string TranslationName { get; set; }

            [JsonPropertyName("translation_note")]
            public string TranslationNote { get; set; }
        }
        #endregion


        /*
         *      ------------------------ Add a New Verse to the User ------------------------
         */

        public async Task AddNewUserVerseAsync(int userId,
            string reference,
            string category = "Uncategorized",
            string translation = "kjv", 
            float progressPercent = 0.0f,
            string lastPracticed = "",
            int timesReviewed = 0,
            int timesMemorized = 0,
            string dateMemorized = "")
        {
            Verse newVerse = new Verse(reference, translation);
            newVerse.Category = category;
            newVerse.ProgressPercent = progressPercent;
            newVerse.LastPracticed = null;
            newVerse.TimesReviewed = 0;
            newVerse.TimesMemorized = 0;
            newVerse.DateMemorized = null;
            await AddNewUserVerseDBAsync(userId, newVerse);
            await GetUserVersesDBAsync(userId);
        }


        /*
         *      ------------------------ User Verse DB Methods ------------------------
         */

        public async Task GetUserVersesDBAsync(int userId)
        {
            //userVerses = new Dictionary<Verse, string>();
            userVerses = new List<Verse>();

            string query = @"SELECT * FROM userverses 
                             WHERE USERID = :userId";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            try
            {
                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add(new OracleParameter("userId", userId));
                OracleDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Verse verse = new Verse();

                    verse.UserId = reader.GetInt32(reader.GetOrdinal("USERID"));
                    verse.VerseId = reader.GetInt32(reader.GetOrdinal("VERSEID"));
                    //string category = reader.GetString(reader.GetOrdinal("category"));
                    verse.Category = reader.GetString(reader.GetOrdinal("CATEGORY"));
                    verse.ProgressPercent = reader.GetFloat(reader.GetOrdinal("PROGRESSPERCENT"));
                    if (!reader.IsDBNull(reader.GetOrdinal("LASTPRACTICED")))
                    {
                        DateTime date = reader.GetDateTime(reader.GetOrdinal("LASTPRACTICED"));
                        verse.LastPracticed = date.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.LastPracticed = "";
                    }
                    // Displays Thursday, April 10, 2008 1:30:00 PM
                    verse.TimesReviewed = reader.GetInt32(reader.GetOrdinal("TIMESREVIEWED"));
                    verse.TimesMemorized = reader.GetInt32(reader.GetOrdinal("TIMESMEMORIZED"));
                    if (!reader.IsDBNull(reader.GetOrdinal("DATEMEMORIZED")))
                    {
                        DateTime date2 = reader.GetDateTime(reader.GetOrdinal("DATEMEMORIZED"));
                        verse.DateMemorized = date2.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.DateMemorized = "";
                    }
                    verse.Reference = reader.GetString(reader.GetOrdinal("REFERENCE"));
                    verse.Translation = reader.GetString(reader.GetOrdinal("TRANSLATION"));
                    Verse returnedVerse = await GetAPIVerseAsync(verse.Reference, verse.Translation);
                    verse.Text = returnedVerse.Text;

                    userVerses.Add(verse);
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                return;
            }

            conn.Close();
            conn.Dispose();
        }

        public async Task GetOtherUserVersesDBAsync(int userId)
        {
            otherUserVerses = new List<Verse>();

            string query = @"SELECT * FROM USERVERSES
                                WHERE USERID = :userId";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            try
            {

                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add(new OracleParameter("userId", userId));
                OracleDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Verse verse = new Verse();

                    verse.UserId = reader.GetInt32(reader.GetOrdinal("USERID"));
                    verse.VerseId = reader.GetInt32(reader.GetOrdinal("VERSEID"));
                    //string category = reader.GetString(reader.GetOrdinal("category"));
                    verse.Category = reader.GetString(reader.GetOrdinal("CATEGORY"));
                    verse.ProgressPercent = reader.GetFloat(reader.GetOrdinal("PROGRESSPERCENT"));
                    if (!reader.IsDBNull(reader.GetOrdinal("LASTPRACTICED")))
                    {
                        DateTime date = reader.GetDateTime(reader.GetOrdinal("LASTPRACTICED"));
                        verse.LastPracticed = date.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.LastPracticed = "";
                    }
                    // Displays Thursday, April 10, 2008 1:30:00 PM
                    verse.TimesReviewed = reader.GetInt32(reader.GetOrdinal("TIMESREVIEWED"));
                    verse.TimesMemorized = reader.GetInt32(reader.GetOrdinal("TIMESMEMORIZED"));
                    if (!reader.IsDBNull(reader.GetOrdinal("DATEMEMORIZED")))
                    {
                        DateTime date2 = reader.GetDateTime(reader.GetOrdinal("DATEMEMORIZED"));
                        verse.DateMemorized = date2.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.DateMemorized = "";
                    }
                    verse.Reference = reader.GetString(reader.GetOrdinal("REFERENCE"));
                    verse.Translation = reader.GetString(reader.GetOrdinal("TRANSLATION"));
                    Verse returnedVerse = await GetAPIVerseAsync(verse.Reference, verse.Translation);
                    verse.Text = returnedVerse.Text;

                    otherUserVerses.Add(verse);
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                return;
            }

            conn.Close();
            conn.Dispose();
        }

        public async Task AddNewUserVerseDBAsync(int userId, Verse verse)
        {
            if (verse == null)
                throw new ArgumentException("Fatal error adding verse to database: Verse was null.");

            string query = @"
                            INSERT INTO USERVERSES 
                            (userId, reference, category, progresspercent, lastpracticed,
                             timesreviewed, timesmemorized, datememorized, translation)
                            VALUES 
                            (:userId, :reference, :category, :progressPercent, NULL,
                             :timesreviewed, :timesmemorized, NULL, :translation)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("userId", userId));
            cmd.Parameters.Add(new OracleParameter("reference", verse.Reference));
            cmd.Parameters.Add(new OracleParameter("category", verse.Category));
            cmd.Parameters.Add(new OracleParameter("progressPercent", verse.ProgressPercent));
            cmd.Parameters.Add(new OracleParameter("timesreviewed", verse.TimesReviewed));
            cmd.Parameters.Add(new OracleParameter("timesmemorized", verse.TimesMemorized));
            cmd.Parameters.Add(new OracleParameter("translation", verse.Translation));

            await cmd.ExecuteNonQueryAsync();
        }


        /*
         *      ------------------------ Delete Verse DB Method ------------------------
         */

        public async Task DeleteUserVerseDBAsync(int userId, Verse verse)
        {
            string query = "DELETE FROM USERVERSES WHERE USERID = :userId AND REFERENCE = :reference";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            cmd.Parameters.Add(new OracleParameter("reference", verse.Reference));
            await cmd.ExecuteNonQueryAsync();
        }


        /*
         *      ------------------------ Verse DB Field Update Method ------------------------
         */

        public async Task UpdateVerseColumnDBAsync(int userId, Verse verse, string column, object value)
        {
            if (value == null)
                throw new ArgumentException($"Fatal error updating {column}. Value was null.");

            List<string> validColumnNames = new List<string>
            {
                "CATEGORY", "PROGRESSPERCENT", "LASTPRACTICED", "TIMESREVIEWED", "TIMESMEMORIZED", "DATEMEMORIZED", "REFERENCE", "TRANSLATION"
            };

            if (!validColumnNames.Contains(column))
                throw new ArgumentException($"Fatal error updating {column}. {column} does not exist in the database.");

            string query = @$"UPDATE USERVERSES
                             SET {column} = :value
                             WHERE USERID = :userId AND VERSEID : :verseId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("value", value));
            await cmd.ExecuteNonQueryAsync();
        }

        public void LogoutUser()
        {
            userVerses = new List<Verse>();
        }
    }
}
