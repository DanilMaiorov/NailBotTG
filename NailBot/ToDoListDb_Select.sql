/*1) Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);*/

SELECT * FROM "ToDoItems" WHERE UserId = @UserId;

/*2) Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);*/

SELECT * FROM "ToDoItems" WHERE UserId = @UserId AND ToDoItemState = @ToDoItemState;

/*3) Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);*/

SELECT * FROM "ToDoItems"
WHERE UserId = @UserId
  AND ToDoListId = @ToDoListId;

SELECT * FROM "ToDoItems"
WHERE UserId = @UserId
  AND ToDoListId IS NULL;

/*4) Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);*/

SELECT EXISTS (
	SELECT 1 FROM "ToDoItems" WHERE UserId = @UserId AND Name LIKE @NamePrefix
);

/*5) Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct);*/

SELECT * FROM "ToDoItems"
WHERE UserId = @UserId AND Name LIKE @NamePrefix;

/*6) Task<int> CountActive(Guid userId, CancellationToken ct);*/

SELECT COUNT(*) FROM "ToDoItems" WHERE UserId = @UserId AND ToDoItemState = @ToDoItemState;

/*7) Task<ToDoItem?> Get(Guid id, CancellationToken ct);*/

SELECT * FROM "ToDoItems" WHERE Id = @ToDoItemId;

/*8) Task Add(ToDoItem item, CancellationToken ct);*/

INSERT INTO "ToDoItems"(Id, UserId, Name, CreatedAt, ToDoItemState, StateChangedAt, Deadline, ToDoListId)
VALUES (@Id, @UserId, @Name, @CreatedAt, @ToDoItemState, @StateChangedAt, @Deadline, @ToDoListId);

/*9) Task Delete(Guid id, CancellationToken ct);*/

DELETE FROM "ToDoItems" WHERE Id = @ToDoItemId;

/*10) Task Update(ToDoItem item, CancellationToken ct);*/

UPDATE "ToDoItems" SET ToDoItemState = @ToDoItemState WHERE Id = @ToDoItemId