using Zuhid.Base.Query;
using Zuhid.Template.DateType;
using Zuhid.Template.Entities;

namespace Zuhid.Template.Tests.DateType;

public class DateTypeRepositoryTests
{
    private static DateTypeEntity NewEntity(DateOnly? dateOnly = null) => new()
    {
        Id = Guid.NewGuid(),
        DateTimeWithoutTzType = new DateTime(2024, 3, 15, 10, 30, 45, DateTimeKind.Unspecified),
        DateTimeType = new DateTime(2025, 6, 20, 14, 5, 9, DateTimeKind.Utc),
        TimeSpanType = new TimeSpan(1, 2, 3, 4),
        DateOnlyType = dateOnly ?? new DateOnly(2023, 12, 25),
        TimeOnlyType = new TimeOnly(8, 15, 30),
        UpdatedById = Guid.NewGuid(),
        Updated = DateTime.UtcNow,
    };

    private static DateTypeEntity NewNullEntity() => new()
    {
        Id = Guid.NewGuid(),
        UpdatedById = Guid.NewGuid(),
        Updated = DateTime.UtcNow,
    };

    private static async Task<DateTypeRepository> SeedAsync(TestDatabase db, params DateTypeEntity[] entities)
    {
        db.Context.DateType.AddRange(entities);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        return new DateTypeRepository(db.Context);
    }

