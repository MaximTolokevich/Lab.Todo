Feature: Get all ToDo items

Scenario: Get all ToDo items
	Given the next tasks in database
	| Title      | Description	       | AssignedTo		  |
	| SmokeTask1 | Description of task | user1@domain.com |
	| SmokeTask2 | Description of task | user2@domain.com |
	When user 'user1@domain.com' asks the service to return all tasks
	Then response contains the next tasks
	| Title      | Description         | AssignedTo       |
	| SmokeTask1 | Description of task | user1@domain.com |