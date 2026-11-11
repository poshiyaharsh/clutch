# /run-tests
1. `dotnet build -warnaserror`
2. `dotnet test --collect:"XPlat Code Coverage"`
3. Report: passed/failed counts, failing test names with cause, coverage on Services/.
4. Fix failures, re-run until green. Log the result in PROJECT_LOG.md.
