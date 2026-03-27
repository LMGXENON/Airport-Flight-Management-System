# API Integration Testing Guide

## Overview
This document provides guidelines for testing the flight management system API endpoints.

## Authentication
All API requests should include appropriate authentication headers.

## Endpoints

### Search Endpoints
- `GET /api/flight/search/{flightNumber}` - Search flights by flight number
- `GET /api/flight/airline/{airline}` - Search flights by airline
- `GET /api/flight/all` - Get all flights

### Expected Response Format
```json
[
  {
    "id": 1,
    "flightNumber": "AA101",
    "airline": "American Airlines",
    "destination": "New York",
    "departureTime": "2024-01-15T10:00:00Z",
    "arrivalTime": "2024-01-15T13:00:00Z",
    "terminal": 1,
    "status": "Scheduled"
  }
]
```

## Status Codes
- 200: Success
- 400: Bad Request
- 404: Not Found
- 500: Internal Server Error

## Testing Checklist
- [ ] Test with valid flight numbers
- [ ] Test with invalid flight numbers
- [ ] Test with special characters
- [ ] Test with empty results
- [ ] Test performance with large datasets
- [ ] Test concurrent requests
- [ ] Verify error handling

## Notes
All endpoints support filtering and pagination for improved performance.
