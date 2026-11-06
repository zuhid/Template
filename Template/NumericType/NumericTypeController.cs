using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.NumericType;

[ApiController]
[Route("[controller]")]
public class NumericTypeController(NumericTypeRepository repository, MapperBase<NumericTypeEntity, NumericTypeModel> mapper, NoValidator<NumericTypeModel> validator)
    : ControllerApiBase<TemplateContext, NumericTypeRepository, NumericTypeEntity, NumericTypeModel, MapperBase<NumericTypeEntity, NumericTypeModel>, NoValidator<NumericTypeModel>>
    (repository, mapper, validator)
{
    [HttpGet]
    public async Task<List<NumericTypeModel>> Get() => await GetCore();

    [HttpGet("{id:guid}")]
    public async Task<List<NumericTypeModel>> Get(Guid id) => await GetCore(id);

    [HttpPost]
    public async Task<UpdatedResponse?> Add(NumericTypeModel model) => await AddCore(model);

    [HttpPut]
    public async Task<UpdatedResponse?> Update(NumericTypeModel model) => await UpdateCore(model);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await DeleteCore(id);
}
