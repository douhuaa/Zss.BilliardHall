using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Extensions.DependencyInjection;

using OpenIddict.Validation.AspNetCore;
using OpenIddict.Server.AspNetCore;

using Zss.BilliardHall.Blazor.Client.Navigation;
using Zss.BilliardHall.EntityFrameworkCore;
using Zss.BilliardHall.Localization;
using Zss.BilliardHall.MultiTenancy;

using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

using Zss.BilliardHall.Blazor.Client;
using Zss.BilliardHall.Blazor.Components;

using Volo.Abp;
using Volo.Abp.AspNetCore.Components.Web;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.Components.WebAssembly.Theming.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Components.Server.LeptonXLiteTheme.Bundling;
using Volo.Abp.AspNetCore.Components.Server.LeptonXLiteTheme;
using Volo.Abp.AspNetCore.Components.WebAssembly.LeptonXLiteTheme.Bundling;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Account.Web;
using Volo.Abp.Identity.Blazor.Server;
using Volo.Abp.TenantManagement.Blazor.Server;

namespace Zss.BilliardHall.Blazor;

[DependsOn(typeof(BilliardHallApplicationModule),
typeof(BilliardHallEntityFrameworkCoreModule),
typeof(BilliardHallHttpApiModule),
typeof(AbpAutofacModule),
typeof(AbpSwashbuckleModule),
typeof(AbpIdentityBlazorServerModule),
typeof(AbpTenantManagementBlazorServerModule),
typeof(AbpAccountWebOpenIddictModule),
typeof(AbpAspNetCoreComponentsServerLeptonXLiteThemeModule),
typeof(AbpAspNetCoreComponentsWebAssemblyLeptonXLiteThemeBundlingModule),
typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
typeof(AbpAspNetCoreSerilogModule))]
public class BilliardHallBlazorModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(typeof(BilliardHallResource),
            typeof(BilliardHallDomainModule).Assembly,
            typeof(BilliardHallDomainSharedModule).Assembly,
            typeof(BilliardHallApplicationModule).Assembly,
            typeof(BilliardHallApplicationContractsModule).Assembly,
            typeof(BilliardHallBlazorModule).Assembly);
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("BilliardHall");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx",
                configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }

        PreConfigure<AbpAspNetCoreComponentsWebOptions>(options =>
        {
            options.IsBlazorWebApp = true;
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Add services to the container.
        context
            .Services
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
        }

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureSwaggerServices(context.Services);
        ConfigureAutoApiControllers();
        ConfigureBlazorise(context);
        ConfigureRouter(context);
        ConfigureMenu(context);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults
            .AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]
                                                     ?.Split(',') ??
                                                 Array.Empty<string>());
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            // Blazor Web App
            options.Parameters.InteractiveAuto = true;

            // MVC UI
            options.StyleBundles.Configure(LeptonXLiteThemeBundles.Styles.Global,
            bundle =>
            {
                bundle.AddFiles("/global-styles.css");
            });

            options.ScriptBundles.Configure(LeptonXLiteThemeBundles.Scripts.Global,
            bundle =>
            {
                bundle.AddFiles("/global-scripts.js");
            });

            // Blazor UI
            options.StyleBundles.Configure(BlazorLeptonXLiteThemeBundles.Styles.Global,
            bundle =>
            {
                bundle.AddFiles("/global-styles.css");
            });
        });

        Configure<AbpBundlingOptions>(options =>
        {
            var globalStyles = options.StyleBundles.Get(BlazorWebAssemblyStandardBundles.Styles.Global);
            globalStyles.AddContributors(typeof(BilliardHallStyleBundleContributor));

            var globalScripts = options.ScriptBundles.Get(BlazorWebAssemblyStandardBundles.Scripts.Global);
            globalScripts.AddContributors(typeof(BilliardHallScriptBundleContributor));

        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<BilliardHallDomainSharedModule>(
                Path.Combine(hostingEnvironment.ContentRootPath,
                $"..{Path.DirectorySeparatorChar}Zss.BilliardHall.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<BilliardHallDomainModule>(
                Path.Combine(hostingEnvironment.ContentRootPath,
                $"..{Path.DirectorySeparatorChar}Zss.BilliardHall.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<BilliardHallApplicationContractsModule>(
                Path.Combine(hostingEnvironment.ContentRootPath,
                $"..{Path.DirectorySeparatorChar}Zss.BilliardHall.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<BilliardHallApplicationModule>(
                Path.Combine(hostingEnvironment.ContentRootPath,
                $"..{Path.DirectorySeparatorChar}Zss.BilliardHall.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<BilliardHallBlazorModule>(hostingEnvironment
                    .ContentRootPath);
            });
        }
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "BilliardHall API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }


    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        context
            .Services
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new BilliardHallMenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(BilliardHallBlazorModule).Assembly;
            options.AdditionalAssemblies.Add(typeof(BilliardHallBlazorClientModule).Assembly);
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(BilliardHallApplicationModule).Assembly);
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<BilliardHallBlazorModule>();
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();

        app.Use(async (ctx, next) =>
        {
            /* Converting to https to be able to include https URLs in `/.well-known/openid-configuration` endpoint.
             * This should only be done if the request is coming outside of the cluster.  */
            if (ctx.Request.Headers.ContainsKey("from-ingress"))
            {
                ctx.Request.Scheme = "https";
            }

            await next();
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseRouting();
        var configuration = context.GetConfiguration();
        if (Convert.ToBoolean(configuration["AuthServer:IsOnK8s"]))
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value != null &&
                    context.Request.Path.Value.StartsWith("/appsettings", StringComparison.OrdinalIgnoreCase) &&
                    context.Request.Path.Value.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    // Set endpoint to null so the static files middleware will handle the request.
                    context.SetEndpoint(null);
                }
                await next(context);
            });

            app.UseStaticFilesForPatterns("appsettings*.json");
        }
        app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAntiforgery();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "BilliardHall API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();

        app.UseConfiguredEndpoints(builder =>
        {
            builder
                .MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(builder
                    .ServiceProvider
                    .GetRequiredService<IOptions<AbpRouterOptions>>()
                    .Value
                    .AdditionalAssemblies
                    .ToArray());
        });
    }
}
