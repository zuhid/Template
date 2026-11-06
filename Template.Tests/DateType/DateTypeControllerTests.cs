using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.DateType;
using Zuhid.Template.Entities;

namespace Zuhid.Template.Tests.DateType;

public class DateTypeControllerTests
{
    private static DateTypeController CreateController(TestDatabase db, Guid? userId) => new(
        new DateTypeRepository(db.Context),
        new MapperBase<DateTypeEntity, DateTypeModel>(),
        new NoValidator<DateTypeModel>())
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userId is null ? [] : [new Claim(ClaimTypes.NameIdentifier, userId.ToString()!)])),
            },
        },
    };

    private static DateTypeModel NewModel() => new()
    {
        Id = Guid.NewGuid(),
        DateTimeWithoutTzType = new DateTime(2024, 3, 15, 10, 30, 45, DateTimeKind.Unspecified),
        DateTimeType = new DateTime(2025, 6, 20, 14, 5, 9, DateTimeKind.Utc),
        TimeSpanType = new TimeSpan(1, 2, 3, 4),
        DateOnlyType = new DateOnly(2023, 12, 25),
        TimeOnlyType = new TimeOnly(8, 15, 30),
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
        Assert.Equal(model.DateTimeWithoutTzType, saved.DateTimeWithoutTzType);
        Assert.Equal(model.TimeSpanType, saved.TimeSpanType);
        Assert.Equal(model.DateOnlyType, saved.DateOnlyType);
        Assert.Equal(model.TimeOnlyType, saved.TimeOnlyType);
        Assert.Equal(userId, saved.UpdatedById);
        Assert.True(saved.Updated >= before);
        Assert.Equal(saved.Updated, response.Updated);
    }

    [Fact]
    public async Task Add_Should_PersistNulls_When_AllOptionalFieldsNull()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());
        var model = new DateTypeModel { Id = Guid.NewGuid() };

        await controller.Add(model);
        db.Context.ChangeTracker.Clear();

        var saved = Assert.Single(await controller.Get(model.Id));
        Assert.Null(saved.DateTimeWithoutTzType);
        Assert.Null(saved.DateTimeType);
        Assert.Null(saved.TimeSpanType);
        Assert.Null(saved.DateOnlyType);
        Assert.Null(saved.TimeOnlyType);
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
    public async Task Get_Should_ReturnEmpty_When_TableIsEmpty()
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
        model.DateOnlyType = new DateOnly(2031, 1, 2);
        model.TimeOnlyType = null;

        var response = await controller.Update(model);
        db.Context.ChangeTracker.Clear();

        Assert.NotNull(response);
        var saved = Assert.Single(await controller.Get(model.Id));
        Assert.Equal(new DateOnly(2031, 1, 2), saved.DateOnlyType);
        Assert.Null(saved.TimeOnlyType);
        Assert.Equal(editor, saved.UpdatedById);
        Assert.Equal(saved.Updated, response.Updated);
    }

    [Fact]
    public async Task Update_Should_Throw_When_RowDoesNotExist()
    {
        using var db = new TestDatabase();
        var controller = CreateController(db, Guid.NewGuid());

        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(() => controller.Update(NewModel()));
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
