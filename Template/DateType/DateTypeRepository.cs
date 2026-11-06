using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.DateType;

public class DateTypeRepository(TemplateContext context) : RepositoryBase<TemplateContext, DateTypeEntity, DateTypeModel>(context)
{
    protected override IQueryable<DateTypeModel> Query => Context.DateType.Select(e => new DateTypeModel
    {
        Id = e.Id,
        DateTimeWithoutTzType = e.DateTimeWithoutTzType,
        DateTimeType = e.DateTimeType,
        TimeSpanType = e.TimeSpanType,
        DateOnlyType = e.DateOnlyType,
        TimeOnlyType = e.TimeOnlyType,
        UpdatedById = e.UpdatedById,
        Updated = e.Updated,
    });
}
