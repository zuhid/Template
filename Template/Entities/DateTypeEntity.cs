using System.ComponentModel.DataAnnotations.Schema;

namespace Zuhid.Template.Entities;

public class DateTypeEntity : BaseEntity
{

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? DateTimeWithoutTzType { get; set; }
    public DateTime? DateTimeType { get; set; }
    public TimeSpan? TimeSpanType { get; set; }
    public DateOnly? DateOnlyType { get; set; }
    public TimeOnly? TimeOnlyType { get; set; }
}
