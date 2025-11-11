using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WebTemplate.ApiTests;

public class TestWebAppFactory : WebApplicationFactory<WebTemplate.API.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["AuthSettings:User:RequireConfirmedEmail"] = "false"
            };
            configBuilder.AddInMemoryCollection(overrides!);
        });
    }
}
