using FluentValidation;
using LinkShorter.Application;
using LinkShorter.Application.Options;
using LinkShorter.Application.Services;
using LinkShorter.Core.Repositories;
using LinkShorter.Infrastructure;
using LinkShorter.Infrastructure.Repositories;
using LinkShorter.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) =>
        serviceCollection
            .Configure<ShortLinkOptions>(configuration.GetRequiredSection(nameof(ShortLinkOptions)))
            .Configure<BarcodeGenerationOptions>(configuration.GetRequiredSection(nameof(BarcodeGenerationOptions)))
            .Configure<BarcodeStorageOptions>(configuration.GetRequiredSection(nameof(BarcodeStorageOptions)))
            .AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>()
            .AddMediatR(typeof(ApplicationAssemblyMarker));

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) =>
        serviceCollection
            .AddDbContext<DatabaseContext>(
                options => options.UseNpgsql(configuration.GetConnectionString("PostgresDb")))
            .AddScoped<IShortLinkRepository, ShortLinkRepository>()
            .AddScoped<IBarcodeInfoRepository, BarcodeInfoRepository>()
            .AddScoped<IBarcodeGenerationService, BarcodeGenerationService>()
            .AddScoped<IBarcodeStorageService, BarcodeStorageService>();


}