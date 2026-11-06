using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.NumericType;

public class NumericTypeRepository(TemplateContext context) : RepositoryBase<TemplateContext, NumericTypeEntity, NumericTypeModel>(context)
{
    protected override IQueryable<NumericTypeModel> Query => Context.NumericType.Select(e => new NumericTypeModel
    {
        Id = e.Id,
        SmallintType = e.SmallintType,
        IntegerType = e.IntegerType,
        BigintType = e.BigintType,
        UpdatedById = e.UpdatedById,
        Updated = e.Updated,
    });
}
