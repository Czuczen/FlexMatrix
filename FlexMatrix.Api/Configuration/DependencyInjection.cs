using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Repositories;
using FlexMatrix.Api.Data.Services;

namespace FlexMatrix.Api.Configuration;

public static class DependencyInjection
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        // Add database
        builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
            new UnitOfWork(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IRepository, Repository>();
        builder.Services.AddScoped<IStructureService, StructureService>();
        
    }
}
