using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareKey.Database
{
    class Client
    {
        private string _file;
        public Client(string file)
        {
            _file = file;
        }

        IEnumerable<string> RunQuery(string query)
        {
            List<string> results  = new List<string>();
            using (var connection = new SqliteConnection($"Data Source ={_file}"))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetString(0));
                    }
                }
            }

            return results;
        }
    }
}
