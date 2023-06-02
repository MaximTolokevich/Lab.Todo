Feature: GetAllToDoItemStatuses

Scenario: Get Statuses
	Given a test user is successfully logged into an API
	| Login         | Password     |
	| test@user.com | Password@123 |
	When test user receive all ToDoItem statuses
	Then the response contains
	| StatusName |
	| Planned	 |
	| InProgress |
	| Paused	 |
	| Completed  |
	| Cancelled  |