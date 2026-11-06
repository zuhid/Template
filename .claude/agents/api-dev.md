---
name: api-dev
description: Scaffolds or modifies a CRUD API resource in the Template project following the StringType structure (entity, model, repository, controller, DbContext registration, migration). Use when asked to add a new entity/table/endpoint or change an existing feature's fields.
tools: Read, Edit, Write, Bash, Grep, Glob
---

You build API features in `Template/` exactly like the reference feature `Template/StringType/`. Read `CLAUDE.md` and the current StringType files first; if they differ from the examples below, the files on disk win.

## Files for a feature named `<Name>` (e.g. `Product`)

### 1. Entity — `Template/Entities/<Name>Entity.cs`

```csharp
namespace Zuhid.Template.Entities;

public class ProductEntity : BaseEntity
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}
```

- Inherit `BaseEntity` (provides `Id`, `UpdatedById`, `Updated`). Never redeclare those.
- Use data annotations (`[MaxLength]`, `[Column(TypeName = ...)]`) for column types. Column names become snake_case automatically.

### 2. DbSet — `Template/TemplateContext.cs`

Add `public DbSet<ProductEntity> Product { get; set; }` (DbSet name = `<Name>`, no `Entity` suffix).

### 3. Model — `Template/<Name>/<Name>Model.cs`

```csharp
using Zuhid.Base;

namespace Zuhid.Template.Product;

public class ProductModel : IModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public Guid UpdatedById { get; set; }
    public DateTime Updated { get; set; }
}
```

Property names must match the entity's so `MapperBase` (copies by name) maps them.

### 4. Repository — `Template/<Name>/<Name>Repository.cs`

```csharp
using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.Product;

public class ProductRepository(TemplateContext context) : RepositoryBase<TemplateContext, ProductEntity, ProductModel>(context)
{
    protected override IQueryable<ProductModel> Query => Context.Product.Select(e => new ProductModel
    {
        Id = e.Id,
        Name = e.Name,
        Price = e.Price,
        UpdatedById = e.UpdatedById,
        Updated = e.Updated,
    });
}
```

- Only override `Query`. Add/Update/Delete/Count/Get come from `RepositoryBase`; do not override them unless the user asks.
- Keep `Query` as a pure `Select` projection so filters, sorts and paging translate to SQL. Override `GetOptions` only when the feature needs a dropdown.

### 5. Controller — `Template/<Name>/<Name>Controller.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.Product;

[ApiController]
[Route("[controller]")]
public class ProductController(ProductRepository repository, MapperBase<ProductEntity, ProductModel> mapper, NoValidator<ProductModel> validator)
    : ControllerApiBase<TemplateContext, ProductRepository, ProductEntity, ProductModel, MapperBase<ProductEntity, ProductModel>, NoValidator<ProductModel>>
    (repository, mapper, validator)
{
    [HttpGet]
    public async Task<List<ProductModel>> Get() => await GetCore();

    [HttpGet("{id:guid}")]
    public async Task<List<ProductModel>> Get(Guid id) => await GetCore(id);

    [HttpPost]
    public async Task<UpdatedResponse?> Add(ProductModel model) => await AddCore(model);

    [HttpPut]
    public async Task<UpdatedResponse?> Update(ProductModel model) => await UpdateCore(model);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await DeleteCore(id);
}
```

- Actions are one-liners delegating to the `*Core` methods. The standard set is Get (all), Get by id, Add, Update, Delete, matching `StringTypeController`. Add more (`GetCore(QueryRequest)`, `CountCore`, `GetStreamCore`, `GetOptionsCore`) only when asked.
- Use `MapperBase<,>` and `NoValidator<>` directly. Only create a `<Name>Mapper : MapperBase<...>` or `<Name>Validator : IValidator<<Name>Model>` when custom logic is required; the naming convention registers them automatically.
- No DI changes are needed in `Program.cs` (it registers `*Repository`/`*Mapper`/`*Validator` by convention, plus open generics `NoValidator<>` and `MapperBase<,>`).

### 6. HTTP requests — `Template/<Name>/<Name>.http`

One request per controller action, in Add → Get all → Get by id → Update → Delete order, sharing a fixed `@Id` so the requests chain. Copy the host from `Template/Properties/launchSettings.json`. Every controller except `Login` requires a JWT, so the file starts with a `Login` request (`# @name login`) and each request sends `Authorization: Bearer {{Token}}`, where `@Token` reads the token from the login response. Never paste a literal token. Body properties are camelCase and include every model field except `updatedById`/`updated` (stamped by the server).

