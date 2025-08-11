INSERT INTO 
	ToDoUser (UserId, TelegramUserId, TelegramUserName, RegisteredAt)
VALUES 
	('c320d317-0949-43c2-a42e-14f77c3a0728', 11111, 'testUser_1', NOW()),
	('a1b2c3d4-e5f6-7890-1234-567890abcdef', 22222, 'testUser_2', NOW() + INTERVAL '1 minute'),
	('d317c320-43c2-0949-14f7-a42e7c567890', 33333, 'testUser_3', NOW() + INTERVAL '2 minutes');

INSERT INTO 
	ToDoList (Id, Name, UserId, CreatedAt)
VALUES 
	('0949c320-43c2-c320-14f7-a42e7c567890', 'testToDoList_1', (SELECT UserId FROM ToDoUser WHERE UserId = 'c320d317-0949-43c2-a42e-14f77c3a0728'), NOW() + INTERVAL '5 minutes'),
	('14f7c320-d317-0949-a42e-14f77c567890', 'testToDoList_2', (SELECT UserId FROM ToDoUser WHERE UserId = 'a1b2c3d4-e5f6-7890-1234-567890abcdef'), NOW() + INTERVAL '10 minutes'),
	('c320d317-43c2-7890-0949-a42e7c5614f7', 'testToDoList_3', (SELECT UserId FROM ToDoUser WHERE UserId = 'd317c320-43c2-0949-14f7-a42e7c567890'), NOW() + INTERVAL '15 minutes');

INSERT INTO 
	ToDoItem (Id, UserId, Name, CreatedAt, ToDoItemState, StateChangedAt, Deadline, ToDoListId)
SELECT
	'7c5c3200-09c2-e494-14f7-a42e4379c000'::uuid, -- Id
	(SELECT UserId FROM ToDoUser WHERE UserId = 'c320d317-0949-43c2-a42e-14f77c3a0728'), -- UserId
	'testToDoItem_1', -- Name
	NOW(), -- CreatedAt
	0, -- ToDoItemState
	NOW() + INTERVAL '5 minutes', -- StateChangedAt
	NOW() + INTERVAL '2 weeks', -- Deadline
	(SELECT Id FROM ToDoList WHERE Id = '0949c320-43c2-c320-14f7-a42e7c567890') -- ToDoListId
UNION ALL
SELECT
	'7c5c3200-09c2-e494-14f7-a42e4379c000', -- Id 
	(SELECT UserId FROM ToDoUser WHERE UserId = 'a1b2c3d4-e5f6-7890-1234-567890abcdef'), -- UserId
	'testToDoItem_2', -- Name
	NOW(), -- CreatedAt
	0, -- ToDoItemState
	NOW() + INTERVAL '5 minutes', -- StateChangedAt
	NOW() + INTERVAL '2 weeks', -- Deadline
	(SELECT Id FROM ToDoList WHERE Id = '14f7c320-d317-0949-a42e-14f77c567890') -- UserId
UNION ALL
SELECT
	'6c2c3210-0994-e3e4-22f7-d32e494d79c4', -- Id 
	(SELECT UserId FROM ToDoUser WHERE UserId = 'd317c320-43c2-0949-14f7-a42e7c567890'), -- UserId 
	'testToDoItem_3', -- Name
	NOW(), -- CreatedAt
	0, -- ToDoItemState
	NOW() + INTERVAL '5 minutes', -- StateChangedAt
	NOW() + INTERVAL '2 weeks', -- Deadline
	(SELECT Id FROM ToDoList WHERE Id = 'c320d317-43c2-7890-0949-a42e7c5614f7'); -- UserId

