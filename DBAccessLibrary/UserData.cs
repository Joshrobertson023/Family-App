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
    public class UserData
    {
        public List<UserModel> Users = new List<UserModel>();

        public UserModel? User;

        private string? connectionString;

        public UserData(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetConnectionString("Default");
        }

        private IConfiguration _config;

        public async Task GetAllUsers()
        {
            try
            {
                string query = $@"
                            SELECT * FROM USERS
                            ";

                OracleConnection conn = new OracleConnection(connectionString);
                conn.Open();

                OracleCommand cmd = new OracleCommand(query, conn);
                OracleDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    UserModel user = new UserModel
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        UserPassword = reader.GetString(reader.GetOrdinal("UserPassword")),
                        DateRegistered = reader.GetDateTime(reader.GetOrdinal("DateRegistered")),
                        LastSeen = reader.GetDateTime(reader.GetOrdinal("LastSeen"))
                    };

                    Users.Add(user);
                }

                conn.Close();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Function GetAllUsers failed: " + ex.Message);
            }
        }

        public async Task AddUserAsync(UserModel user)
        {
            string query = @"
                            INSERT INTO USERS (UserId, Username, UserPassword, DateRegistered, LastSeen)
                            VALUES (:UserId, :Username, :UserPassword, SYSDATE, SYSDATE)";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("UserId", user.UserId));
            cmd.Parameters.Add(new OracleParameter("Username", user.Username));
            cmd.Parameters.Add(new OracleParameter("UserPassword", user.UserPassword));

            await cmd.ExecuteNonQueryAsync();
        }

        // If returns 0, it was a fail
        public async Task<int> GetNextUserId()
        {
            int lastUserId = 0;

            string query = $@"
                        SELECT * FROM PRIMARYKEYS
                        ";

            OracleConnection conn = new OracleConnection(connectionString);
            conn.Open();

            OracleCommand cmd = new OracleCommand(query, conn);
            OracleDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lastUserId = reader.GetInt32(reader.GetOrdinal("LastUserId"));
                lastUserId += 1;
            }

            conn.Close();
            conn.Dispose();
            await IncrementUserIdInDB(lastUserId);
            return lastUserId;
        }

        public async Task IncrementUserIdInDB(int nextId)
        {
            string query = "UPDATE PRIMARYKEYS SET LastUserId = :newId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand updateCmd = new OracleCommand(query, conn);
            updateCmd.Parameters.Add(new OracleParameter("newId", nextId));
            await updateCmd.ExecuteNonQueryAsync();
        }

        public async Task SetUserActive()
        {
            if (User == null)
                return;

            string query = "UPDATE USERS SET LASTSEEN = SYSDATE where UserId = :UserId";

            using OracleConnection conn = new OracleConnection(connectionString);
            await conn.OpenAsync();

            using OracleCommand updateCmd = new OracleCommand(query, conn);
            updateCmd.Parameters.Add(new OracleParameter("UserId", User.UserId));
            await updateCmd.ExecuteNonQueryAsync();
        }
    }
}
