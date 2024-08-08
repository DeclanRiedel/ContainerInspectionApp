using Microsoft.AspNetCore.Components;
using ContainerInspectionApp.Services;
using ContainerInspectionApp.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace ContainerInspectionApp.Components
{
    public class BasePage : ComponentBase
    {
        [Inject] protected IConfiguration Configuration { get; set; } = default!;
        [Inject] protected IValidator<string> ConnectionStringValidator { get; set; } = default!;
        [Inject] protected ContainerTableOperations ContainerTableOperations { get; set; } = default!;

        protected static bool _isDatabaseConnected = false;
        protected static string _connectionError = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (!_isDatabaseConnected)
            {
                var connectionString = Configuration.GetConnectionString("container-forms");
                if (connectionString != null)
                {
                    var validationResult = await ConnectionStringValidator.ValidateAsync(connectionString);
                    _isDatabaseConnected = validationResult.IsValid;
                    _connectionError = validationResult.IsValid ? string.Empty : validationResult.Errors.First().ErrorMessage;
                }
                else
                {
                    _isDatabaseConnected = false;
                    _connectionError = "Connection string is missing.";
                }
            }
            await base.OnInitializedAsync();
        }
    }
}