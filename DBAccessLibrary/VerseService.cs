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
                    Verse returnedVerse = await BibleAPI.GetAPIVerseAsync(verse.Reference, verse.Translation);
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
                    Verse returnedVerse = await BibleAPI.GetAPIVerseAsync(verse.Reference, verse.Translation);
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
