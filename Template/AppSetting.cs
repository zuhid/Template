using Zuhid.Base;

namespace Zuhid.Template;

public class AppSetting : SettingBase
{
    public ConnectionString ConnectionStrings { get; set; } = default!;
    public class ConnectionString
    {
        public string Template { get; set; } = default!;
        public string Log { get; set; } = default!;
    }

    public AppSetting(IConfiguration configuration) : base(configuration)
    {
        ConnectionStrings = new ConnectionString
        {
            Template = ReplaceCredential(configuration, "Template"),
            Log = ReplaceCredential(configuration, "Log"),
        };
    }

    private static string ReplaceCredential(IConfiguration configuration, string connString)
    {
        return configuration.GetConnectionString(connString)!
          .Replace("[postgres_credential]", configuration.GetValue<string>("postgres_credential"), StringComparison.Ordinal);
    }
}
