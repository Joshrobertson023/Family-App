using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace DBAccessLibrary
{
    public class VerseService
    {
        private string connectionString;

        public Verse Verse;
        public List<Verse> UserVerses = new List<Verse>();
        public List<Verse> OtherUserVerses = new List<Verse>();

        public VerseService(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }

        private IConfiguration _config;

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

        public async Task<string> GetVerseText(string reference, string translation)
        {
            Verse = new Verse();

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
            Verse.json = jsonResponse;

            return text;
        }

        #region GetUserVerses()
        public async Task GetUserVerses(int userId)
        {
            UserVerses = new List<Verse>();

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

                verse.userId = reader.GetInt32(reader.GetOrdinal("verseid"));

                UserVerses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
        }
        #endregion

        #region GetOtherUserVerses()
        public async Task GetOtherUserVerses(int userId)
        {
            OtherUserVerses = new List<Verse>();

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

                verse.userId = reader.GetInt32(reader.GetOrdinal("userid"));
                verse.verseId = reader.GetInt32(reader.GetOrdinal("verseid"));
                verse.category = reader.GetString(reader.GetOrdinal("category"));
                verse.progressPercent = reader.GetFloat(reader.GetOrdinal("progresspercent"));
                DateTime date = reader.GetDateTime(reader.GetOrdinal("lastpracticed"));
                verse.lastPracticed = date.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                // Displays Thursday, April 10, 2008 1:30:00 PM
                verse.timesReviewed = reader.GetInt32(reader.GetOrdinal("timesreviewed"));
                verse.timesMemorized = reader.GetInt32(reader.GetOrdinal("timesmemorized"));
                DateTime date2 = reader.GetDateTime(reader.GetOrdinal("datememorized"));
                verse.dateMemorized = date2.ToString("U", CultureInfo.CreateSpecificCulture("en-US"));
                verse.reference = reader.GetString(reader.GetOrdinal("reference"));
                verse.translation = reader.GetString(reader.GetOrdinal("translation"));
                verse.text = await GetVerseText(verse.reference, verse.translation);

                OtherUserVerses.Add(verse);
            }

            conn.Close();
            conn.Dispose();
        }
        #endregion
    }

    public class Verse
    {
        public string? json { get; set; }
        public string reference { get; set; }
        public string text { get; set; }
        public string translation { get; set; }
        public int? userId { get; set; }
        public int? verseId { get; set; }
        public string? category { get; set; }
        public float? progressPercent { get; set; }
        public string? lastPracticed { get; set; }
        public int? timesReviewed { get; set; }
        public int? timesMemorized { get; set; }
        public string? dateMemorized { get; set; }
    }
}
