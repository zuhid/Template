using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.StringType;

public class StringTypeRepository(TemplateContext context): RepositoryBase<TemplateContext, StringTypeEntity, StringTypeModel>(context)
{
    protected override IQueryable<StringTypeModel> Query => Context.StringType.Select(e => new StringTypeModel
    {
        Id = e.Id,
        CharType = e.CharType,
        VarcharType = e.VarcharType,
        TextType = e.TextType,
        UpdatedById = e.UpdatedById,
        Updated = e.Updated,
    });
}
