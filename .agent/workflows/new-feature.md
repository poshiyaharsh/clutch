# /new-feature <name>
1. Read ROADMAP.md and confirm the feature belongs to the current phase.
2. Write a short plan: entities touched, services, routes, views, tests.
3. Update docs/DB_SCHEMA.md and docs/ARCHITECTURE.md first if the plan changes them.
4. Implement: entity/migration -> service -> controller -> ViewModel -> view.
5. Add tests from docs/TEST_PLAN.md.
6. Run build and tests.
7. Run /end-session.
