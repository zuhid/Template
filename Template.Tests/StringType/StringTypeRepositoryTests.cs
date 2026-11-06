using Zuhid.Base.Query;
using Zuhid.Template.Entities;
using Zuhid.Template.StringType;

namespace Zuhid.Template.Tests.StringType;

public class StringTypeRepositoryTests
{
    private static StringTypeEntity NewEntity(string? charType = null, string? varcharType = null, string? textType = null) => new()
    {
        Id = Guid.NewGuid(),
        CharType = charType ?? Guid.NewGuid().ToString("N")[..8],
        VarcharType = varcharType ?? Guid.NewGuid().ToString("N"),
        TextType = textType ?? Guid.NewGuid().ToString("N"),
        UpdatedById = Guid.NewGuid(),
        Updated = DateTime.UtcNow,
    };

    private static async Task<StringTypeRepository> SeedAsync(TestDatabase db, params StringTypeEntity[] entities)
    {
        db.Context.StringType.AddRange(entities);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        return new StringTypeRepository(db.Context);
    }

    [Fact]
    public async Task Get_Should_MapEveryField_When_RowExists()
    {
        using var db = new TestDatabase();
        var entity = NewEntity();
        var repository = await SeedAsync(db, entity);

        var result = await repository.Get(entity.Id);

        var model = Assert.Single(result);
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.CharType, model.CharType);
        Assert.Equal(entity.VarcharType, model.VarcharType);
        Assert.Equal(entity.TextType, model.TextType);
        Assert.Equal(entity.UpdatedById, model.UpdatedById);
        Assert.Equal(entity.Updated, model.Updated);
    }

    [Fact]
    public async Task Get_Should_ReturnAllRows_When_NoIdProvided()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(), NewEntity(), NewEntity());

        var result = await repository.Get();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Get_Should_ReturnEmpty_When_IdNotFound()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity());

        var result = await repository.Get(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Get_Should_ReturnEmpty_When_TableIsEmpty()
    {
        using var db = new TestDatabase();
        var repository = new StringTypeRepository(db.Context);

        var result = await repository.Get();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRequest_Should_ReturnMatchingRows_When_FilteredByField()
    {
        using var db = new TestDatabase();
        var match = NewEntity(varcharType: "needle-" + Guid.NewGuid());
        var repository = await SeedAsync(db, match, NewEntity(), NewEntity());
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(StringTypeModel.VarcharType), FilterOperator = FieldOperator.Equal, FilterValue = match.VarcharType }],
        };

        var result = await repository.Get(request);

        Assert.Equal(match.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetByRequest_Should_ReturnEmpty_When_NoRowMatchesFilter()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(), NewEntity());
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(StringTypeModel.TextType), FilterOperator = FieldOperator.Equal, FilterValue = "does-not-exist" }],
        };

        var result = await repository.Get(request);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetByRequest_Should_SortRows_When_SortProvided(bool descending)
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(textType: "b"), NewEntity(textType: "c"), NewEntity(textType: "a"));
        var request = new QueryRequest { Sorts = [new SortRule { SortBy = nameof(StringTypeModel.TextType), Descending = descending }] };

        var result = await repository.Get(request);

        var expected = descending ? new[] { "c", "b", "a" } : ["a", "b", "c"];
        Assert.Equal(expected, result.Select(r => r.TextType));
    }

    [Fact]
    public async Task GetByRequest_Should_PageRows_When_SkipAndTakeProvided()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(textType: "a"), NewEntity(textType: "b"), NewEntity(textType: "c"), NewEntity(textType: "d"));
        var request = new QueryRequest
        {
            Sorts = [new SortRule { SortBy = nameof(StringTypeModel.TextType) }],
            Skip = 1,
            Take = 2,
        };

        var result = await repository.Get(request);

        Assert.Equal(["b", "c"], result.Select(r => r.TextType));
    }

    [Fact]
    public async Task Count_Should_ReturnTotalAndFilteredCounts()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(charType: "x"), NewEntity(charType: "x"), NewEntity(charType: "y"));
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(StringTypeModel.CharType), FilterOperator = FieldOperator.Equal, FilterValue = "x" }],
        };

        Assert.Equal(3, await repository.Count());
        Assert.Equal(2, await repository.Count(request));
    }

    [Fact]
    public async Task Add_Should_PersistRow_When_EntityIsNew()
    {
        using var db = new TestDatabase();
        var repository = new StringTypeRepository(db.Context);
        var entity = NewEntity();

        var response = await repository.Add(entity);
        db.Context.ChangeTracker.Clear();

        Assert.Equal(entity.Updated, response.Updated);
        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Equal(entity.TextType, model.TextType);
    }

    [Fact]
    public async Task Add_Should_Throw_When_IdAlreadyExists()
    {
        using var db = new TestDatabase();
        var existing = NewEntity();
        var repository = await SeedAsync(db, existing);
        var duplicate = NewEntity();
        duplicate.Id = existing.Id;

        await Assert.ThrowsAnyAsync<Exception>(() => repository.Add(duplicate));
    }

    [Fact]
    public async Task Update_Should_ChangeFields_When_RowExists()
    {
        using var db = new TestDatabase();
        var entity = NewEntity();
        var repository = await SeedAsync(db, entity);
        var changed = NewEntity(charType: "new", varcharType: "new varchar", textType: "new text");
        changed.Id = entity.Id;

        await repository.Update(changed);
        db.Context.ChangeTracker.Clear();

        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Equal("new", model.CharType);
        Assert.Equal("new varchar", model.VarcharType);
        Assert.Equal("new text", model.TextType);
    }

    [Fact]
    public async Task Update_Should_Throw_When_RowDoesNotExist()
    {
        using var db = new TestDatabase();
        var repository = new StringTypeRepository(db.Context);

        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(() => repository.Update(NewEntity()));
    }

    [Fact]
    public async Task Delete_Should_RemoveOnlyTargetRow()
    {
        using var db = new TestDatabase();
        var target = NewEntity();
        var other = NewEntity();
        var repository = await SeedAsync(db, target, other);

        await repository.Delete(target.Id);

        var remaining = Assert.Single(await repository.Get());
        Assert.Equal(other.Id, remaining.Id);
    }

    [Fact]
    public async Task Delete_Should_NotThrow_When_RowDoesNotExist()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity());

        await repository.Delete(Guid.NewGuid());

        Assert.Single(await repository.Get());
    }
}
