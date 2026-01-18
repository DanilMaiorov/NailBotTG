using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NailBot.Core.DataAccess;
using NailBot.Core.Services;
using NailBot.Infrastructure.DataAccess;
using NailBot.Options;
using NailBot.TelegramBot;
using NailBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace NailBot.Extensions;

public static class CommonDiExtension
{
    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgreSqlConnectionString = configuration.GetConnectionString("PostgreSql")
                                         ?? throw new ArgumentException("ConnectionStrings:PostgreSql NotFound");
        
        services.AddSingleton<IDataContextFactory<ToDoDataContext>>(
            _ => new DataContextFactory(postgreSqlConnectionString));
        
        return services;
    } 
    public static IServiceCollection AddTelegramBot(this IServiceCollection services,
        IConfiguration configuration)
    {
        var botToken = configuration["Tokens:TelegramBotToken"]
                                         ?? throw new ArgumentException("TelegramBotToken NotFound");
        
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
        
        return services;
    } 
    
    public static IServiceCollection AddApplicationOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TaskOptions>(configuration.GetSection("TaskOptions"));
        services.Configure<FolderOptions>(configuration.GetSection("FolderOptions"));
        services.Configure<PaginationOptions>(configuration.GetSection("PaginationOptions"));
        return services;
    }
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, SqlUserRepository>();
        services.AddScoped<IToDoRepository, SqlToDoRepository>();
        services.AddScoped<IToDoListRepository, SqlToDoListRepository>();
        
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IToDoService, ToDoService>();
        services.AddScoped<IToDoReportService, ToDoReportService>();
        services.AddScoped<IToDoListService, ToDoListService>();
        return services;
    }
    
    public static IServiceCollection AddMessageServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageService, MessageService>();
        return services;
    }
    
    public static IServiceCollection AddScenarios(this IServiceCollection services)
    {
        services.AddTransient<IScenario, AddTaskScenario>();
        services.AddTransient<IScenario, DeleteTaskScenario>();
        services.AddTransient<IScenario, AddListScenario>();
        services.AddTransient<IScenario, DeleteListScenario>();
        return services;
    }
    
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddApplicationOptions(config)
            .AddDatabaseInfrastructure(config)
            .AddRepositories()
            .AddApplicationServices()
            .AddMessageServices()
            .AddScenarios()
            .AddScoped<IUpdateHandler, UpdateHandler>()
            .AddScoped<IScenarioContextRepository, InMemoryScenarioContextRepository>()
            .AddTelegramBot(config);
    }
}