using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Afterman.NSB.InstanceMapping.Repository
{
    using Concepts; 

    public class SqlHelper
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["NServiceBus/Persistence"]?.ConnectionString;

        public void Add(InstanceMapping instanceMapping)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"INSERT INTO InstanceMapping(EndpointName,TargetMachine, IsEnabled) 
                            VALUES(@param1,@param2, @param3)";

                    cmd.Parameters.AddWithValue("@param1", instanceMapping.EndpointName);
                    cmd.Parameters.AddWithValue("@param2", instanceMapping.TargetMachine);
                    cmd.Parameters.AddWithValue("@param3", instanceMapping.IsEnabled);
                    
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<InstanceMapping> GetAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM dbo.InstanceMapping", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new InstanceMapping
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        EndpointName = reader["EndpointName"].ToString(),
                        IsEnabled = Convert.ToBoolean(reader["IsEnabled"]),
                        TargetMachine = reader["TargetMachine"].ToString()
                    };
                }
            }
        }
    }
}
