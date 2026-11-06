using Zuhid.Base;

namespace Zuhid.Template.StringType;

public class StringTypeModel : IModel
{
    public Guid Id { get; set; }
    public string CharType { get; set; } = null!;
    public string VarcharType { get; set; } = null!;
    public string TextType { get; set; } = null!;
    public Guid UpdatedById { get; set; }
    public DateTime Updated { get; set; }
}
