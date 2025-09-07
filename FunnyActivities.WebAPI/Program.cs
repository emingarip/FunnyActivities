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

// Serilog'u yapılandır
SerilogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// Controller'ları ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddControllers(builder.Services);  

// Application Insights'ı yapılandır
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddApplicationInsights(builder.Services);

// HttpClient servislerini ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHttpClientServices(builder.Services);

// Health check'leri yapılandır
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHealthChecks(builder.Services, builder.Configuration);

// JWT ile kimlik doğrulamayı (Authentication) ekle
FunnyActivities.CrossCuttingConcerns.ServiceCollectionExtensions.AddJwtAuthentication(builder.Services, builder.Configuration);

// Yetkilendirme (Authorization) politikalarını ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddAuthorizationPolicies(builder.Services);

// Özel yetkilendirme handler'larını kaydet
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddAuthorizationHandlers(builder.Services);

// API versiyonlamayı ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddApiVersioning(builder.Services);

// Swagger (API dokümantasyonu) ekle
builder.Services.AddSwagger();

// CORS (Cross-Origin Resource Sharing) politikasını ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddCors(builder.Services);

// Veritabanı DbContext'ini ekle
builder.Services.AddDatabase(builder.Configuration);

// Repository'leri ekle
builder.Services.AddRepositories();

// MinIO client'ını ekle
builder.Services.AddMinio(builder.Configuration);

// Resim işleme ve MinIO servislerini ekle
builder.Services.AddImageProcessingServices();

// Dosya yükleme ayarlarını ekle
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

// MediatR'ı ekle
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddMediatR(builder.Services);

// HttpContextAccessor'ı ekle (Audit loglama gibi işlemler için)
FunnyActivities.WebAPI.Extensions.ServiceCollectionExtensions.AddHttpContextAccessor(builder.Services);

// Redis (caching için) ekle
builder.Services.AddRedis(builder.Configuration);


// --- 2. UYGULAMA VE MIDDLEWARE YAPILANDIRMASI ---

var app = builder.Build();

// Veritabanı migration'larını uygula (sadece ilişkisel veritabanları için)
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
        logger.LogError(ex, "Veritabanı migrate edilirken bir hata oluştu.");
        throw; // Uygulamanın başlamasını engelle
    }
}

// HTTP istek işleme hattını (pipeline) yapılandır
// Geliştirme ortamındaysa Swagger ve geliştirici hata sayfasını kullan
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(); // Özel Swagger extension metodunuz
}

// Hataları merkezi olarak yakalamak için custom exception middleware'ını kullan
app.UseCustomExceptionHandling();

// Gelen istekleri HTTP'den HTTPS'e yönlendir
app.UseHttpsRedirection();

// Yönlendirme (Routing) middleware'ını ekle. Bu, isteğin hangi endpoint'e gideceğini belirler.
app.UseRouting();

// Prometheus metriklerini toplamak için (UseRouting'den sonra gelmeli)
app.UseHttpMetrics();

// CORS middleware'ını ekle. Tarayıcıların farklı domain'lerden API'ye erişimine izin verir.
// Güvenlik middleware'larından (Authentication/Authorization) önce gelmelidir.
app.UseCors("AllowSpecificOrigins"); // "AllowSpecificOrigins" adıyla tanımladığınız politikayı kullanır

app.UseAuthentication(); // ← Bu eksik!

// Kimlik doğrulama (Authentication) middleware'ını ekle. Gelen JWT'yi doğrular.
app.UseAuthenticationMiddleware(); // Sizin custom middleware'ınız
// Not: Eğer .NET'in kendi JWT mekanizmasını kullanıyorsanız burada app.UseAuthentication(); da olmalı.

// Yetkilendirme (Authorization) middleware'ını ekle. [Authorize] attributelarını denetler.
app.UseAuthorization();

// Yanıtları önbelleğe almak için (isteğe bağlı)
app.UseResponseCaching();

// Diğer özel middleware'larınız (rol doğrulama, loglama vb.)
app.UseMiddleware<RoleValidationMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

// Prometheus metrik sunucusunu endpoint olarak ekle
app.UseMetricServer();

// Controller endpoint'lerini haritala. Bu, gelen isteği doğru Controller Action'ına yönlendirir.
app.MapControllers();

// Uygulamayı çalıştır
await app.RunAsync();


// Entegrasyon testleri için Program sınıfını public yap
public partial class Program { }