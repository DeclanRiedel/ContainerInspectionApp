using FluentValidation;
using Npgsql;

namespace ContainerInspectionApp.Validators
{
    public class ConnectionStringValidator : AbstractValidator<string>
    {
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
                using var dataSource = NpgsqlDataSource.Create(connectionString);
                using var connection = dataSource.OpenConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
