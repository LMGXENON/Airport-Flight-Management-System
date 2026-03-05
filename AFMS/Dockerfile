# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

# Copy project file
COPY ["AFMS.csproj", "./"]

# Restore dependencies
RUN dotnet restore "AFMS.csproj"

# Copy remaining source code
COPY . .

# Build the application in Release mode
RUN dotnet build "AFMS.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish

RUN dotnet publish "AFMS.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# Install runtime dependencies (if needed for SQLite)
RUN apk add --no-cache tzdata libssl3 libcrypto3

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup && \
    chown -R appuser:appgroup /app

USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

EXPOSE 8080
EXPOSE 8443

# Set environment for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080;https://+:8443
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "AFMS.dll"]
