using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.Entities;
using Zuhid.Template.NumericType;

namespace Zuhid.Template.Tests.NumericType;

public class NumericTypeControllerTests
{
    private static NumericTypeController CreateController(TestDatabase db, Guid? userId) => new(
        new NumericTypeRepository(db.Context),
        new MapperBase<NumericTypeEntity, NumericTypeModel>(),
        new NoValidator<NumericTypeModel>())
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userId is null ? [] : [new Claim(ClaimTypes.NameIdentifier, userId.ToString()!)])),
            },
        },
    };

    private static NumericTypeModel NewModel() => new()
    {
        Id = Guid.NewGuid(),
        SmallintType = 10,
        IntegerType = 1000,
        BigintType = 100000,
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
        Assert.Equal(10, saved.SmallintType);
        Assert.Equal(1000, saved.IntegerType);
        Assert.Equal(100000, saved.BigintType);
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
    public async Task Get_Should_ReturnAllRows_When_NoIdProvided()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());
        await controller.Add(NewModel());
        await controller.Add(NewModel());
        db.Context.ChangeTracker.Clear();

        Assert.Equal(2, (await controller.Get()).Count);
    }

    [Fact]
    public async Task Get_Should_ReturnEmpty_When_NoRowsExist()
    {
        using var db = new TestDatabase();

        Assert.Empty(await CreateController(db, Guid.NewGuid()).Get());
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

        Assert.Empty(await controller.Get(Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_Should_ChangeRowAndRestampUser_When_ModelProvided()
    {
        using var db = new TestDatabase();
        var editor = Guid.NewGuid();
        var model = NewModel();
        await CreateController(db, Guid.NewGuid()).Add(model);
        db.Context.ChangeTracker.Clear();
        var controller = CreateController(db, editor);
        model.IntegerType = 4321;

        var response = await controller.Update(model);
        db.Context.ChangeTracker.Clear();

        Assert.NotNull(response);
        var saved = Assert.Single(await controller.Get(model.Id));
        Assert.Equal(4321, saved.IntegerType);
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

        await CreateController(db, Guid.NewGuid()).Delete(Guid.NewGuid());
    }
}
