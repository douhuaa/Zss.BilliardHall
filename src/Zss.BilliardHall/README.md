# Zss.BilliardHall

## About this solution

This is a layered startup solution based on [Domain Driven Design (DDD)](https://abp.io/docs/latest/framework/architecture/domain-driven-design) practises. All the fundamental ABP modules are already installed. Check the [Application Startup Template](https://abp.io/docs/latest/solution-templates/layered-web-application) documentation for more info.

### Pre-requirements

* [.NET9.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)

### Configurations

The solution comes with a default configuration that works out of the box. However, you may consider to change the following configuration before running your solution:


### Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies. This step is automatically done when you create a new solution, if you didn't especially disabled it. However, you should run it yourself if you have first cloned this solution from your source control, or added a new client-side package dependency to your solution.
* Run `Zss.BilliardHall.DbMigrator` to create the initial database. This step is also automatically done when you create a new solution, if you didn't especially disabled it. This should be done in the first run. It is also needed if a new database migration is added to the solution later.

#### Generating a Signing Certificate

In the production environment, you need to use a production signing certificate. ABP Framework sets up signing and encryption certificates in your application and expects an `openiddict.pfx` file in your application.

To generate a signing certificate, you can use the following command:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p 18b8d840-32f5-4f5a-9aa2-4e2cf1620a97
```

> `18b8d840-32f5-4f5a-9aa2-4e2cf1620a97` is the password of the certificate, you can change it to any password you want.

It is recommended to use **two** RSA certificates, distinct from the certificate(s) used for HTTPS: one for encryption, one for signing.

For more information, please refer to: [OpenIddict Certificate Configuration](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)

> Also, see the [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment) documentation for more information.

### Solution structure

This is a layered monolith application that consists of the following applications:

* `Zss.BilliardHall.DbMigrator`: A console application which applies the migrations and also seeds the initial data. It is useful on development as well as on production environment.

### Distributed Application (Aspire AppHost)

The repository contains an `.AppHost` project leveraging .NET Aspire to orchestrate Postgres, the DbMigrator and the HttpApi Host. Typical local workflow:

1. (Optional) export database credential env variables:

```powershell
$env:BH_DB_USER="billiard_dev"
$env:BH_DB_PASSWORD="DevPassword123!"
```

1. Start the host:

```powershell
dotnet run --project src/Zss.BilliardHall.AppHost/Zss.BilliardHall.AppHost.csproj
```
1. Aspire dashboard will show resources (Postgres container, migrations, api).

Connection string in `HttpApi.Host/appsettings.json` uses placeholders:

```json
{
	"ConnectionStrings": {
		"Default": "Host=localhost;Port=5432;Database=BilliardHall;Username=${BH_DB_USER};Password=${BH_DB_PASSWORD};"
	}
}
```

If env vars are not set, container defaults are used (development only).

### Observability (Serilog + OpenTelemetry)

Logging is performed via Serilog. If you set `BH_OTEL_EXPORTER_OTLP_ENDPOINT` the application will add OpenTelemetry (traces, metrics, logs) export to the specified OTLP collector.

Example:

```powershell
$env:BH_OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
```

### Health Checks

Health endpoints are registered:

* `/healthz` (liveness)
* `/hc` (combined)
* `/healthchecks-ui` (UI - in‑memory storage for local use)

In production you should protect UI endpoint (reverse proxy auth / network ACL). See `ServiceDefaults` project for registration code.

### CORS

Development override located in `HttpApi.Host/appsettings.Development.json`:

```json
{
	"App": {
		"CorsOrigins": "https://localhost:3000"
	}
}
```
Multiple origins are comma separated. For production restrict to trusted frontends only.

### Authentication / OpenIddict

Current seeding registers a SPA public client and (temporarily) enables password + client_credentials grants for bootstrap convenience. A deprecation note exists in the seeder; future hardening PR will remove password grant and rotate secrets.

### Secrets Management

See `docs/08_配置管理/Secrets管理.md` for full guidelines. Never commit real credentials. Prefer User Secrets locally and a vault in production.

### Quick Start (Developer)

```powershell
# Install JS libs (if using ABP UI packages)
abp install-libs

# Apply migrations (optional if starting through AppHost which runs migrator)
dotnet run --project src/Zss.BilliardHall/src/Zss.BilliardHall.DbMigrator/Zss.BilliardHall.DbMigrator.csproj

# Run distributed application
dotnet run --project src/Zss.BilliardHall.AppHost/Zss.BilliardHall.AppHost.csproj
```

Navigate to the HttpApi host Swagger UI (port assigned by Aspire, typically shown in dashboard). Use seeded admin credentials to sign in if required.

### Roadmap (Security / Ops Hardening)

* Split DB accounts (migration vs runtime)
* Remove password grant
* Protect health UI & PII logging off by default in production
* Introduce structured audit log export via OTLP


## Deploying the application

Deploying an ABP application follows the same process as deploying any .NET or ASP.NET Core application. However, there are important considerations to keep in mind. For detailed guidance, refer to ABP's [deployment documentation](https://abp.io/docs/latest/Deployment/Index).

### Additional resources

You can see the following resources to learn more about your solution and the ABP Framework:

* [Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [Application Startup Template](https://abp.io/docs/latest/startup-templates/application/index)
