using FunnyActivities.CrossCuttingConcerns;
using FunnyActivities.CrossCuttingConcerns.APIDocumentation;
using FunnyActivities.Infrastructure;
using FunnyActivities.WebAPI.Extensions;
using FunnyActivities.WebAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Prometheus;
using Minio;
using Serilog;
using FunnyActivities.CrossCuttingConcerns.Logging;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVIS YAPILANDIRMASI (DEPENDENCY INJECTION) ---

// Serilog'u yapilandir
SerilogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// Controller'lari ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddControllers(builder.Services);  

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

// Özel yetkilendirme handler'larini kaydet
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddAuthorizationHandlers(builder.Services);

// API versiyonlamayi ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddApiVersioning(builder.Services);

// Swagger (API dokümantasyonu) ekle
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

// Dosya yükleme ayarlarini ekle
builder.Services.AddFileUploadConfiguration(builder.Configuration);

// Dosya yükleme servislerini ekle
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

// HttpContextAccessor'i ekle (Audit loglama gibi islemler için)
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHttpContextAccessor(builder.Services);

// Redis (caching için) ekle
builder.Services.AddRedis(builder.Configuration);


// --- 2. UYGULAMA VE MIDDLEWARE YAPILANDIRMASI ---

var app = builder.Build();

// Veritabani migration'larini uygula (sadece iliskisel veritabanlari için)
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
    app.UseSwaggerDocumentation(); // Özel Swagger extension metodunuz
}

// Hatalari merkezi olarak yakalamak için custom exception middleware'ini kullan
app.UseCustomExceptionHandling();

// Gelen istekleri HTTP'den HTTPS'e yönlendir
app.UseHttpsRedirection();

// Yönlendirme (Routing) middleware'ini ekle. Bu, istegin hangi endpoint'e gidecegini belirler.
app.UseRouting();

// Prometheus metriklerini toplamak için (UseRouting'den sonra gelmeli)
app.UseHttpMetrics();

// CORS middleware'ini ekle. Tarayicilarin farkli domain'lerden API'ye erisimine izin verir.
// Güvenlik middleware'larindan (Authentication/Authorization) önce gelmelidir.
app.UseCors("AllowAllOrigins"); // Development için tüm origin'lere izin ver

// Kimlik dogrulama (Authentication) middleware'ini ekle. Gelen JWT'yi dogrular.
app.UseAuthentication();
app.UseAuthenticationMiddleware(); // Sizin custom middleware'iniz

// Yetkilendirme (Authorization) middleware'ini ekle. [Authorize] attributelarini denetler.
app.UseAuthorization();

// Yanitlari önbellege almak için (istege bagli)
app.UseResponseCaching();

// Diger özel middleware'lariniz (rol dogrulama, loglama vb.)
app.UseMiddleware<RoleValidationMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

// Prometheus metrik sunucusunu endpoint olarak ekle
app.UseMetricServer();

// Controller endpoint'lerini haritala. Bu, gelen istegi dogru Controller Action'ina yönlendirir.
app.MapControllers();

// Health check endpoint'ini haritala
app.MapHealthChecks("/health");

// Uygulamayi çalistir
await app.RunAsync();


// Entegrasyon testleri için Program sinifini public yap
public partial class Program { }
