using Azure;
using Azure.AI.DocumentIntelligence;
using TaxReturnAutomation.Infrastructure.Data;
using TaxReturnAutomation.Infrastructure.Data.Interceptors;
using TaxReturnAutomation.Infrastructure.Parsing;
using TaxReturnAutomation.Infrastructure.Persistence;
using TaxReturnAutomation.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {       
        var connectionString = builder.Configuration["ConnectionStrings:TaxReturnAutomationDb"];
        Guard.Against.Null(connectionString, message: "Connection string 'TaxReturnAutomationDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            //options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddSingleton(sp =>
        {
            var endpoint = builder.Configuration["AzureAI:DocumentIntelligence:Endpoint"];
            var key = builder.Configuration["AzureAI:DocumentIntelligence:Key"];

            Guard.Against.NullOrWhiteSpace(endpoint, message: "Azure Document Intelligence endpoint not found.");
            Guard.Against.NullOrWhiteSpace(key, message: "Azure Document Intelligence key not found.");

            return new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(key));
        });

        builder.Services.AddTransient<IFileStorageService, BlobStorageService>();
        builder.Services.AddScoped<IFileProcessingTracker, FileProcessingTracker>();

        builder.Services.AddTransient<IBankStatementParser, AzureFormRecognizerBankStatementParser>();
        builder.Services.AddScoped<IBankStatementRepository, BankStatementRepository>();

        builder.Services.AddScoped<IInvoiceParser, AzureFormRecongnizerInvoiceParser>();
        builder.Services.AddScoped<IInvoiceStorage, InvoiceRepository>();
    }
}
