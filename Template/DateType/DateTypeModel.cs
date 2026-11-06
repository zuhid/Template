using Zuhid.Base;

namespace Zuhid.Template.DateType;

public class DateTypeModel : IModel
{
    public Guid Id { get; set; }
    public DateTime? DateTimeWithoutTzType { get; set; }
    public DateTime? DateTimeType { get; set; }
    public TimeSpan? TimeSpanType { get; set; }
    public DateOnly? DateOnlyType { get; set; }
    public TimeOnly? TimeOnlyType { get; set; }
    public Guid UpdatedById { get; set; }
    public DateTime Updated { get; set; }
}
