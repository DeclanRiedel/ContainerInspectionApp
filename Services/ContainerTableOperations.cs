using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContainerInspectionApp.Models;

namespace ContainerInspectionApp.Services
{
    public class ContainerTableOperations
    {
        private readonly IConfiguration _configuration;
        private readonly NpgsqlDataSource _dataSource;

        public ContainerTableOperations(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = GetConnectionString();
            _dataSource = NpgsqlDataSource.Create(connectionString);
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("container-forms") ?? throw new InvalidOperationException("Connection string 'container-forms' not found.");
        }

        public async Task<(bool Success, string ErrorMessage)> TestConnection()
        {
            try
            {
                await using var cmd = _dataSource.CreateCommand("SELECT 1");
                await cmd.ExecuteScalarAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
        }

        public async Task<bool> InsertContainer(Container container)
        {
            try
            {
                await using var cmd = _dataSource.CreateCommand(@"
                    INSERT INTO containers (container_id, container_type, contents, date_added, location, status) 
                    VALUES ($1, $2, $3, $4, $5, $6)");
                cmd.Parameters.AddWithValue(container.ContainerId);
                cmd.Parameters.AddWithValue(container.ContainerType);
                cmd.Parameters.AddWithValue(container.Contents);
                cmd.Parameters.AddWithValue(container.DateAdded ?? DateTime.Now);
                cmd.Parameters.AddWithValue(container.Location);
                cmd.Parameters.AddWithValue(container.Status);
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
            try
            {
                await using var cmd = _dataSource.CreateCommand(@"
                    DROP TABLE IF EXISTS containers;
                    CREATE TABLE containers (
                        id SERIAL PRIMARY KEY,
                        container_id VARCHAR(50) NOT NULL,
                        container_type VARCHAR(50) NOT NULL,
                        contents TEXT NOT NULL,
                        date_added DATE NOT NULL,
                        location VARCHAR(100) NOT NULL,
                        status VARCHAR(50) NOT NULL
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
            var containers = new List<Container>();

            await using var cmd = _dataSource.CreateCommand("SELECT id, container_id, container_type, contents, date_added, location, status FROM containers ORDER BY date_added DESC");
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                containers.Add(new Container
                {
                    Id = reader.GetInt32(0),
                    ContainerId = reader.GetString(1),
                    ContainerType = reader.GetString(2),
                    Contents = reader.GetString(3),
                    DateAdded = reader.GetDateTime(4),
                    Location = reader.GetString(5),
                    Status = reader.GetString(6)
                });
            }

            return containers;
        }
    }
}