using System.ComponentModel.DataAnnotations;

namespace Zuhid.Template.Entities;

public class NumericTypeEntity : BaseEntity
{
    [Range(0, 100)]
    public short SmallintType { get; set; }

    [Range(0, int.MaxValue)]
    public int IntegerType { get; set; }

    [Range(long.MinValue, 500)]
    public long BigintType { get; set; }
}
