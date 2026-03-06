using System;
using FunnyActivities.Application.AI;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.CrossCuttingConcerns;
using FunnyActivities.CrossCuttingConcerns.APIDocumentation;
using FunnyActivities.CrossCuttingConcerns.CORS;
using FunnyActivities.CrossCuttingConcerns.ErrorHandling;
using FunnyActivities.CrossCuttingConcerns.HealthMonitoring;
using FunnyActivities.CrossCuttingConcerns.Caching;
using FunnyActivities.CrossCuttingConcerns.FileUpload;
using FunnyActivities.CrossCuttingConcerns.FileUpload.Configuration;
using FunnyActivities.Infrastructure;
using FunnyActivities.Infrastructure.Repositories;
using FunnyActivities.Infrastructure.Services;
using FunnyActivities.Infrastructure.Services.AI;
using FunnyActivities.Infrastructure.Services.Settings;
using FunnyActivities.Infrastructure.Services.Prompts;
using FunnyActivities.WebAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using MediatR;
using StackExchange.Redis;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Minio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using FluentValidation;

namespace FunnyActivities.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationInsights(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        return services;
    }

    public static IServiceCollection AddControllers(this IServiceCollection services)
    {
        Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions.AddControllers(services)
            .AddDataAnnotationsLocalization()
            .AddViewLocalization();
        return services;
    }

    public static IServiceCollection AddHttpClientServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        return services;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // Register health check services
        services.AddScoped<DatabaseHealthCheck>(sp => new DatabaseHealthCheck(
            configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required"),
            sp.GetRequiredService<ILogger<DatabaseHealthCheck>>()));
        services.AddScoped<SendGridHealthCheck>();
        services.AddScoped<TwilioHealthCheck>();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy, new[] { "database" })
            .AddRedis(configuration.GetConnectionString("Redis") ?? "localhost:6379", "redis", HealthStatus.Unhealthy, new[] { "cache" })
            .AddCheck<SendGridHealthCheck>("sendgrid", HealthStatus.Degraded, new[] { "external" })
            .AddCheck<TwilioHealthCheck>("twilio", HealthStatus.Degraded, new[] { "external" });
        return services;
    }



    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Existing policy
            options.AddPolicy("AdminOnly", policy =>
                policy.Requirements.Add(new FunnyActivities.CrossCuttingConcerns.Authorization.AdminRequirement("Admin")));

            // BaseProduct policies
            options.AddPolicy("CanCreateBaseProduct", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateBaseProduct", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteBaseProduct", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanViewBaseProduct", policy =>
                policy.RequireRole("Admin", "Viewer"));

            // ProductVariant policies
            options.AddPolicy("CanCreateProductVariant", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateProductVariant", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteProductVariant", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanViewProductVariant", policy =>
                policy.RequireRole("Admin", "Viewer"));

            // UnitOfMeasure policies
            options.AddPolicy("CanCreateUnit", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateUnit", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteUnit", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanViewUnit", policy =>
                policy.RequireRole("Admin", "Viewer"));

            // ShoppingCart policies
            options.AddPolicy("CanCreateShoppingCartItem", policy =>
                policy.RequireRole("Admin", "User"));
            options.AddPolicy("CanUpdateShoppingCartItem", policy =>
                policy.RequireRole("Admin", "User"));
            options.AddPolicy("CanDeleteShoppingCartItem", policy =>
                policy.RequireRole("Admin", "User"));
            options.AddPolicy("CanViewShoppingCart", policy =>
                policy.RequireRole("Admin", "User", "Viewer"));
            // Category Policies
            options.AddPolicy("CanViewCategory", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanCreateCategory", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateCategory", policy =>
               policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteCategory", policy =>
               policy.RequireRole("Admin"));

            // Activity Policies
            options.AddPolicy("CanViewActivity", policy =>
                policy.RequireRole("Admin", "Viewer"));
            options.AddPolicy("CanCreateActivity", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateActivity", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteActivity", policy =>
                policy.RequireRole("Admin"));

            // ActivityCategory Policies
            options.AddPolicy("CanViewActivityCategory", policy =>
                policy.RequireRole("Admin", "Viewer"));
            options.AddPolicy("CanCreateActivityCategory", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateActivityCategory", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteActivityCategory", policy =>
                policy.RequireRole("Admin"));

            // Step Policies
            options.AddPolicy("CanViewStep", policy =>
                policy.RequireRole("Admin", "Viewer"));
            options.AddPolicy("CanCreateStep", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateStep", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteStep", policy =>
                policy.RequireRole("Admin"));

            // ActivityProductVariant Policies
            options.AddPolicy("CanViewActivityProductVariant", policy =>
                policy.RequireRole("Admin", "Viewer"));
            options.AddPolicy("CanCreateActivityProductVariant", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateActivityProductVariant", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteActivityProductVariant", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("CanMigrateData", policy =>
                policy.RequireRole("Admin"));

            // User management policies
            options.AddPolicy("Admin", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireRole("Admin"));

            // Survey management policies
            options.AddPolicy("CanViewSurvey", policy =>
                policy.RequireRole("Admin", "Viewer"));
            options.AddPolicy("CanCreateSurvey", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanUpdateSurvey", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanDeleteSurvey", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanViewSurveyResults", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("CanViewSurveyStatistics", policy =>
                policy.RequireRole("Admin"));

            // Enhanced survey-specific authorization policies
            options.AddPolicy("SurveyAdmin", policy =>
                policy.RequireRole("Admin")
                      .RequireAssertion(context =>
                      {
                          // Additional admin validation logic can be added here
                          return context.User.IsInRole("Admin");
                      }));

            options.AddPolicy("SurveyViewer", policy =>
                policy.RequireRole("Admin", "Viewer")
                      .RequireAssertion(context =>
                      {
                          // Additional viewer validation logic can be added here
                          return context.User.IsInRole("Admin") || context.User.IsInRole("Viewer");
                      }));

            options.AddPolicy("PublicSurveyAccess", policy =>
                policy.RequireAssertion(context =>
                {
                    // Allow both authenticated and anonymous users for public surveys
                    return !context.User.Identity.IsAuthenticated ||
                           context.User.IsInRole("Admin") ||
                           context.User.IsInRole("Viewer") ||
                           context.User.IsInRole("User");
                }));

            // Require authenticated users by default for [Authorize] endpoints.
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Keep fallback open so only endpoints explicitly decorated with
            // authorization metadata are protected.
            options.FallbackPolicy = null;
        });
        return services;
    }

    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, FunnyActivities.CrossCuttingConcerns.Authorization.AdminRequirementHandler>();
        return services;
    }

    public static IServiceCollection AddApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerDocumentation();
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (configuredOrigins is { Length: > 0 })
        {
            services.AddCustomCors(configuredOrigins);
            return services;
        }

        services.AddCustomCors(new[] {
            "http://localhost:3000",
            "https://localhost:3000",
            "http://localhost:3001",
            "https://localhost:3001",
            "http://localhost:8080",
            "https://localhost:8080"
        });
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<FunnyActivities.Application.Interfaces.IUserRepository, UserRepository>();
        services.AddScoped<FunnyActivities.Domain.Interfaces.IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IBaseProductRepository, BaseProductRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<FunnyActivities.Application.Interfaces.ICategoryRepository, FunnyActivities.Infrastructure.CategoryRepository>();
        services.AddScoped<IMaterialRepository, FunnyActivities.Infrastructure.MaterialRepository>();
        services.AddScoped<IShoppingCartItemRepository, ShoppingCartItemRepository>();

        // Activity repositories
        services.AddScoped<FunnyActivities.Application.Interfaces.IActivityRepository, ActivityRepository>();
        services.AddScoped<FunnyActivities.Application.Interfaces.IActivityCategoryRepository, ActivityCategoryRepository>();
        services.AddScoped<FunnyActivities.Application.Interfaces.IStepRepository, StepRepository>();
        services.AddScoped<FunnyActivities.Application.Interfaces.IActivityProductVariantRepository, ActivityProductVariantRepository>();

        // Favorites repository
        services.AddScoped<FunnyActivities.Application.Interfaces.IFavoritesRepository, FunnyActivities.Infrastructure.FavoritesRepository>();

        // Survey repositories
        services.AddScoped<FunnyActivities.Domain.Interfaces.ISurveyRepository, FunnyActivities.Infrastructure.SurveyRepository>();
        services.AddScoped<FunnyActivities.Domain.Interfaces.IVoteRepository, FunnyActivities.Infrastructure.VoteRepository>();

        // Persona repositories
        services.AddScoped<FunnyActivities.Domain.Interfaces.IPersonaRepository, FunnyActivities.Infrastructure.PersonaRepository>();
        services.AddScoped<FunnyActivities.Domain.Interfaces.IPersonaActivityAssociationRepository, FunnyActivities.Infrastructure.PersonaActivityAssociationRepository>();

        // Prompt template repositories
        services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
        services.AddScoped<IPromptCallLogRepository, PromptCallLogRepository>();

        return services;
    }

    public static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        // Check if MinIO is properly configured
        var minioEndpoint = configuration["MinIO:Endpoint"];
        var minioAccessKey = configuration["MinIO:AccessKey"];
        var minioSecretKey = configuration["MinIO:SecretKey"];

        // If MinIO is not configured, register null client
        if (string.IsNullOrEmpty(minioEndpoint) || string.IsNullOrEmpty(minioAccessKey) || string.IsNullOrEmpty(minioSecretKey))
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<Program>>();
            logger?.LogWarning("MinIO configuration is incomplete. MinIO functionality will be disabled.");

            services.AddSingleton<IMinioClient>(sp => null);
            return services;
        }

        var minioConfig = new FunnyActivities.Infrastructure.Services.MinioConfiguration
        {
            Endpoint = minioEndpoint,
            ExternalEndpoint = configuration["MinIO:ExternalEndpoint"] ?? minioEndpoint,
            AccessKey = minioAccessKey,
            SecretKey = minioSecretKey,
            UseSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false"),
            Region = configuration["MinIO:Region"] ?? "us-east-1",
            CorsOrigins = configuration.GetSection("MinIO:CorsOrigins").Get<string[]>() ?? new[] {
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3001",
                "http://localhost:8080",
                "https://localhost:8080"
            }
        };

        try
        {
            minioConfig.Validate();

            services.AddSingleton<IMinioClient>(sp =>
                new MinioClient()
                    .WithEndpoint(minioConfig.Endpoint)
                    .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey)
                    .WithRegion(minioConfig.Region)
                    .WithSSL(minioConfig.UseSSL)
                    .Build());
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the application startup
            var logger = services.BuildServiceProvider().GetService<ILogger<Program>>();
            logger?.LogWarning(ex, "Failed to configure MinIO service. MinIO functionality will be disabled.");

            // Register a null MinIO client to indicate MinIO is not available
            services.AddSingleton<IMinioClient>(sp => null);
        }

        return services;
    }

    public static IServiceCollection AddImageProcessingServices(this IServiceCollection services)
    {
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        services.AddScoped<IMinioService, MinioService>(sp =>
            new MinioService(
                sp.GetRequiredService<IMinioClient>(),
                sp.GetRequiredService<ApplicationDbContext>(),
                sp.GetRequiredService<MinioConfiguration>(),
                sp.GetRequiredService<ILogger<MinioService>>()));

        // Register MinioConfiguration
        services.AddSingleton<MinioConfiguration>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var minioEndpoint = configuration["MinIO:Endpoint"];
            var minioAccessKey = configuration["MinIO:AccessKey"];
            var minioSecretKey = configuration["MinIO:SecretKey"];

            if (string.IsNullOrEmpty(minioEndpoint) || string.IsNullOrEmpty(minioAccessKey) || string.IsNullOrEmpty(minioSecretKey))
            {
                throw new InvalidOperationException("MinIO configuration is incomplete");
            }

            return new MinioConfiguration
            {
                Endpoint = minioEndpoint,
                ExternalEndpoint = configuration["MinIO:ExternalEndpoint"] ?? minioEndpoint,
                AccessKey = minioAccessKey,
                SecretKey = minioSecretKey,
                UseSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false"),
                Region = configuration["MinIO:Region"] ?? "us-east-1",
                CorsOrigins = configuration.GetSection("MinIO:CorsOrigins").Get<string[]>() ?? new[] {
                    "http://localhost:3000",
                    "https://localhost:3000",
                    "http://localhost:3001",
                    "https://localhost:3001",
                    "http://localhost:8080",
                    "https://localhost:8080"
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddFileUploadConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FileUploadConfiguration>(sp =>
        {
            var config = new FileUploadConfiguration();
            configuration.GetSection("FileUpload").Bind(config);
            config.Validate();
            return config;
        });
        return services;
    }

    public static IServiceCollection AddFileUploadServices(this IServiceCollection services)
    {
        services.AddScoped<IStorageProvider, MinioStorageProvider>();
        services.AddScoped<IFileValidator, BasicFileValidator>();
        services.AddScoped<IFileProcessor, BasicFileProcessor>();
        return services;
    }

    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var smtpSection = configuration.GetSection("Smtp");
        var smtpHost = smtpSection["Host"];
        var smtpUser = smtpSection["Username"] ?? smtpSection["User"];

        if (!string.IsNullOrWhiteSpace(smtpHost) && !string.IsNullOrWhiteSpace(smtpUser))
        {
            services.Configure<SmtpOptions>(smtpSection);
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else
        {
            var sendGridApiKey = configuration["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(sendGridApiKey))
            {
                throw new InvalidOperationException("Email provider not configured. Set Smtp or SendGrid settings.");
            }
            services.AddScoped<IEmailService>(_ => new SendGridEmailService(sendGridApiKey));
        }

        services.AddScoped<ISmsService>(sp =>
            new TwilioSmsService(
                configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured"),
                configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured"),
                configuration["Twilio:FromNumber"] ?? throw new InvalidOperationException("Twilio FromNumber not configured")
            ));
        return services;
    }

    public static IServiceCollection AddComplianceServices(this IServiceCollection services)
    {
        services.AddScoped<FunnyActivities.CrossCuttingConcerns.Security.DataAnonymizationService>();
        services.AddSingleton<FunnyActivities.CrossCuttingConcerns.Security.ConsentService>();
        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<FunnyActivities.Domain.Services.UserService>();
        services.AddScoped<FunnyActivities.Application.Services.GoogleLoginService>();

        // Survey services
        services.AddScoped<FunnyActivities.Application.Services.ISurveyService, FunnyActivities.Application.Services.SurveyService>();
        services.AddScoped<FunnyActivities.Application.Services.IVotingService, FunnyActivities.Application.Services.VotingService>();

        // Input sanitization service
        services.AddScoped<FunnyActivities.Application.Interfaces.IInputSanitizer, FunnyActivities.Infrastructure.Services.InputSanitizer>();

        // AI services
        services.AddMemoryCache();
        services.Configure<LlmOptions>(configuration.GetSection("Llm"));
        services.AddHttpClient<OllamaLlmProvider>();
        services.AddHttpClient<OpenAiChatProvider>();
        services.AddTransient<ILlmProvider>(sp => sp.GetRequiredService<OllamaLlmProvider>());
        services.AddTransient<ILlmProvider>(sp => sp.GetRequiredService<OpenAiChatProvider>());
        services.AddScoped<IAIService, MultiProviderAiService>();
        services.AddScoped<ILlmSettingsRepository, LlmSettingsRepository>();
        services.AddScoped<ILlmSettingsService, LlmSettingsService>();
        services.AddScoped<ILlmSettingsInitializer, LlmSettingsInitializer>();
        services.AddScoped<IPromptTemplateService, PromptTemplateService>();

        return services;
    }

    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton<FunnyActivities.CrossCuttingConcerns.Logging.SecurityEventLogger>();
        return services;
    }

    public static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FunnyActivities.Application.Commands.UserManagement.RegisterUserCommand).Assembly));

        // Add FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<FunnyActivities.Application.Validators.ActivityManagement.CreateStepCommandValidator>();

        // Register pipeline behaviors (order matters: validation first, then authorization)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FunnyActivities.Application.Behaviors.ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FunnyActivities.Application.Behaviors.AuthorizationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddHttpContextAccessor(this IServiceCollection services)
    {
        Microsoft.Extensions.DependencyInjection.HttpServiceCollectionExtensions.AddHttpContextAccessor(services);
        return services;
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        // Configure Redis connection with advanced options
        var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
        configurationOptions.AbortOnConnectFail = false; // Don't fail on connection issues
        configurationOptions.ConnectRetry = 3; // Retry connection attempts
        configurationOptions.ConnectTimeout = 5000; // 5 second timeout
        configurationOptions.SyncTimeout = 5000; // 5 second sync timeout
        configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000); // Exponential backoff
        configurationOptions.KeepAlive = 60; // Keep alive ping every 60 seconds

        // Enable connection pooling
        configurationOptions.EndPoints.Clear();
        var endpoints = redisConnectionString.Split(',');
        foreach (var endpoint in endpoints)
        {
            configurationOptions.EndPoints.Add(endpoint.Trim());
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            try
            {
                var multiplexer = ConnectionMultiplexer.Connect(configurationOptions);

                // Log connection events
                multiplexer.ConnectionFailed += (sender, args) =>
                {
                    logger.LogError("Redis connection failed: {FailureType} - {Exception}",
                        args.FailureType, args.Exception?.Message);
                };

                multiplexer.ConnectionRestored += (sender, args) =>
                {
                    logger.LogInformation("Redis connection restored: {FailureType}",
                        args.FailureType);
                };

                multiplexer.ErrorMessage += (sender, args) =>
                {
                    logger.LogWarning("Redis error: {Message}", args.Message);
                };

                return multiplexer;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
                // Return a null multiplexer or throw depending on requirements
                throw;
            }
        });

        // Register distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "FunnyActivities:";
        });

        // Register our custom cache service
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
