/*1) ToDoUsers */
CREATE TABLE "ToDoUsers"(
	"UserId" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "RegisteredAt" TIMESTAMP NOT NULL
);	

/*2) ToDoLists */
CREATE TABLE "ToDoLists"(
	"Id" UUID PRIMARY KEY,
	"Name" VARCHAR(255) NOT NULL,
	"UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
	FOREIGN KEY ("UserId") REFERENCES "ToDoUsers"("UserId")
);

/*3) ToDoItems */
CREATE TABLE "ToDoItems"(
	"Id" UUID PRIMARY KEY,
	"UserId" UUID NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
	"ToDoItemState" INT NOT NULL,
    "StateChangedAt" TIMESTAMP,
	"Deadline" TIMESTAMP NOT NULL,
	"ToDoListId" UUID NOT NULL,
	FOREIGN KEY ("UserId") REFERENCES "ToDoUsers"("UserId"),
	FOREIGN KEY ("ToDoListId") REFERENCES "ToDoLists"("Id")
);

/*4) Indexes */
CREATE INDEX idx_todolist_userid ON "ToDoLists"("UserId");
CREATE INDEX idx_todoitem_userid ON "ToDoItems"("UserId");
CREATE INDEX idx_todoitem_todolistid ON "ToDoItems"("ToDoListId");
CREATE UNIQUE INDEX idx_todouser_telegramuserid ON "ToDoUsers"("TelegramUserId");


