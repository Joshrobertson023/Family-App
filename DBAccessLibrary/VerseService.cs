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

        public string[] books { get; } =
        {
            "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy",
            "Joshua", "Judges", "Ruth", "1 Samuel", "2 Samuel",
            "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra",
            "Nehemiah", "Esther", "Job", "Psalms", "Proverbs",
            "Ecclesiastes", "Song of Solomon", "Isaiah", "Jeremiah", "Lamentations",
            "Ezekiel", "Daniel", "Hosea", "Joel", "Amos",
            "Obadiah", "Jonah", "Micah", "Nahum", "Habakkuk",
            "Zephaniah", "Haggai", "Zechariah", "Malachi",
            "Matthew", "Mark", "Luke", "John", "Acts",
            "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians",
            "Philippians", "Colossians", "1 Thessalonians", "2 Thessalonians", "1 Timothy",
            "2 Timothy", "Titus", "Philemon", "Hebrews", "James",
            "1 Peter", "2 Peter", "1 John", "2 John", "3 John",
            "Jude", "Revelation"
        };

        private IConfiguration _config;
        public VerseService(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }

        public async Task AddNewUserVerseAsync(int userId,
            string reference,
            string category = "Uncategorized",
            int visibility = 1,
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
            newVerse.Visibility = visibility;
            await AddNewUserVerseDBAsync(userId, newVerse);
            await GetUserVersesDBAsync(userId);
        }


        /*
         *      ------------------------ User Verse DB Methods ------------------------
         */

        public async Task GetUserVersesDBAsync(int userId)
        {
            userVerses = new List<Verse>();

            string query = @"SELECT * FROM USER_VERSES 
                             WHERE USER_ID = :userId";

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

                    verse.Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID"));
                    verse.UserId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                    //string category = reader.GetString(reader.GetOrdinal("category"));
                    verse.Category = reader.GetString(reader.GetOrdinal("CATEGORY_NAME"));
                    verse.ProgressPercent = reader.GetFloat(reader.GetOrdinal("PROGRESS_PERCENT"));
                    if (!reader.IsDBNull(reader.GetOrdinal("LAST_PRACTICED")))
                    {
                        DateTime date = reader.GetDateTime(reader.GetOrdinal("LAST_PRACTICED"));
                        verse.LastPracticed = date.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.LastPracticed = "";
                    }
                    // Displays Thursday, April 10, 2008 1:30:00 PM
                    verse.TimesReviewed = reader.GetInt32(reader.GetOrdinal("TIMES_REVIEWED"));
                    verse.TimesMemorized = reader.GetInt32(reader.GetOrdinal("TIMES_MEMORIZED"));
                    if (!reader.IsDBNull(reader.GetOrdinal("DATE_MEMORIZED")))
                    {
                        DateTime date2 = reader.GetDateTime(reader.GetOrdinal("DATE_MEMORIZED"));
                        verse.DateMemorized = date2.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.DateMemorized = "";
                    }
                    verse.Reference = reader.GetString(reader.GetOrdinal("REFERENCE"));
                    verse.Translation = reader.GetString(reader.GetOrdinal("TRANSLATION"));
                    verse.Visibility = reader.GetInt32(reader.GetOrdinal("VISIBILITY"));
                    VerseModel returnedVerse = await BibleAPI.GetAPIVerseAsync(verse.Reference, verse.Translation);
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

            string query = @"SELECT * FROM USER_VERSES
                                WHERE USER_ID = :userId";

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

                    verse.Id = reader.GetInt32(reader.GetOrdinal("VERSE_ID"));
                    verse.UserId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                    //string category = reader.GetString(reader.GetOrdinal("category"));
                    verse.Category = reader.GetString(reader.GetOrdinal("CATEGORY_NAME"));
                    verse.ProgressPercent = reader.GetFloat(reader.GetOrdinal("PROGRESS_PERCENT"));
                    if (!reader.IsDBNull(reader.GetOrdinal("LAST_PRACTICED")))
                    {
                        DateTime date = reader.GetDateTime(reader.GetOrdinal("LAST_PRACTICED"));
                        verse.LastPracticed = date.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.LastPracticed = "";
                    }
                    // Displays Thursday, April 10, 2008 1:30:00 PM
                    verse.TimesReviewed = reader.GetInt32(reader.GetOrdinal("TIMES_REVIEWED"));
                    verse.TimesMemorized = reader.GetInt32(reader.GetOrdinal("TIMES_MEMORIZED"));
                    if (!reader.IsDBNull(reader.GetOrdinal("DATE_MEMORIZED")))
                    {
                        DateTime date2 = reader.GetDateTime(reader.GetOrdinal("DATE_MEMORIZED"));
                        verse.DateMemorized = date2.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    else
                    {
                        verse.DateMemorized = "";
                    }
                    verse.Reference = reader.GetString(reader.GetOrdinal("REFERENCE"));
                    verse.Translation = reader.GetString(reader.GetOrdinal("TRANSLATION"));
                    verse.Visibility = reader.GetInt32(reader.GetOrdinal("VISIBILITY"));
                    VerseModel returnedVerse = await BibleAPI.GetAPIVerseAsync(verse.Reference, verse.Translation);
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

        public async Task<(int, int)> GetVerseInteractionInfoDBAsync(string reference)
        {
            int usersSaved = 0;
            int usersHighlighted = 0;

            string query = @"SELECT USERS_SAVED_VERSE, USERS_HIGHLIGHTED_VERSE 
                                FROM VERSES
                                WHERE VERSE_REFERENCE = :reference";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("reference", reference));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usersSaved = reader.GetInt32(reader.GetOrdinal("USERS_SAVED_VERSE"));
                usersHighlighted = reader.GetInt32(reader.GetOrdinal("USERS_HIGHLIGHTED_VERSE"));
            }

            conn.Close();
            conn.Dispose();

            return (usersSaved, usersHighlighted);
        }

        public async Task AddNewUserVerseDBAsync(int userId, Verse verse)
        {
            string query = @"
                            INSERT INTO USER_VERSES 
                            (USER_ID, CATEGORY_NAME, REFERENCE, LAST_PRACTICED, DATE_MEMORIZED,
                             PROGRESS_PERCENT, TIMES_REVIEWED, TIMES_MEMORIZED, TRANSLATION, VISIBILITY)
                            VALUES 
                            (:userId, :categoryName, :reference, NULL, NULL,
                             :progressPercent, :timesReviewed, NULL, :translation, :visibility)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("userId", userId));
            cmd.Parameters.Add(new OracleParameter("reference", verse.Reference));
            cmd.Parameters.Add(new OracleParameter("categoryName", verse.Category));
            cmd.Parameters.Add(new OracleParameter("progressPercent", verse.ProgressPercent));
            cmd.Parameters.Add(new OracleParameter("timesReviewed", verse.TimesReviewed));
            cmd.Parameters.Add(new OracleParameter("timesMemorized", verse.TimesMemorized));
            cmd.Parameters.Add(new OracleParameter("translation", verse.Translation));
            cmd.Parameters.Add(new OracleParameter("visibility", verse.Visibility));

            await cmd.ExecuteNonQueryAsync();

            List<int> verses = new List<int>();
            
            //foreach (int verse in )
            // Increment usersSaved for the verse

            conn.Close();
            conn.Dispose();
        }

        public async Task AddAllVersesOfBibleDBAsync()
        {
            string query = @"
                            INSERT INTO VERSES 
                            (VERSE_REFERENCE, USERS_SAVED_VERSE, USERS_HIGHLIGHTED_VERSE)
                            VALUES 
                            (:reference, :saved, :highlighted)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            string reference = "";
            int saved = 0;
            int highlighted = 0;

            var referenceParameter = new OracleParameter("reference", reference);
            var savedParameter = new OracleParameter("saved", saved);
            var highlightedParameter = new OracleParameter("highlighted", highlighted);
            cmd.Parameters.Add(referenceParameter);
            cmd.Parameters.Add(savedParameter);
            cmd.Parameters.Add(highlightedParameter);

            for (int i = 0; i < books.Count(); i++)
            {
                for (int j = 0; j <= BibleStructure.GetNumberChapters(books[i]); j++)
                {
                    for (int k = 0; k < BibleStructure.GetNumberVerses(books[i], j); k++)
                    {
                        List<int> verse = new List<int>() { k+1 };
                        referenceParameter.Value = ReferenceParse.ConvertToReferenceString(books[i], j, verse);
                        savedParameter.Value = 0;
                        highlightedParameter.Value = 0;

                        //await cmd.ExecuteNonQueryAsync();
                    }
                }
            }

            conn.Close();
            conn.Dispose();
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
