using Zuhid.Base.Query;
using Zuhid.Template.Entities;
using Zuhid.Template.NumericType;

namespace Zuhid.Template.Tests.NumericType;

public class NumericTypeRepositoryTests
{
    private static NumericTypeEntity NewEntity(short smallint = 1, int integer = 1, long bigint = 1) => new()
    {
        Id = Guid.NewGuid(),
        SmallintType = smallint,
        IntegerType = integer,
        BigintType = bigint,
        UpdatedById = Guid.NewGuid(),
        Updated = DateTime.UtcNow,
    };

    private static async Task<NumericTypeRepository> SeedAsync(TestDatabase db, params NumericTypeEntity[] entities)
    {
        db.Context.NumericType.AddRange(entities);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        return new NumericTypeRepository(db.Context);
    }

    [Fact]
    public async Task Get_Should_MapEveryField_When_RowExists()
    {
        using var db = new TestDatabase();
        var entity = NewEntity(42, 123456, -9876543210L);
        var repository = await SeedAsync(db, entity);

        var result = await repository.Get(entity.Id);

        var model = Assert.Single(result);
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.SmallintType, model.SmallintType);
        Assert.Equal(entity.IntegerType, model.IntegerType);
        Assert.Equal(entity.BigintType, model.BigintType);
        Assert.Equal(entity.UpdatedById, model.UpdatedById);
        Assert.Equal(entity.Updated, model.Updated);
    }

    [Fact]
    public async Task Get_Should_ReturnAllRows_When_NoIdProvided()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(), NewEntity(), NewEntity());

        Assert.Equal(3, (await repository.Get()).Count);
    }

    [Fact]
    public async Task Get_Should_ReturnEmpty_When_IdNotFound()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity());

        Assert.Empty(await repository.Get(Guid.NewGuid()));
    }

    [Fact]
    public async Task Get_Should_ReturnEmpty_When_TableIsEmpty()
    {
        using var db = new TestDatabase();

        Assert.Empty(await new NumericTypeRepository(db.Context).Get());
    }

    [Fact]
    public async Task Get_Should_RoundTripBoundaryValues()
    {
        using var db = new TestDatabase();
        var entity = NewEntity(100, int.MaxValue, long.MinValue);
        var repository = await SeedAsync(db, entity);

        var model = Assert.Single(await repository.Get(entity.Id));

        Assert.Equal(100, model.SmallintType);
        Assert.Equal(int.MaxValue, model.IntegerType);
        Assert.Equal(long.MinValue, model.BigintType);
    }

    [Fact]
    public async Task GetByRequest_Should_ReturnMatchingRows_When_FilteredByField()
    {
        using var db = new TestDatabase();
        var match = NewEntity(integer: 777001);
        var repository = await SeedAsync(db, match, NewEntity(integer: 1), NewEntity(integer: 2));
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(NumericTypeModel.IntegerType), FilterOperator = FieldOperator.Equal, FilterValue = "777001" }],
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
            Filters = [new FilterRule { FilterBy = nameof(NumericTypeModel.IntegerType), FilterOperator = FieldOperator.Equal, FilterValue = "999999" }],
        };

        Assert.Empty(await repository.Get(request));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetByRequest_Should_SortRows_When_SortProvided(bool descending)
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(bigint: 20), NewEntity(bigint: 30), NewEntity(bigint: 10));
        var request = new QueryRequest { Sorts = [new SortRule { SortBy = nameof(NumericTypeModel.BigintType), Descending = descending }] };

        var result = await repository.Get(request);

        var expected = descending ? new long[] { 30, 20, 10 } : [10, 20, 30];
        Assert.Equal(expected, result.Select(r => r.BigintType));
    }

    [Fact]
    public async Task GetByRequest_Should_PageRows_When_SkipAndTakeProvided()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(smallint: 1), NewEntity(smallint: 2), NewEntity(smallint: 3), NewEntity(smallint: 4));
        var request = new QueryRequest
        {
            Sorts = [new SortRule { SortBy = nameof(NumericTypeModel.SmallintType) }],
            Skip = 1,
            Take = 2,
        };

        var result = await repository.Get(request);

        Assert.Equal(new short[] { 2, 3 }, result.Select(r => r.SmallintType));
    }

    [Fact]
    public async Task Count_Should_ReturnTotalAndFilteredCounts()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(smallint: 5), NewEntity(smallint: 5), NewEntity(smallint: 6));
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(NumericTypeModel.SmallintType), FilterOperator = FieldOperator.Equal, FilterValue = "5" }],
        };

        Assert.Equal(3, await repository.Count());
        Assert.Equal(2, await repository.Count(request));
    }

    [Fact]
    public async Task Add_Should_PersistRow_When_EntityIsNew()
    {
        using var db = new TestDatabase();
        var repository = new NumericTypeRepository(db.Context);
        var entity = NewEntity(7, 8, 9);

        var response = await repository.Add(entity);
        db.Context.ChangeTracker.Clear();

        Assert.Equal(entity.Updated, response.Updated);
        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Equal(9, model.BigintType);
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
        var changed = NewEntity(11, 22, 33);
        changed.Id = entity.Id;

        await repository.Update(changed);
        db.Context.ChangeTracker.Clear();

        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Equal(11, model.SmallintType);
        Assert.Equal(22, model.IntegerType);
        Assert.Equal(33, model.BigintType);
    }

    [Fact]
    public async Task Update_Should_Throw_When_RowDoesNotExist()
    {
        using var db = new TestDatabase();
        var repository = new NumericTypeRepository(db.Context);

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

        Assert.Equal(other.Id, Assert.Single(await repository.Get()).Id);
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
