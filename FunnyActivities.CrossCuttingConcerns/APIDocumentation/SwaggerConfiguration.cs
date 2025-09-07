using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FunnyActivities.CrossCuttingConcerns.APIDocumentation;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FunnyActivities API",
                Version = "v1",
                Description = "A comprehensive API for managing materials in the FunnyActivities system. " +
                             "Supports CRUD operations, bulk operations, advanced filtering, and role-based access control.",
                Contact = new OpenApiContact
                {
                    Name = "FunnyActivities Support",
                    Email = "support@funnyactivities.com"
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Include XML comments from other assemblies
            var applicationXml = "FunnyActivities.Application.xml";
            var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
            if (File.Exists(applicationXmlPath))
            {
                c.IncludeXmlComments(applicationXmlPath);
            }

            var domainXml = "FunnyActivities.Domain.xml";
            var domainXmlPath = Path.Combine(AppContext.BaseDirectory, domainXml);
            if (File.Exists(domainXmlPath))
            {
                c.IncludeXmlComments(domainXmlPath);
            }

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field. Example: Bearer {token}",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

            // Configure Swagger to handle file uploads
            c.OperationFilter<FileUploadOperationFilter>();
            c.OperationFilter<AuthorizeOperationFilter>();
            c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });

            // Group endpoints by controller
            c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            c.DocInclusionPredicate((name, api) => true);

            // Customize schemaId generation to include full namespace for uniqueness
            c.CustomSchemaIds(type => type.FullName);
        });

        return services;
    }

    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Type == typeof(IFormFile) ||
                           (p.Type.IsGenericType &&
                            p.Type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                            p.Type.GetGenericArguments()[0] == typeof(IFormFile)))
                .ToList();

            var formParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Source == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Form)
                .ToList();

            if (formParameters.Any())
            {
                // Clear existing parameters since we're handling them in the request body
                operation.Parameters.Clear();

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>()
                            }
                        }
                    }
                };

                foreach (var parameter in formParameters)
                {
                    if (parameter.Type == typeof(IFormFile) ||
                        (parameter.Type.IsGenericType &&
                         parameter.Type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                         parameter.Type.GetGenericArguments()[0] == typeof(IFormFile)))
                    {
                        // Handle IFormFile parameters as binary files
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[parameter.Name] =
                            new OpenApiSchema { Type = "string", Format = "binary" };
                    }
                    else
                    {
                        // For complex objects, add their properties directly to the schema
                        var schema = context.SchemaGenerator.GenerateSchema(parameter.Type, context.SchemaRepository);
                        if (schema.Properties != null)
                        {
                            foreach (var property in schema.Properties)
                            {
                                operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Key] = property.Value;
                            }
                        }
                    }
                }
            }
        }
    }

    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();

            if (!authorizeAttributes.Any())
            {
                // Check if the controller has Authorize attribute
                authorizeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .ToList() ?? new List<AuthorizeAttribute>();
            }

            if (authorizeAttributes.Any())
            {
                // Check for existing response keys before adding to prevent duplicate key exceptions
                if (!operation.Responses.ContainsKey("401"))
                {
                    operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                }
                if (!operation.Responses.ContainsKey("403"))
                {
                    operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
                }

                var policy = authorizeAttributes.FirstOrDefault(a => !string.IsNullOrEmpty(a.Policy))?.Policy;
                if (!string.IsNullOrEmpty(policy))
                {
                    operation.Summary += $" (Requires: {policy})";
                }
                else
                {
                    operation.Summary += " (Requires Authentication)";
                }
            }
        }
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FunnyActivities API v1");
        });

        return app;
    }
}