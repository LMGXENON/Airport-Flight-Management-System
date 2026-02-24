# Real-Time Flight Tracking Feature

## Overview

The Airport Flight Management System now supports **real-time flight tracking** with live updates powered by SignalR and AeroDataBox API integration. Flight data is automatically synchronized every 2 minutes and pushed to all connected clients instantly.

## Features

### ✨ Real-Time Updates
- **Live Flight Status**: Automatic updates for flight status changes (On-Time, Delayed, Boarding, Departed, etc.)
- **Gate Changes**: Real-time notifications when gate assignments change
- **New Flights**: Automatically adds newly scheduled flights to the dashboard
- **Visual Feedback**: Animated highlights when flights are updated

### 🔄 Automatic Synchronization
- Background service polls AeroDataBox API every 2 minutes
- Syncs with configured airport (default: EGLL - London Heathrow)
- Updates existing flights and adds new ones automatically
- Efficient change detection - only modified flights trigger notifications

### 🎯 Connection Status
- Live connection indicator in top-right corner
- Shows connection status (Connected/Reconnecting)
- Automatic reconnection on network issues
- Pulse animation indicates active connection

### 📊 Manual Refresh
- Refresh button for on-demand data updates
- Instantly fetches latest flight information
- Visual feedback during refresh operation

## Technical Architecture

### Backend Components

#### 1. **FlightHub** (`Hubs/FlightHub.cs`)
- SignalR hub for real-time communication
- Manages client connections and groups
- Broadcasts flight updates to subscribed clients

#### 2. **FlightSyncService** (`Services/FlightSyncService.cs`)
- Synchronizes AeroDataBox data with local database
- Detects changes and triggers SignalR notifications
- Handles flight updates and new flight additions

#### 3. **FlightUpdateBackgroundService** (`BackgroundServices/FlightUpdateBackgroundService.cs`)
- Hosted background service
- Runs every 2 minutes (configurable)
- Automatically calls FlightSyncService

#### 4. **Enhanced AeroDataBoxService** (`Services/AeroDataBoxService.cs`)
- Fetches flight data from AeroDataBox API
- Handles 12-hour windows for large date ranges
- Supports both departures and arrivals

### Frontend Components

#### 1. **flight-tracking.js**
- SignalR client implementation
- Handles real-time flight updates
- DOM manipulation for live table updates
- Connection management and error handling
- Manual refresh functionality

#### 2. **flight-tracking.css**
- Smooth animations for flight updates
- Connection status indicator styling
- Toast notifications
- Pulse effects for active statuses

## Configuration

### appsettings.json
```json
{
  "AeroDataBox": {
    "ApiKey": "your-api-key-here",
    "ApiHost": "aerodatabox.p.rapidapi.com",
    "DefaultAirport": "EGLL"
  }
}
```

**Configuration Options:**
- `ApiKey`: Your RapidAPI key for AeroDataBox
- `ApiHost`: AeroDataBox API endpoint
- `DefaultAirport`: ICAO code for the airport to track (e.g., EGLL for London Heathrow)

### Update Interval
Modify the update interval in `FlightUpdateBackgroundService.cs`:
```csharp
private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(2);
```

## Usage

### For Users

1. **Navigate to Flight Index**: Go to `/Flight/Index`
2. **Connection Indicator**: Check the status indicator in the top-right corner
3. **Automatic Updates**: Flights will update automatically - watch for highlight animations
4. **Manual Refresh**: Click the "🔄 Refresh" button to fetch latest data immediately
5. **Notifications**: Toast notifications appear when flights are updated or added

### For Developers

#### Adding Custom Update Logic
```csharp
// In FlightSyncService.cs
public async Task SyncFlightsAsync()
{
    // Your custom sync logic here
    
    // Notify clients
    await _hubContext.Clients.Group("FlightUpdates")
        .SendAsync("FlightUpdated", updatedFlights);
}
```

#### Subscribing to Events (Client-Side)
```javascript
connection.on("FlightUpdated", function(flights) {
    // Handle flight updates
    flights.forEach(flight => {
        updateFlightRow(flight);
    });
});
```

## Visual Indicators

### Status Badges
- **On-Time** (Green): Flight is on schedule
- **Delayed** (Orange): Flight is delayed with pulse indicator
- **Boarding** (Blue): Boarding in progress with pulse indicator
- **Departed** (Purple): Flight has departed
- **Arrived** (Teal): Flight has arrived
- **Cancelled** (Red): Flight cancelled
- **Scheduled** (Gray): Default status

### Animations
- **Flight Updated**: Blue highlight fade effect
- **New Flight**: Slide-in animation with green accent
- **Connection Pulse**: Animated dot indicates live connection
- **Notification Toast**: Slide-in from right side

## API Endpoints

### Manual Refresh
**POST** `/Flight/RefreshFlights`

Triggers an immediate flight data synchronization.

**Response:**
```json
{
  "success": true,
  "message": "Flights refreshed successfully"
}
```

## Performance Considerations

1. **Rate Limiting**: Automatic sync runs every 2 minutes to respect API limits
2. **Efficient Updates**: Only changed flights trigger database updates
3. **Batched Notifications**: Multiple flight updates sent in single SignalR message
4. **Change Detection**: Compares status, gate, and times before updating

## Troubleshooting

### Connection Issues
- Check browser console for SignalR connection errors
- Verify server is running and SignalR hub is configured
- Check network connectivity

### No Updates Appearing
- Verify AeroDataBox API credentials in appsettings.json
- Check background service logs for errors
- Ensure DefaultAirport ICAO code is correct

### Performance Issues
- Increase update interval if too frequent
- Check database performance with large flight datasets
- Monitor SignalR connection count

## Future Enhancements

- [ ] Multiple airport tracking
- [ ] User-specific flight subscriptions
- [ ] WebSocket fallback for older browsers
- [ ] Flight delay predictions
- [ ] Email/SMS notifications for specific flights
- [ ] Historical flight data analysis
- [ ] Weather integration
- [ ] Real-time passenger capacity tracking

## Dependencies

- **Microsoft.AspNetCore.SignalR** (9.0.0)
- **SignalR Client** (8.0.0 - CDN)
- **AeroDataBox API** (via RapidAPI)
- **Entity Framework Core** (9.0.0)

## License

This feature is part of the Airport Flight Management System project.

---

**Need Help?** Check the browser console for detailed SignalR logs and connection status.
