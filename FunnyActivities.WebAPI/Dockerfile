# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY FunnyActivities.sln ./
COPY FunnyActivities.WebAPI/*.csproj ./FunnyActivities.WebAPI/
COPY FunnyActivities.Application/*.csproj ./FunnyActivities.Application/
COPY FunnyActivities.Domain/*.csproj ./FunnyActivities.Domain/
COPY FunnyActivities.CrossCuttingConcerns/*.csproj ./FunnyActivities.CrossCuttingConcerns/
COPY FunnyActivities.Infrastructure/*.csproj ./FunnyActivities.Infrastructure/

# Restore dependencies
RUN dotnet restore FunnyActivities.WebAPI/FunnyActivities.WebAPI.csproj

# Copy everything else
COPY . ./

# Build and publish
WORKDIR /src/FunnyActivities.WebAPI
RUN dotnet build FunnyActivities.WebAPI.csproj -c Release -o /app/build
RUN dotnet publish FunnyActivities.WebAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Install Trivy for security scanning (optional, for CI/CD)
# RUN apk add --no-cache curl && \
#     curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sh -s -- -b /usr/local/bin

# Create non-root user
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

WORKDIR /app
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "FunnyActivities.WebAPI.dll"]

# Security note: Run Trivy scan in CI/CD pipeline
# Example: docker run --rm -v /path/to/trivy-cache:/root/.cache/trivy aquasec/trivy image your-image:tag