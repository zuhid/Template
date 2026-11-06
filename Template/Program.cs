using Zuhid.Base;

namespace Zuhid.Template;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var appSetting = new AppSetting(builder.Configuration);
        builder.Services.AddSingleton(appSetting);
        builder.AddServices(appSetting);
        builder.AddScopedByConvention(typeof(AppSetting).Assembly, ["Repository", "Mapper", "Validator"]);
        builder.Services.AddScoped(typeof(NoValidator<>));
        builder.Services.AddScoped(typeof(MapperBase<,>));
        builder.AddPostgres<TemplateContext>(appSetting.ConnectionStrings.Template);
        builder.Services.AddSignalR();

        var app = builder.BuildServices(appSetting);
        app.MapHubsByConvention(typeof(AppSetting).Assembly);
        app.Run();
    }
}
