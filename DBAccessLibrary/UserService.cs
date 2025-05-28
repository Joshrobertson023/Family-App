using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace DBAccessLibrary
{
    public class UserService
    {
        public List<UserModel> users = new List<UserModel>();
        public UserModel? user;
        public List<string> userCategories = new List<string>();
        public List<string> usernames;
        public Dictionary<UserModel, int> userFriends = new Dictionary<UserModel, int>(); // friend, type
        public List<UserModel> otherUserFriends = new List<UserModel>();

        private string connectionString;

        public UserService(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }

        private IConfiguration _config;

        public async Task GetAllUsersDBAsync()
        {
            string query = @"SELECT * FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                UserModel user = new UserModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("USERID")),
                    Username = reader.GetString(reader.GetOrdinal("USERNAME")),
                    UserPassword = reader.GetString(reader.GetOrdinal("USERPASSWORD")),
                    DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATEREGISTERED")),
                    LastSeen = reader.GetDateTime(reader.GetOrdinal("LASTSEEN"))
                };

                users.Add(user);
            }

            conn.Close();
            conn.Dispose();
        }

        public async Task<UserModel> GetUserDBAsync(string username)
        {
            UserModel user = new UserModel();

            string query = @"SELECT * FROM USERS WHERE USERNAME = :username";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("username", username));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                user.Id = reader.GetInt32(reader.GetOrdinal("USERID"));
                user.Username = reader.GetString(reader.GetOrdinal("USERNAME"));
                user.FirstName = reader.GetString(reader.GetOrdinal("FIRSTNAME"));
                user.LastName = reader.GetString(reader.GetOrdinal("LASTNAME"));
                if (!reader.IsDBNull(reader.GetOrdinal("EMAIL")))
                {
                    user.Email = reader.GetString(reader.GetOrdinal("EMAIL"));
                }
                user.UserPassword = reader.GetString(reader.GetOrdinal("USERPASSWORD"));
                user.DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATEREGISTERED"));
                user.LastSeen = reader.GetDateTime(reader.GetOrdinal("LASTSEEN"));
            }

            conn.Close();
            conn.Dispose();

            return user;
        }

        public async Task GetUserFriendsDBAsync(int userId)
        {
            // Update this function to get friends of user who's id is passed as parameter ^^^

            userFriends = new Dictionary<UserModel, int>(); // friend, type

            string query = @"SELECT * FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                UserModel user = new UserModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("USERID")),
                    Username = reader.GetString(reader.GetOrdinal("USERNAME")),
                    UserPassword = reader.GetString(reader.GetOrdinal("USERPASSWORD")),
                    DateRegistered = reader.GetDateTime(reader.GetOrdinal("DATEREGISTERED")),
                    LastSeen = reader.GetDateTime(reader.GetOrdinal("LASTSEEN"))
                };

                userFriends.Add(user, 0);
            }
            conn.Close();
            conn.Dispose();
        }

        public async Task GetAllUsernamesDBAsync()
        {
            usernames = new List<string>();

            string query = @"SELECT USERNAME FROM USERS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string username = reader.GetString(reader.GetOrdinal("USERNAME"));

                usernames.Add(username);
            }

            conn.Close();
            conn.Dispose();
        }

        public async Task GetUserCategoriesDBAsync(int userId)
        {
            //userVerses = new Dictionary<Verse, string>();
            userCategories = new List<string>();

            string query = @"SELECT DISTINCT NVL(CATEGORY, 'Uncategorized') AS CATEGORY FROM userverses 
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
                    string category = reader.GetString(reader.GetOrdinal("CATEGORY"));

                    userCategories.Add(category);
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

        public async Task AddUserDBAsync(UserModel user)
        {
            string query = @"INSERT INTO USERS (USERNAME, FIRSTNAME, LASTNAME, EMAIL, USERPASSWORD, DATEREGISTERED, LASTSEEN)
                             VALUES (:username, :firstName, :lastName, :email, :userPassword, SYSDATE, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("username", user.Username));
            cmd.Parameters.Add(new OracleParameter("firstName", user.FirstName.ToLower()));
            cmd.Parameters.Add(new OracleParameter("lastName", user.LastName.ToLower()));
            cmd.Parameters.Add(new OracleParameter("email", user.Email));
            cmd.Parameters.Add(new OracleParameter("userPassword", user.UserPassword));

            await cmd.ExecuteNonQueryAsync();

            this.user = await GetUserDBAsync(user.Username);
        }

        // If returns 0, it was a fail
        public async Task<int> GetNextUserIdDBAsync()
        {
            int lastUserId = 0;

            string query = @"SELECT * FROM PRIMARYKEYS";

            OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lastUserId = reader.GetInt32(reader.GetOrdinal("LASTUSERID"));
                lastUserId += 1;
            }

            conn.Close();
            conn.Dispose();
            await IncrementUserIdDBAsync(lastUserId);
            return lastUserId;
        }

        public async Task IncrementUserIdDBAsync(int nextId)
        {
            string query = @"UPDATE PRIMARYKEYS
                             SET LASTUSERID = :newId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("newId", nextId));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SetUserActiveDBAsync(int userId)
        {
            if (userId == null)
                throw new ArgumentException("Fatal error setting user as active. User was null.");

            string query = @"UPDATE USERS 
                             SET LASTSEEN = SYSDATE 
                             WHERE USERID = :userId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", userId));
            await cmd.ExecuteNonQueryAsync();
        }

        public void LogoutUser()
        {
            this.user = null;
            this.userCategories = new List<string>();
            this.userFriends = new Dictionary<UserModel, int>();
        }
    }
}
