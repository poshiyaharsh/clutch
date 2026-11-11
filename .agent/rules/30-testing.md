# Testing rules
- Every service method with a rule gets a unit test. Every role boundary gets an authorization test.
- Run `dotnet build` and `dotnet test` before marking a task done.
- Test names: Method_Scenario_ExpectedResult.
- Follow docs/TEST_PLAN.md; add new cases there when you find edge cases.
