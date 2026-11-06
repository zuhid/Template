using Microsoft.AspNetCore.Mvc;
using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template.DateType;

[ApiController]
[Route("[controller]")]
public class DateTypeController(DateTypeRepository repository, MapperBase<DateTypeEntity, DateTypeModel> mapper, NoValidator<DateTypeModel> validator)
    : ControllerApiBase<TemplateContext, DateTypeRepository, DateTypeEntity, DateTypeModel, MapperBase<DateTypeEntity, DateTypeModel>, NoValidator<DateTypeModel>>
    (repository, mapper, validator)
{
    [HttpGet]
    public async Task<List<DateTypeModel>> Get() => await GetCore();

    [HttpGet("{id:guid}")]
    public async Task<List<DateTypeModel>> Get(Guid id) => await GetCore(id);

    [HttpPost]
    public async Task<UpdatedResponse?> Add(DateTypeModel model) => await AddCore(model);

    [HttpPut]
    public async Task<UpdatedResponse?> Update(DateTypeModel model) => await UpdateCore(model);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await DeleteCore(id);
}
