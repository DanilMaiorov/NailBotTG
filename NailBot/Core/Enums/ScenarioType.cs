namespace NailBot.Core.Enums
{
    public enum ScenarioType
    {
        None = 0,
        AddTask,
        AddList
    }
}



//Добавление и удаление списка

//Добавить класс AddListScenario, который реализует интерфейс IScenario и в конструкторе принимает IUserService и IToDoListService
//Добавить обработку шагов сценария (ScenarioContext.CurrentStep) через switch case
//case null
//Получить ToDoUser и сохранить его в ScenarioContext.Data.
//Отправить пользователю сообщение "Введите название списка:"
//Обновить ScenarioContext.CurrentStep на "Name"
//Вернуть ScenarioResult.Transition
//case "Name"
//Вызвать IToDoListService.Add. Передать ToDoUser из ScenarioContext.Data и name из сообщения
//Вернуть ScenarioResult.Completed
//При нажатии на кнопку "🆕Добавить" должен запускаться сценарий AddListScenario
//Добавить DeleteList в ScenarioType
//Добавить класс DeleteListScenario, который реализует интерфейс IScenario и в конструкторе принимает IUserService, IToDoListService и IToDoService
//Добавить обработку шагов сценария (ScenarioContext.CurrentStep) через switch case
//case null
//Получить ToDoUser и сохранить его в ScenarioContext.Data.
//Отправить пользователю сообщение "Выберете список для удаления:" с Inline кнопками. callbackData = ToDoListCallbackDto.ToString(). Action = "deletelist"
//Обновить ScenarioContext.CurrentStep на "Approve"
//case "Approve"
//Получить ToDoList и сохранить его в ScenarioContext.Data.
//Отправить пользователю сообщение "Подтверждаете удаление списка {toDoList.Name} и всех его задач" с Inline кнопками: WithCallbackData("✅Да", "yes"), WithCallbackData("❌Нет", "no")
//Обновить ScenarioContext.CurrentStep на "Delete"
//case "Delete"
//ЕСЛИ update.CallbackQuery.Data равна
//"yes" ТО удалить все задачи по ToDoUser и ToDoList. Удалить ToDoList.
//"no" ТО отправить сообщение "Удаление отменено".
//Вернуть ScenarioResult.Completed.
//При нажатии на кнопку "❌Удалить" должен запускаться сценарий DeleteListScenario