using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading.Tasks;
using ContainerInspectionApp.Models;

namespace ContainerInspectionApp.Services
{
    public class ContainerTableOperations
    {
        private readonly IConfiguration _configuration;

        public ContainerTableOperations(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("container-forms");
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                var connectionString = GetConnectionString();
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task InsertData(string data)
        {
            var connectionString = GetConnectionString();

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            await using (var cmd = dataSource.CreateCommand("INSERT INTO data (some_field) VALUES ($1)"))
            {
                cmd.Parameters.AddWithValue(data);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<string[]> GetAllData()
        {
            var connectionString = GetConnectionString();

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            var data = new System.Collections.Generic.List<string>();

            await using (var cmd = dataSource.CreateCommand("SELECT some_field FROM data"))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    data.Add(reader.GetString(0));
                }
            }

            return data.ToArray();
        }

        public async Task<bool> InsertContainer(string containerId, string containerType, string contents, DateTime dateAdded)
        {
            var connectionString = GetConnectionString();

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            try
            {
                await using var cmd = dataSource.CreateCommand("INSERT INTO containers (container_id, container_type, contents, date_added) VALUES ($1, $2, $3, $4)");
                cmd.Parameters.AddWithValue(containerId);
                cmd.Parameters.AddWithValue(containerType);
                cmd.Parameters.AddWithValue(contents);
                cmd.Parameters.AddWithValue(dateAdded);
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetDatabase()
        {
            var connectionString = GetConnectionString();

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            try
            {
                await using var cmd = dataSource.CreateCommand(@"
                    DROP TABLE IF EXISTS containers;
                    CREATE TABLE containers (
                        id SERIAL PRIMARY KEY,
                        container_id VARCHAR(50) NOT NULL,
                        container_type VARCHAR(50) NOT NULL,
                        contents TEXT NOT NULL,
                        date_added DATE NOT NULL
                    );");
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Container>> GetAllContainers()
        {
            var connectionString = GetConnectionString();

            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            var containers = new List<Container>();

            await using var cmd = dataSource.CreateCommand("SELECT container_id, container_type, contents, date_added FROM containers ORDER BY date_added DESC");
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                containers.Add(new Container
                {
                    ContainerId = reader.GetString(0),
                    ContainerType = reader.GetString(1),
                    Contents = reader.GetString(2),
                    DateAdded = reader.GetDateTime(3)
                });
            }

            return containers;
        }
    }
}