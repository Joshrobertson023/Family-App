using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace DBAccessLibrary
{
    public class VerseService
    {
        private string connectionString;

        public Verse verse;
        public List<Verse> userVerses = new List<Verse>();
        public List<Verse> otherUserVerses = new List<Verse>();

        public VerseService(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }

        private IConfiguration _config;


        /*
         *      ------------------------ Bible-API Methods ------------------------
         */

        public async Task GetAPIVerseAsync(string book, string chapter, string verses, string translation="kjv")
        {
            Verse verse = new Verse();

            HttpClient httpClient = new();
            using HttpResponseMessage response 
                = await httpClient.GetAsync($"https://bible-api.com/{book} {chapter}{verses}?translation={translation}");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            verse.Json = jsonResponse;
        }

        public async Task<string> GetAPIVerseTextAsync(string reference, string translation)
        {
            Verse verse = new Verse();

            string text = "";

            List<string> components = ReferenceParse.ConvertToReferenceParts(reference);
            string book = components[0];
            string chapter = components[1];
            string verses = components[2];

            HttpClient httpClient = new();
            using HttpResponseMessage response
                = await httpClient.GetAsync($"https://bible-api.com/{book} {chapter}{verses}?translation={translation}");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            verse.Json = jsonResponse;

            return text;
        }


        /*
         *      ------------------------ Add a New Verse to the User ------------------------
         */

        public async Task AddNewUserVerseAsync(int userId,
            string reference, 
            string text, 
            string translation, 
            string category = "No Category",
            float progressPercent = 0.0f,
            string lastPracticed = "",
            int timesReviewed = 0,
            int timesMemorized = 0,
            string dateMemorized = "")
        {
            int nextVerseId = await GetNextVerseId();

            if (nextVerseId == 0)
                throw new Exception("Fatal error getting next verse ID.");

            Verse newVerse = new Verse(nextVerseId, reference, text, translation, category);
            userVerses.Add(newVerse);
            await AddNewUserVerseDBAsync(userId, newVerse);
        }


        /*
         *      ------------------------ User Verse DB Methods ------------------------
         */

        public async Task GetUserVersesDBAsync(int userId)
        {
            userVerses = new List<Verse>();

            string query = $@"
                        SELECT * FROM userverses where userid = :userid
                        ";

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userid", userId));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse();

                verse.UserId = reader.GetInt32(reader.GetOrdinal("verseid"));

                userVerses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
        }

        public async Task GetOtherUserVersesDBAsync(int userId)
        {
            otherUserVerses = new List<Verse>();

            string query = $@"
                        SELECT * FROM userverses where userid = :userid
                        ";

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userid", userId));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Verse verse = new Verse();

                verse.UserId = reader.GetInt32(reader.GetOrdinal("userid"));
                verse.VerseId = reader.GetInt32(reader.GetOrdinal("verseid"));
                verse.Category = reader.GetString(reader.GetOrdinal("category"));
                verse.ProgressPercent = reader.GetFloat(reader.GetOrdinal("progresspercent"));
                DateTime date = reader.GetDateTime(reader.GetOrdinal("lastpracticed"));
                verse.LastPracticed = date.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                // Displays Thursday, April 10, 2008 1:30:00 PM
                verse.TimesReviewed = reader.GetInt32(reader.GetOrdinal("timesreviewed"));
                verse.TimesMemorized = reader.GetInt32(reader.GetOrdinal("timesmemorized"));
                DateTime date2 = reader.GetDateTime(reader.GetOrdinal("datememorized"));
                verse.DateMemorized = date2.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                verse.Reference = reader.GetString(reader.GetOrdinal("reference"));
                verse.Translation = reader.GetString(reader.GetOrdinal("translation"));
                verse.Text = await GetAPIVerseTextAsync(verse.Reference, verse.Translation);

                otherUserVerses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
        }


        public async Task AddNewUserVerseDBAsync(int userId, Verse verse)
        {
            if (verse == null)
                throw new ArgumentException("Fatal error adding verse to database: Verse was null.");

            string query = @"
                            INSERT INTO userverses 
                            (userId, verseId, category, progresspercent, lastpracticed,
                             timesreviewed, timesmemorized, datememorized, reference, translation)
                            VALUES 
                            (:userId, :verseId, :category, :progressPercent, NULL,
                             :timesreviewed, :timesmemorized, NULL, :reference, :translation)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("userId", userId));
            cmd.Parameters.Add(new OracleParameter("verseId", verse.Id));
            cmd.Parameters.Add(new OracleParameter("category", verse.Category));
            cmd.Parameters.Add(new OracleParameter("progressPercent", verse.ProgressPercent));
            cmd.Parameters.Add(new OracleParameter("timesreviewed", verse.TimesReviewed));
            cmd.Parameters.Add(new OracleParameter("timesmemorized", verse.TimesMemorized));
            cmd.Parameters.Add(new OracleParameter("reference", verse.Reference));
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


        /*
         *      ------------------------ Verse Id DB methods ------------------------
         */

        // If returns 0, it was a fail
        public async Task<int> GetNextVerseId()
        {
            int lastVerseId = 0;

            string query = $@"
                        SELECT * FROM PRIMARYKEYS
                        ";

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lastVerseId = reader.GetInt32(reader.GetOrdinal("LastVerseId"));
                lastVerseId += 1;
            }

            conn.Close();
            conn.Dispose();
            await IncrementVerseIdDB(lastVerseId);
            return lastVerseId;
        }

        public async Task IncrementVerseIdDB(int nextId)
        {
            string query = "UPDATE PRIMARYKEYS SET LastVerseId = :newId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand updateCmd = new OracleCommand(query, conn);
            updateCmd.Parameters.Add(new OracleParameter("newId", nextId));
            await updateCmd.ExecuteNonQueryAsync();
        }
    }
}
