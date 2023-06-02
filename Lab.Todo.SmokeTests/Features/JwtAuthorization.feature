Feature: JwtAuthorization

Scenario: JWT Authorization endpoint returns a new auth-token
	Given a user with name 'SmokeUser'
	When smoke user tries to receive a new auth jwt token
	Then server returns a 200 status and a new auth jwt token in response body
	
Scenario: JWT Authorization endpoint does not authenticate an invalid user
	Given an invalid user with name 'BadUser'
	When bad user tries to receive a new auth jwt token
	Then server returns a 401 status and no auth token in response body

Scenario: Server allows access to anonymous API enpoint
	Given a not defined auth token
	When I make a request to any anonymous endpoint
	Then I receive a success status

Scenario: Server allows access to API with valid JWT token
	Given a valid auth jwt token
	When I make a request to any endpoint with required authentication
	Then I receive a success status

Scenario: Server does not allows access to API with invalid #(e.g. wrong secret) JWT token
	Given an invalid auth jwt token
	When I make a request to any endpoint with required authentication
	Then I receive status 401

Scenario: Server does not allows access to API with broken JWT token #(e.g. token: blablabla)
	Given a broken auth jwt token
	When I make a request to any endpoint with required authentication
	Then I receive status 401

Scenario: Server does not allows access to API with expired JWT token
	Given an expired auth jwt token
	When I make a request to any endpoint with required authentication
	Then I receive status 401