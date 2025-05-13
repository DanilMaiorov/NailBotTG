using NailBot.Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot.TelegramBot
{
    public class Init
    {
        private readonly ITelegramBotClient _botClient;

        private readonly IUpdateHandler _updateHandler;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        private readonly IToDoReportService _toDoReportService;

        private readonly CancellationTokenSource _cts;

        // Предоставляем доступ к обработчику для подписки/отписки извне
        internal UpdateHandler BotUpdateHandler => (UpdateHandler)_updateHandler;

        public Init(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService, CancellationTokenSource cts)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;

            _toDoReportService = toDoReportService;
            _cts = cts;

            _updateHandler = new UpdateHandler(userService, toDoService, toDoReportService, cts);
        }

        public void Start()
        {
            if (_updateHandler is UpdateHandler castHandler)
            {
                //подписываюсь на события
                castHandler.OnHandleUpdateStarted += castHandler.HandleStart;
                castHandler.OnHandleUpdateCompleted += castHandler.HandleComplete;

                try
                {
                    _botClient.StartReceiving(_updateHandler, _cts.Token);
                }
                finally
                {
                    //отписываюсь от событий
                    castHandler.OnHandleUpdateStarted -= castHandler.HandleStart;
                    castHandler.OnHandleUpdateCompleted -= castHandler.HandleComplete;
                }
            }
        }

        public void Stop()
        {
            Console.WriteLine($"До свидания! До новых встреч!");
        }
    }
}
