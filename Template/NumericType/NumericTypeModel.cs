using System.ComponentModel.DataAnnotations;
using Zuhid.Base;

namespace Zuhid.Template.NumericType;

public class NumericTypeModel : IModel
{
    public Guid Id { get; set; }

    [Range(0, 100)]
    public short SmallintType { get; set; }

    [Range(0, int.MaxValue)]
    public int IntegerType { get; set; }

    [Range(long.MinValue, 500)]
    public long BigintType { get; set; }

    public Guid UpdatedById { get; set; }
    public DateTime Updated { get; set; }
}
