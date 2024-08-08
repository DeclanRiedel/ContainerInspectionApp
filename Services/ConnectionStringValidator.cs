using FluentValidation;
using Npgsql;
using System;

namespace ContainerInspectionApp.Validators
{
    public class ConnectionStringValidator : AbstractValidator<string>
    {
        private static NpgsqlDataSource? _cachedDataSource;
        private static string? _cachedConnectionString;

        public ConnectionStringValidator()
        {
            RuleFor(connectionString => connectionString)
                .NotEmpty()
                .Must(BeAValidPostgreSqlConnection)
                .WithMessage("Invalid PostgreSQL connection string or unable to connect to the database.");
        }

        private bool BeAValidPostgreSqlConnection(string connectionString)
        {
            try
            {
                if (connectionString != _cachedConnectionString)
                {
                    _cachedDataSource?.Dispose();
                    _cachedDataSource = NpgsqlDataSource.Create(connectionString ?? string.Empty);
                    _cachedConnectionString = connectionString;
                }

                using var connection = _cachedDataSource.OpenConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}