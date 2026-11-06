using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.StringType;

[ApiController]
[Route("[controller]")]
public class StringTypeController(StringTypeRepository repository, MapperBase<StringTypeEntity, StringTypeModel> mapper, NoValidator<StringTypeModel> validator)
    : ControllerApiBase<TemplateContext, StringTypeRepository, StringTypeEntity, StringTypeModel, MapperBase<StringTypeEntity, StringTypeModel>, NoValidator<StringTypeModel>>
    (repository, mapper, validator)
{
    [HttpGet]
    public async Task<List<StringTypeModel>> Get() => await GetCore();

    [HttpGet("{id:guid}")]
    public async Task<List<StringTypeModel>> Get(Guid id) => await GetCore(id);

    [HttpPost]
    public async Task<UpdatedResponse?> Add(StringTypeModel model) => await AddCore(model);

    [HttpPut]
    public async Task<UpdatedResponse?> Update(StringTypeModel model) => await UpdateCore(model);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await DeleteCore(id);
}
