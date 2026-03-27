# Docker Deployment Guide

## Prerequisites
- Docker Desktop installed
- .NET 10.0 SDK
- Docker Compose

## Building Docker Image

```bash
docker build -t afms:latest .
```

## Running with Docker Compose

```bash
docker-compose up -d
```

## Services
- **Web Application**: http://localhost:5000
- **Database**: SQLite at `/app/flights.db`

## Environment Configuration

### Development
```bash
docker-compose -f docker-compose.yaml up -d
```

### Production
Update `appsettings.Production.json` before deployment.

## Troubleshooting

### Container won't start
```bash
docker logs <container_id>
```

### Database connection issues
Ensure volume mount is correct in docker-compose.yaml

### Port conflicts
Change port mappings in docker-compose.yaml

## Cleanup

```bash
docker-compose down -v
```

## Health Check
Application includes health check endpoint at `/health`
