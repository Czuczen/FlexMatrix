using FlexMatrix.Api.Data.Repositories;
using FlexMatrix.Api.Data.Services;

namespace FlexMatrix.Api.Configuration;

public static class DependencyInjection
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRepository, Repository>();
        builder.Services.AddScoped<IStructureService, StructureService>();
        
    }
}
