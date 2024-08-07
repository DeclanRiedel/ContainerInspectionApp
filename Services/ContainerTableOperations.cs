using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading.Tasks;

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
    }
}