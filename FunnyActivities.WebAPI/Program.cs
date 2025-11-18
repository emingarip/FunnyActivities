using FunnyActivities.CrossCuttingConcerns;
using FunnyActivities.CrossCuttingConcerns.APIDocumentation;
using FunnyActivities.CrossCuttingConcerns.Logging;
using FunnyActivities.Infrastructure;
using FunnyActivities.WebAPI.Extensions;
using FunnyActivities.WebAPI.Middleware;
using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Minio;
using Prometheus;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVIS YAPILANDIRMASI (DEPENDENCY INJECTION) ---

// Serilog'u yapilandir
SerilogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// Controller'lari ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddControllers(builder.Services);
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var localizationSection = builder.Configuration.GetSection("Localization");
var defaultCulture = localizationSection["DefaultCulture"] ?? "tr-TR";
var supportedCultureCodes = localizationSection.GetSection("SupportedCultures").Get<string[]>() ?? new[] { "tr-TR", "en-US" };
var supportedCultures = supportedCultureCodes.Select(code => new CultureInfo(code)).ToList();

// Application Insights'i yapilandir
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddApplicationInsights(builder.Services);

// HttpClient servislerini ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHttpClientServices(builder.Services);

// Health check'leri yapilandir
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHealthChecks(builder.Services, builder.Configuration);

// JWT ile kimlik dogrulamayi (Authentication) ekle
FunnyActivities.CrossCuttingConcerns.ServiceCollectionExtensions.AddJwtAuthentication(builder.Services, builder.Configuration);

// Yetkilendirme (Authorization) politikalarini ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddAuthorizationPolicies(builder.Services);

// �zel yetkilendirme handler'larini kaydet
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddAuthorizationHandlers(builder.Services);

// API versiyonlamayi ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddApiVersioning(builder.Services);

// Swagger (API dokumantasyonu) ekle
builder.Services.AddSwagger();

// CORS (Cross-Origin Resource Sharing) politikasini ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddCors(builder.Services);

// Veritabani DbContext'ini ekle
builder.Services.AddDatabase(builder.Configuration);

// Repository'leri ekle
builder.Services.AddRepositories();

// MinIO client'ini ekle
builder.Services.AddMinio(builder.Configuration);

// Resim isleme ve MinIO servislerini ekle
builder.Services.AddImageProcessingServices();

// Dosya y�kleme ayarlarini ekle
builder.Services.AddFileUploadConfiguration(builder.Configuration);

// Dosya y�kleme servislerini ekle
builder.Services.AddFileUploadServices();

// Bildirim servislerini ekle
builder.Services.AddNotificationServices(builder.Configuration);

// Uyumluluk servislerini ekle
builder.Services.AddComplianceServices();

// Domain servislerini ekle
builder.Services.AddDomainServices();

// Loglama servislerini ekle
builder.Services.AddLoggingServices();

// MediatR'i ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddMediatR(builder.Services);

// HttpContextAccessor'i ekle (Audit loglama gibi islemler icin)
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHttpContextAccessor(builder.Services);

// Redis (caching icin) ekle
builder.Services.AddRedis(builder.Configuration);


// --- 2. UYGULAMA VE MIDDLEWARE YAPILANDIRMASI ---

var app = builder.Build();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true,
    FallBackToParentCultures = true,
    FallBackToParentUICultures = true
};
localizationOptions.SetDefaultCulture(defaultCulture);
app.UseRequestLocalization(localizationOptions);

// Veritabani migration'larini uygula (sadece iliskisel veritabanlari icin)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabani migrate edilirken bir hata olustu.");
        throw; // Uygulamanin baslamasini engelle
    }
}

// HTTP istek isleme hattini (pipeline) yapilandir
// Gelistirme ortamindaysa Swagger ve gelistirici hata sayfasini kullan
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(); // �zel Swagger extension metodunuz
}

// Hatalari merkezi olarak yakalamak icin custom exception middleware'ini kullan
app.UseCustomExceptionHandling();

// Gelen istekleri HTTP'den HTTPS'e yonlendir
app.UseHttpsRedirection();

// Yonlendirme (Routing) middleware'ini ekle. Bu, istegin hangi endpoint'e gidecegini belirler.
app.UseRouting();

// Prometheus metriklerini toplamak icin (UseRouting'den sonra gelmeli)
app.UseHttpMetrics();

// CORS middleware'ini ekle. Tarayicilarin farkli domain'lerden API'ye erisimine izin verir.
// Guvenlik middleware'larindan (Authentication/Authorization) once gelmelidir.
app.UseCors("AllowAllOrigins"); // Development icin tum origin'lere izin ver

// Kimlik dogrulama (Authentication) middleware'ini ekle. Gelen JWT'yi dogrular.
app.UseAuthentication();
app.UseAuthenticationMiddleware(); // Sizin custom middleware'iniz

// Yetkilendirme (Authorization) middleware'ini ekle. [Authorize] attributelarini denetler.
app.UseAuthorization();

// Yanitlari onbellege almak icin (istege bagli)
app.UseResponseCaching();

// Diger ozel middleware'lariniz (rol dogrulama, loglama vb.)
app.UseMiddleware<RoleValidationMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

// Prometheus metrik sunucusunu endpoint olarak ekle
app.UseMetricServer();

// Controller endpoint'lerini haritala. Bu, gelen istegi dogru Controller Action'ina yonlendirir.
app.MapControllers();

// Health check endpoint'ini haritala
app.MapHealthChecks("/health");

// Uygulamayi calistir
await app.RunAsync();


// Entegrasyon testleri icin Program sinifini public yap
public partial class Program { }
