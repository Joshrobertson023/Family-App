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

namespace DBAccessLibrary
{
    public class UserService
    {
        public List<UserModel> users = new List<UserModel>();
        public UserModel? user;
        public List<string> userCategories = new List<string>();
        public Dictionary<int, int> userFriends = new Dictionary<int, int>(); // friend id, type

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
            conn.Open();

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

        public async Task GetUserCategoriesDBAsync()
        {
            string query = @"SELECT * FROM USERCATEGORIES 
                             WHERE USERID = :userId";

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();

            OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", user.Id));
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string category = reader.GetString(reader.GetOrdinal("categoryname"));

                userCategories.Add(category);
            }

            conn.Close();
            conn.Dispose();
        }

        public async Task AddUserDBAsync(UserModel user)
        {
            string query = @"INSERT INTO USERS (USERID, USERNAME, USERPASSWORD, DATEREGISTERED, LASTSEEN)
                             VALUES (:userId, :username, :userPassword, SYSDATE, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("userId", user.Id));
            cmd.Parameters.Add(new OracleParameter("username", user.Username));
            cmd.Parameters.Add(new OracleParameter("userPassword", user.UserPassword));

            await cmd.ExecuteNonQueryAsync();
        }

        // If returns 0, it was a fail
        public async Task<int> GetNextUserIdDBAsync()
        {
            int lastUserId = 0;

            string query = @"SELECT * FROM PRIMARYKEYS";

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();

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

        public async Task SetUserActiveDBAsync()
        {
            if (user == null)
                throw new ArgumentException("Fatal error setting user as active. User was null.");

            string query = @"UPDATE USERS 
                             SET LASTSEEN = SYSDATE 
                             WHERE USERID = :userId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("userId", user.Id));
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
