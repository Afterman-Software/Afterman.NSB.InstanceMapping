using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Afterman.NSB.InstanceMapping.Repository
{
    using Concepts; 

    public static class SqlHelper
    {
        private static readonly string CustomConnectionStringKey = ConfigurationManager.AppSettings["ConnectionStringKey"];
        private const string DefaultConnectionStringKey = "NServiceBus/Persistence";

        public static void CreateTableIfNotExists()
        {
            if (!InstanceMappingTableExists())
                CreateInstanceMappingTable();
        }

        private static string GetConnectionString()
        {
            return string.IsNullOrEmpty(CustomConnectionStringKey)
                ? ConfigurationManager.ConnectionStrings[DefaultConnectionStringKey]?.ConnectionString
                : ConfigurationManager.ConnectionStrings[CustomConnectionStringKey]?.ConnectionString;
        }

        private static bool InstanceMappingTableExists()
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command =
                    new SqlCommand(
                        "SELECT CASE WHEN EXISTS((SELECT * FROM information_schema.tables WHERE table_name = 'InstanceMapping')) THEN 1 ELSE 0 END",
                        connection);
                return Convert.ToBoolean(command.ExecuteScalar());
            }
        }

        private static void CreateInstanceMappingTable()
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"CREATE TABLE [dbo].[InstanceMapping](
                                    	[Id] [int] IDENTITY(1,1) NOT NULL,
                                    	[EndpointName] [nvarchar](255) NULL,
                                    	[TargetMachine] [nvarchar](255) NULL,
                                    	[IsEnabled] [bit] NOT NULL,
                                    PRIMARY KEY CLUSTERED 
                                    (
                                    	[Id] ASC
                                    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                    ) ON [PRIMARY]";
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Add(InstanceMapping instanceMapping)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"INSERT INTO [dbo].[InstanceMapping](EndpointName,TargetMachine, IsEnabled) 
                            VALUES(@param1,@param2, @param3)";

                cmd.Parameters.AddWithValue("@param1", instanceMapping.EndpointName);
                cmd.Parameters.AddWithValue("@param2", instanceMapping.TargetMachine);
                cmd.Parameters.AddWithValue("@param3", instanceMapping.IsEnabled);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<InstanceMapping> GetAll()
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM [dbo].[InstanceMapping]", connection);
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
