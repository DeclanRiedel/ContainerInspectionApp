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
                    INSERT INTO containers (container_id, container_type, extra_info, is_damaged, time_added, manufacturer, manufacture_date, weight) 
                    VALUES ($1, $2, $3, $4, $5, $6, $7, $8)");
                cmd.Parameters.AddWithValue(container.ContainerId);
                cmd.Parameters.AddWithValue(container.ContainerType);
                cmd.Parameters.AddWithValue(container.ExtraInfo);
                cmd.Parameters.AddWithValue(container.IsDamaged);
                cmd.Parameters.AddWithValue(container.TimeAdded);
                cmd.Parameters.AddWithValue(container.Manufacturer);
                cmd.Parameters.AddWithValue(container.ManufactureDate);
                cmd.Parameters.AddWithValue(container.Weight);
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") // Unique violation
            {
                return false;
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
                        container_id VARCHAR(50) NOT NULL UNIQUE,
                        container_type VARCHAR(50) NOT NULL,
                        extra_info TEXT,
                        is_damaged BOOLEAN NOT NULL,
                        time_added TIMESTAMP NOT NULL,
                        manufacturer VARCHAR(100),
                        manufacture_date DATE,
                        weight DECIMAL(10, 2)
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

            await using var cmd = _dataSource.CreateCommand("SELECT id, container_id, container_type, extra_info, is_damaged, time_added, manufacturer, manufacture_date, weight FROM containers ORDER BY time_added DESC");
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                containers.Add(new Container
                {
                    Id = reader.GetInt32(0),
                    ContainerId = reader.GetString(1),
                    ContainerType = reader.GetString(2),
                    ExtraInfo = reader.GetString(3),
                    IsDamaged = reader.GetBoolean(4),
                    TimeAdded = reader.GetDateTime(5),
                    Manufacturer = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    ManufactureDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                    Weight = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8)
                });
            }

            return containers;
        }

        public async Task<int> GetContainerCount()
        {
            try
            {
                await using var cmd = _dataSource.CreateCommand("SELECT COUNT(*) FROM containers");
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<Container?> GetLastAddedContainer()
        {
            try
            {
                await using var cmd = _dataSource.CreateCommand("SELECT * FROM containers ORDER BY time_added DESC LIMIT 1");
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Container
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        ContainerId = reader.GetString(reader.GetOrdinal("container_id")),
                        ContainerType = reader.GetString(reader.GetOrdinal("container_type")),
                        ExtraInfo = reader.GetString(reader.GetOrdinal("extra_info")),
                        IsDamaged = reader.GetBoolean(reader.GetOrdinal("is_damaged")),
                        TimeAdded = reader.GetDateTime(reader.GetOrdinal("time_added")),
                        Manufacturer = reader.IsDBNull(reader.GetOrdinal("manufacturer")) ? string.Empty : reader.GetString(reader.GetOrdinal("manufacturer")),
                        ManufactureDate = reader.IsDBNull(reader.GetOrdinal("manufacture_date")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("manufacture_date")),
                        Weight = reader.IsDBNull(reader.GetOrdinal("weight")) ? 0 : reader.GetDecimal(reader.GetOrdinal("weight"))
                    };
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}