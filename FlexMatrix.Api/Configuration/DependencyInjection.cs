using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Repositories;
using FlexMatrix.Api.Data.Repositories.StructureRepository;
using FlexMatrix.Api.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlexMatrix.Api.Configuration;

public static class DependencyInjection
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        // Add database
        builder.Services.AddScoped<IUnitOfWork>(serviceProvider => 
            new UnitOfWork(serviceProvider.GetRequiredService<ILogger<UnitOfWork>>(), 
            builder.Configuration.GetConnectionString("DefaultConnection")));

        
        builder.Services.AddScoped<IRepository, Repository>();
        builder.Services.AddScoped<IStructureRepository, StructureRepository>();
        builder.Services.AddScoped<IStructureService, StructureService>();
        
    }
}
