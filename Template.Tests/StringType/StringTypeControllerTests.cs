using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.Entities;
using Zuhid.Template.StringType;

namespace Zuhid.Template.Tests.StringType;

public class StringTypeControllerTests
{
    private static StringTypeController CreateController(TestDatabase db, Guid? userId) => new(
        new StringTypeRepository(db.Context),
        new MapperBase<StringTypeEntity, StringTypeModel>(),
        new NoValidator<StringTypeModel>())
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userId is null ? [] : [new Claim(ClaimTypes.NameIdentifier, userId.ToString()!)])),
            },
        },
    };

    private static StringTypeModel NewModel() => new()
    {
        Id = Guid.NewGuid(),
        CharType = "char",
        VarcharType = Guid.NewGuid().ToString("N"),
        TextType = Guid.NewGuid().ToString("N"),
    };

    [Fact]
    public async Task Add_Should_PersistRowStampedWithUser_When_ModelProvided()
    {
        using var db = new TestDatabase();
        var userId = Guid.NewGuid();
        var controller = CreateController(db, userId);
        var model = NewModel();
        var before = DateTime.UtcNow;

        var response = await controller.Add(model);
        db.Context.ChangeTracker.Clear();

        Assert.NotNull(response);
        var saved = Assert.Single(await controller.Get(model.Id));
        Assert.Equal(model.VarcharType, saved.VarcharType);
        Assert.Equal(userId, saved.UpdatedById);
        Assert.True(saved.Updated >= before);
        Assert.Equal(saved.Updated, response.Updated);
    }

    [Fact]
    public async Task Add_Should_IgnoreClientSuppliedUpdatedById()
    {
        using var db = new TestDatabase();
        var userId = Guid.NewGuid();
        var controller = CreateController(db, userId);
        var model = NewModel();
        model.UpdatedById = Guid.NewGuid();

        await controller.Add(model);
        db.Context.ChangeTracker.Clear();

        Assert.Equal(userId, Assert.Single(await controller.Get(model.Id)).UpdatedById);
    }

    [Fact]
    public async Task Add_Should_StampEmptyUserId_When_NoUserClaim()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, null);
        var model = NewModel();

        await controller.Add(model);
        db.Context.ChangeTracker.Clear();

        Assert.Equal(Guid.Empty, Assert.Single(await controller.Get(model.Id)).UpdatedById);
    }

    [Fact]
    public async Task Get_Should_ReturnMatchingRow_When_IdProvided()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());
        var first = NewModel();
        var second = NewModel();
        await controller.Add(first);
        await controller.Add(second);
        db.Context.ChangeTracker.Clear();

        var result = await controller.Get(second.Id);

        Assert.Equal(second.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task Get_Should_ReturnEmpty_When_IdNotFound()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());

        var result = await controller.Get(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Update_Should_ChangeRowAndRestampUser_When_ModelProvided()
    {
        using var db = new TestDatabase();
        var creator = Guid.NewGuid();
        var editor = Guid.NewGuid();
        var model = NewModel();
        await CreateController(db, creator).Add(model);
        db.Context.ChangeTracker.Clear();
        var controller = CreateController(db, editor);
        model.TextType = "updated text";

        var response = await controller.Update(model);
        db.Context.ChangeTracker.Clear();

        Assert.NotNull(response);
        var saved = Assert.Single(await controller.Get(model.Id));
        Assert.Equal("updated text", saved.TextType);
        Assert.Equal(editor, saved.UpdatedById);
        Assert.Equal(saved.Updated, response.Updated);
    }

    [Fact]
    public async Task Delete_Should_RemoveRow_When_IdProvided()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());
        var model = NewModel();
        await controller.Add(model);
        db.Context.ChangeTracker.Clear();

        await controller.Delete(model.Id);

        Assert.Empty(await controller.Get(model.Id));
    }

    [Fact]
    public async Task Delete_Should_NotThrow_When_IdNotFound()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());

        await controller.Delete(Guid.NewGuid());
    }
}
