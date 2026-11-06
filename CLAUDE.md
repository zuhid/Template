# CLAUDE.md

ASP.NET Core (.NET 10) Web API template backed by PostgreSQL via EF Core (Npgsql), plus a Vue front end in `Web.Vue/`.

## Layout

- `Template/` — API project (`Zuhid.Template`)
  - `Entities/` — EF entities; all inherit `BaseEntity` (`Id`, `UpdatedById`, `Updated`), which implements `Zuhid.Base.IEntity`
  - `<Feature>/` — one folder per resource (e.g. `StringType/`): `<Feature>Model.cs`, `<Feature>Repository.cs`, `<Feature>Controller.cs`
  - `TemplateContext.cs` — `DbContext`; one `DbSet<XEntity> X` per entity; applies `ToSnakeCase()` and `LoadCsvData()`
  - `Migrations/` — generated; never hand-edit
  - `Dataload/Entities/<Name>.csv` — optional seed data per entity (`StringTypeEntity` → `StringType.csv`), applied via `HasData` at migration time
- `Template.Tests/` — xUnit tests
- `Web.Vue/` — Vue + Vite front end with Playwright tests
- `../Base/` — sibling repo `Zuhid.Base` (project reference in Debug, NuGet package in Release). Shared base classes live there; read them before changing behavior that comes from them.

## Base library conventions (`Zuhid.Base`)

- `RepositoryBase<TContext, TEntity, TModel>` — subclasses only override `Query` (an `IQueryable<TModel>` projection). Get/Get(id)/Get(QueryRequest)/GetStream/Count/Add/Update/Delete come from the base. Don't override Add/Update unless asked.
- `ControllerApiBase<TContext, TRepository, TEntity, TModel, TMapper, TValidator>` — controllers expose thin actions that call `GetCore`, `AddCore`, `UpdateCore`, `DeleteCore`, etc. `AddCore`/`UpdateCore` stamp `Updated` and `UpdatedById` from the JWT.
- Use `MapperBase<TEntity, TModel>` and `NoValidator<TModel>` directly unless a feature needs custom mapping/validation; both are registered as open generics in `Program.cs`.
- Models implement `IModel` (`Id`, `UpdatedById`, `Updated`).

## DI

`Program.cs` registers every class whose name ends in `Repository`, `Mapper` or `Validator` in this assembly as scoped (`AddScopedByConvention`). New feature classes following that naming need no manual registration.

## Style

- File-scoped namespaces: `Zuhid.Template.<Folder>`.
- Primary constructors; expression-bodied members where short.
- 4-space indentation in C#, 2 elsewhere (`.editorconfig`); max line length 200.
- Nullable enabled; required strings use `= null!`.

## Commands

```bash
dotnet build Template                      # build API
dotnet test Template.Tests                 # run tests
dotnet tool restore                        # restore dotnet-ef, reportgenerator
```

Database: `scripts.sh` `update_database Template Template` deletes `Migrations/`, regenerates the `Initial` migration and `Script.sql`, then **drops and recreates the database**. To regenerate migration files only (no DB changes):

```bash
rm -rf Template/Migrations
dotnet ef migrations add Initial --project Template --startup-project Template
dotnet ef migrations script --project Template --startup-project Template --output Template/Migrations/Script.sql
```

Never drop or update the database without asking.

## Agents

- `api-dev` (`.claude/agents/api-dev.md`) — scaffolds/changes features in `Template/` following the StringType structure; does not write or run tests.
- `api-test` (`.claude/agents/api-test.md`) — writes and runs xUnit tests in `Template.Tests/` (in-memory Sqlite); does not change production code.

## Known gotchas

- `commit.sh` commits everything with a timestamp message and pushes; don't run it unless asked.