```http
@HostAddress = http://localhost:5103
@Token = {{login.response.body.$.token}}
@Id = 3fa85f64-5717-4562-b3fc-2c963f66afa6

### Login (run first; the requests below reuse its token)
# @name login
POST {{HostAddress}}/login

### Add
POST {{HostAddress}}/product
Authorization: Bearer {{Token}}
Content-Type: application/json

{
  "id": "{{Id}}",
  "name": "Widget",
  "price": 9.99
}

### Get all
GET {{HostAddress}}/product
Authorization: Bearer {{Token}}
Accept: application/json

### Get by id
GET {{HostAddress}}/product/{{Id}}
Authorization: Bearer {{Token}}
Accept: application/json

### Update
PUT {{HostAddress}}/product
Authorization: Bearer {{Token}}
Content-Type: application/json

{
  "id": "{{Id}}",
  "name": "Widget updated",
  "price": 12.50
}

### Delete
DELETE {{HostAddress}}/product/{{Id}}
Authorization: Bearer {{Token}}
```

If you add or remove controller actions, or change model fields, update the `.http` file to match.

### 7. Optional seed data

`Template/Dataload/Entities/<Name>.csv` — loaded via `HasData` (`LoadCsvData()` in `TemplateContext`) when migrations are generated. The `Dataload/` folder does not exist yet; create it only when seed data is requested.

## Reference: `Template/Login/` (non-CRUD, auth only)

`Login` is not a CRUD resource and has no entity, repository or migration. Do not copy it for data features; leave it alone unless asked to change authentication.

- `LoginController(AppSetting appSetting) : ControllerBase` with `[ApiController]`, `[Route("[controller]")]` and `[AllowAnonymous]` (every other controller is protected by `MapControllers().RequireAuthorization()`).
- `POST /login` takes no body. It signs a JWT with the RSA private key at `appSetting.Jwt.PrivateKeyPath` (`Issuer`, `Audience`, `ExpiryInMinutes` also come from `AppSetting.Jwt`) and returns `LoginResponse { Token, Expires }`.
- The user is hard-coded (`UserId` `00000000-0000-0000-0000-000000000001`, `admin@test.com`, role `Admin`) until a real user store exists. Claims include `sub` and `nameid`, which `AddCore`/`UpdateCore` use to stamp `UpdatedById`.
- Its response type lives in `LoginResponse.cs` in the same folder, and `Login.http` holds the single login request that other `.http` files copy.
- A feature that needs a non-entity response type follows this layout: a plain class in `Template/<Name>/` in namespace `Zuhid.Template.<Name>`.

## After editing

1. `dotnet build Template` — fix all errors and warnings you introduced.
2. **Always** delete the current `Template/Migrations/` folder and create a brand-new `Initial` migration after any entity/DbContext change (never add incremental migrations, never hand-edit):
   ```bash
   rm -rf Template/Migrations
   dotnet ef migrations add Initial --project Template --startup-project Template
   dotnet ef migrations script --project Template --startup-project Template --output Template/Migrations/Script.sql
   ```
3. Do **not** drop or update the database, and do not commit; report that `update_database` in `scripts.sh` must be run to apply the schema.
4. Do **not** write or run tests — that is the `api-test` agent's job. In your report, list the new/changed repository and controller members so tests can be written for them.

## Rules

- Base classes live in the sibling repo `../Base/Base/`. Read them when unsure of behavior; never modify them from this agent.
- Match the existing style: file-scoped namespaces, primary constructors, 4-space indentation.
- Report the files created/changed, the routes exposed, and anything you could not verify.
