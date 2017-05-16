using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace local_matching_service.Helpers
{
    static class Database {
        public static IEnumerable<dynamic> getUsers() {
            var connectionString = @"Data Source=C:\Users\Administrator\Projects\local-matching-service\identities.db";
            using (var connection = new SqliteConnection(connectionString)) {
                connection.Open();
                var select = connection.CreateCommand();
                select.CommandText = "select * from identities";
                using (var reader = select.ExecuteReader()) {
                    while(reader.Read()) {
                        yield return new {
                            id = reader.GetInt32(0),
                            verifyHashedPid = reader.IsDBNull(1) ? null : reader.GetString(1),
                            firstname = reader.GetString(2),
                            lastname = reader.GetString(3),
                            dateOfBirth = reader.GetString(4),
                            nationalInsuranceNumber = reader.GetString(5)
                        };
                    }
                }
            }
        }

        public static void setHashedPid(int id, String hashedPid) {
            var connectionString = @"Data Source=C:\Users\Administrator\Projects\local-matching-service\identities.db";

            using (var connection = new SqliteConnection(connectionString)) {
                connection.Open();
                var update = connection.CreateCommand();
                update.CommandText = "update identities set verify_hashed_pid = $hashedPid where id = $id";
                update.Parameters.AddWithValue("$id", id);
                update.Parameters.AddWithValue("$hashedPid", hashedPid);
                update.ExecuteNonQuery();
            }
        }
    }
}