    [Fact]
    public async Task Get_Should_MapEveryField_When_RowHasValues()
    {
        using var db = new TestDatabase();
        var entity = NewEntity();
        var repository = await SeedAsync(db, entity);

        var model = Assert.Single(await repository.Get(entity.Id));

        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.DateTimeWithoutTzType, model.DateTimeWithoutTzType);
        Assert.Equal(entity.DateTimeType!.Value.Ticks, model.DateTimeType!.Value.Ticks);
        Assert.Equal(entity.TimeSpanType, model.TimeSpanType);
        Assert.Equal(entity.DateOnlyType, model.DateOnlyType);
        Assert.Equal(entity.TimeOnlyType, model.TimeOnlyType);
        Assert.Equal(entity.UpdatedById, model.UpdatedById);
        Assert.Equal(entity.Updated.Ticks, model.Updated.Ticks);
    }

    [Fact]
    public async Task Get_Should_MapNulls_When_RowHasNoValues()
    {
        using var db = new TestDatabase();
        var entity = NewNullEntity();
        var repository = await SeedAsync(db, entity);

        var model = Assert.Single(await repository.Get(entity.Id));

        Assert.Null(model.DateTimeWithoutTzType);
        Assert.Null(model.DateTimeType);
        Assert.Null(model.TimeSpanType);
        Assert.Null(model.DateOnlyType);
        Assert.Null(model.TimeOnlyType);
    }

    [Fact]
    public async Task Get_Should_ReturnAllRows_When_NoIdProvided()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(), NewNullEntity(), NewEntity());

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

        Assert.Empty(await new DateTypeRepository(db.Context).Get());
    }

    [Fact]
    public async Task GetByRequest_Should_ReturnMatchingRow_When_FilteredByDateOnly()
    {
        using var db = new TestDatabase();
        var match = NewEntity(new DateOnly(1999, 1, 2));
        var repository = await SeedAsync(db, match, NewEntity(), NewEntity());
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(DateTypeModel.DateOnlyType), FilterOperator = FieldOperator.Equal, FilterValue = "1999-01-02" }],
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
            Filters = [new FilterRule { FilterBy = nameof(DateTypeModel.DateOnlyType), FilterOperator = FieldOperator.Equal, FilterValue = "1900-01-01" }],
        };

        Assert.Empty(await repository.Get(request));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetByRequest_Should_SortRows_When_SortProvided(bool descending)
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(new DateOnly(2020, 2, 1)), NewEntity(new DateOnly(2021, 3, 1)), NewEntity(new DateOnly(2019, 1, 1)));
        var request = new QueryRequest { Sorts = [new SortRule { SortBy = nameof(DateTypeModel.DateOnlyType), Descending = descending }] };

        var result = await repository.Get(request);

        var expected = descending
            ? new[] { new DateOnly(2021, 3, 1), new DateOnly(2020, 2, 1), new DateOnly(2019, 1, 1) }
            : [new DateOnly(2019, 1, 1), new DateOnly(2020, 2, 1), new DateOnly(2021, 3, 1)];
        Assert.Equal(expected, result.Select(r => r.DateOnlyType!.Value));
    }

    [Fact]
    public async Task GetByRequest_Should_PageRows_When_SkipAndTakeProvided()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db,
            NewEntity(new DateOnly(2020, 1, 1)), NewEntity(new DateOnly(2020, 1, 2)), NewEntity(new DateOnly(2020, 1, 3)), NewEntity(new DateOnly(2020, 1, 4)));
        var request = new QueryRequest { Sorts = [new SortRule { SortBy = nameof(DateTypeModel.DateOnlyType) }], Skip = 1, Take = 2 };

        var result = await repository.Get(request);

        Assert.Equal([new DateOnly(2020, 1, 2), new DateOnly(2020, 1, 3)], result.Select(r => r.DateOnlyType!.Value));
    }

    [Fact]
    public async Task Count_Should_ReturnTotalAndFilteredCounts()
    {
        using var db = new TestDatabase();
        var repository = await SeedAsync(db, NewEntity(new DateOnly(2000, 5, 5)), NewEntity(new DateOnly(2000, 5, 5)), NewEntity(new DateOnly(2001, 5, 5)));
        var request = new QueryRequest
        {
            Filters = [new FilterRule { FilterBy = nameof(DateTypeModel.DateOnlyType), FilterOperator = FieldOperator.Equal, FilterValue = "2000-05-05" }],
        };

        Assert.Equal(3, await repository.Count());
        Assert.Equal(2, await repository.Count(request));
    }

    [Fact]
    public async Task Add_Should_RoundTripAllValues_When_EntityHasValues()
    {
        using var db = new TestDatabase();
        var repository = new DateTypeRepository(db.Context);
        var entity = NewEntity();

        var response = await repository.Add(entity);
        db.Context.ChangeTracker.Clear();

        Assert.Equal(entity.Updated, response.Updated);
        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Equal(entity.DateTimeWithoutTzType, model.DateTimeWithoutTzType);
        Assert.Equal(entity.TimeSpanType, model.TimeSpanType);
        Assert.Equal(entity.DateOnlyType, model.DateOnlyType);
        Assert.Equal(entity.TimeOnlyType, model.TimeOnlyType);
    }

    [Fact]
    public async Task Add_Should_RoundTripNulls_When_EntityHasNoValues()
    {
        using var db = new TestDatabase();
        var repository = new DateTypeRepository(db.Context);
        var entity = NewNullEntity();

        await repository.Add(entity);
        db.Context.ChangeTracker.Clear();

        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Null(model.DateTimeWithoutTzType);
        Assert.Null(model.DateTimeType);
        Assert.Null(model.TimeSpanType);
        Assert.Null(model.DateOnlyType);
        Assert.Null(model.TimeOnlyType);
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
        var changed = NewEntity(new DateOnly(2030, 7, 8));
        changed.Id = entity.Id;
        changed.TimeSpanType = new TimeSpan(0, 5, 6, 7);
        changed.TimeOnlyType = new TimeOnly(23, 59, 1);

        await repository.Update(changed);
        db.Context.ChangeTracker.Clear();

        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Equal(new DateOnly(2030, 7, 8), model.DateOnlyType);
        Assert.Equal(new TimeSpan(0, 5, 6, 7), model.TimeSpanType);
        Assert.Equal(new TimeOnly(23, 59, 1), model.TimeOnlyType);
    }

    [Fact]
    public async Task Update_Should_ClearFieldsToNull_When_ValuesSetToNull()
    {
        using var db = new TestDatabase();
        var entity = NewEntity();
        var repository = await SeedAsync(db, entity);
        var changed = NewNullEntity();
        changed.Id = entity.Id;

        await repository.Update(changed);
        db.Context.ChangeTracker.Clear();

        var model = Assert.Single(await repository.Get(entity.Id));
        Assert.Null(model.DateTimeWithoutTzType);
        Assert.Null(model.DateTimeType);
        Assert.Null(model.TimeSpanType);
        Assert.Null(model.DateOnlyType);
        Assert.Null(model.TimeOnlyType);
    }

    [Fact]
    public async Task Update_Should_Throw_When_RowDoesNotExist()
    {
        using var db = new TestDatabase();
        var repository = new DateTypeRepository(db.Context);

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
