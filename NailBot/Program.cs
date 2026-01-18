using NailBot.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NailBot.Core.Services;
using NailBot.Extensions;

namespace NailBot
{
    public enum ToDoItemState { Active, Completed };
    
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    
                    config
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication(context.Configuration);
                })
                .Build();
            
            var botClient = host.Services.GetRequiredService<ITelegramBotClient>();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = []
            };

            using var cts = new CancellationTokenSource();

            await using var scope = host.Services.CreateAsyncScope();
            var scopedProvider = scope.ServiceProvider;

            var updateHandler = scopedProvider.GetRequiredService<IUpdateHandler>();

            if (updateHandler is UpdateHandler castHandler)
            {
                try
                {
                    botClient.StartReceiving(
                        updateHandler,
                        receiverOptions: receiverOptions,
                        cancellationToken: cts.Token
                    );
                    
                    while (true)
                    {
                        Console.WriteLine("Нажми A и Ввод для остановки и выхода из бота");
                        var s = Console.ReadLine();

                        if (s?.ToUpper() == "A")
                        {
                            cts.Cancel();
                            Console.WriteLine("Бот остановлен");
                            break;
                        }
                        var me = await botClient.GetMe();
                        Console.WriteLine($"{me.FirstName} запущен!");
                    }
                }
                finally
                {
                    castHandler.OnHandleUpdateStarted -= castHandler.HandleStart;
                    castHandler.OnHandleUpdateCompleted -= castHandler.HandleComplete;
                }
            }
        }
    }
}
