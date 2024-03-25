using FlexMatrix.Api.Data.DataBase;
using FlexMatrix.Api.Data.Parser;
using FlexMatrix.Api.Data.Parser.Strategies;
using FlexMatrix.Api.Data.Repositories.CrudRepository;
using FlexMatrix.Api.Data.Repositories.StructureRepository;
using FlexMatrix.Api.Data.Services.CrudService;
using FlexMatrix.Api.Data.Services.StructureService;
using Microsoft.Extensions.DependencyInjection;

namespace FlexMatrix.Api.Configuration;

public static class DependencyInjection
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        // Add database
        builder.Services.AddScoped<IUnitOfWork>(serviceProvider => 
            new UnitOfWork(serviceProvider.GetRequiredService<ILogger<UnitOfWork>>(),
                serviceProvider.GetRequiredService<IParser>(),
                builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<ICrudRepository, CrudRepository>();
        builder.Services.AddScoped<IStructureRepository, StructureRepository>();
        builder.Services.AddScoped<IStructureService, StructureService>();
        builder.Services.AddScoped<ICrudService, CrudService>();

        builder.Services.AddScoped<IParser, Parser>();
        builder.Services.AddScoped<IParserStrategy, ToDbParser>();
        builder.Services.AddScoped<IParserStrategy, FromDbParser>();
    }
}
