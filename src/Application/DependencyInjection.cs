﻿using System.Reflection;
using TaxReturnAutomation.Application.Common.Behaviours;
using Microsoft.Extensions.Hosting;
using TaxReturnAutomation.Application.Common.UseCases.BankStatements;
using TaxReturnAutomation.Application.Common.UseCases.Invoice;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        builder.Services.AddScoped<IBankStatementProcessor, BankStatementProcessor>();
        builder.Services.AddScoped<IInvoiceProcessor, InvoiceProcessor>();
    }
}
