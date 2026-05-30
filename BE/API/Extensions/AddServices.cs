using Business.Domain.Repositories;
using Business.Domain.Services;
using Business.Mapping.Account;
using Business.Resources;
using Business.Services;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace API.Extensions;

public static class AddServices
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        RegisterRepositories(services);
        RegisterServices(services);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IImageService, ImageService>();

        services.AddAutoMapper(typeof(ModelToResourceProfile));
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IEducationRepository, EducationRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IWorkHistoryRepository, WorkHistoryRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryPersonRepository, CategoryPersonRepository>();
        services.AddScoped<ITechnologyRepository, TechnologyRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPayRepository, PayRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IWorkHistoryService, WorkHistoryService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICategoryPersonService, CategoryPersonService>();
        services.AddScoped<ITechnologyService, TechnologyService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITokenManagementService, TokenManagementService>();
        services.AddScoped<ITimesheetService, TimesheetService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IPayService, PayService>();
    }

    public static void AddCustomizeSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Human Resource Management for IT Company",
                Version = "v1.0"
            });

            options.OperationFilter<SwaggerFileOperationFilter>();

            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };

            options.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [jwtScheme] = Array.Empty<string>()
            });
        });
    }

    public static void AddCronJob<T>(
        this IServiceCollection services,
        Action<IScheduleConfig<T>> configure)
        where T : CronJobService
    {
        ArgumentNullException.ThrowIfNull(configure);

        var config = new ScheduleConfig<T>();
        configure(config);

        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentException(
                "Cron expression cannot be empty.",
                nameof(config.CronExpression));
        }

        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
    }
}
