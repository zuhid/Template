using Microsoft.EntityFrameworkCore;
using Zuhid.Base;
using Zuhid.Template.Entities;

namespace Zuhid.Template;

public class TemplateContext(DbContextOptions<TemplateContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ToSnakeCase();
        builder.LoadCsvData();
    }
    public DbSet<StringTypeEntity> StringType { get; set; }
    public DbSet<NumericTypeEntity> NumericType { get; set; }
    public DbSet<DateTypeEntity> DateType { get; set; }
}
