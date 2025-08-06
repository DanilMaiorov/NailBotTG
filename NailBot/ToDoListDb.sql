CREATE TABLE ToDoUser(
	UserId UUID PRIMARY KEY,
    TelegramUserId BIGINT NOT NULL,
    TelegramUserName VARCHAR(255) NOT NULL,
    RegisteredAt TIMESTAMP NOT NULL
);	

CREATE TABLE ToDoList(
	Id UUID PRIMARY KEY,
	Name VARCHAR(255) NOT NULL,
	UserId UUID NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
	FOREIGN KEY (UserId) REFERENCES ToDoUser(UserId)
);
   
CREATE TABLE ToDoItem(
	Id UUID PRIMARY KEY,
	UserId UUID NOT NULL,
    Name VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
	ToDoItemState INT NOT NULL,
    StateChangedAt TIMESTAMP,
	Deadline TIMESTAMP NOT NULL,
	ToDoListId UUID NOT NULL,
	FOREIGN KEY (UserId) REFERENCES ToDoUser(UserId),
	FOREIGN KEY (ToDoListId) REFERENCES ToDoList(Id)
);

CREATE INDEX index_todolist_userid ON ToDoList(UserId);
CREATE INDEX index_todoitem_userid ON ToDoItem(UserId);
CREATE INDEX index_todoitem_userid ON ToDoItem(ToDoListId);
CREATE UNIQUE INDEX index_todouser_telegramuserid ON ToDoUser(TelegramUserId);
