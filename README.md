# Airport-Flight-Management-System
CST2550 Group Coursework

## A Web App created using C#
### ✨ Features

- **Real-Time Flight Tracking** - Live updates powered by SignalR and AeroDataBox API
- **Flight Management** - Add, edit, delete, and view flight details
- **Advanced Search** - Search flights by multiple criteria
- **Live Status Updates** - Automatic flight status synchronization every 2 minutes
- **Responsive Design** - Modern, clean interface with smooth animations
- **Background Sync** - Automated data synchronization with external flight APIs

### 🚀 Real-Time Updates

The system now includes **live flight tracking** with:
- Automatic flight status updates
- Real-time gate change notifications
- Visual indicators for updated flights
- Connection status monitoring
- Manual refresh capability

See [REALTIME_TRACKING.md](REALTIME_TRACKING.md) for detailed documentation.

### 🛠️ Technology Stack

- **Backend**: ASP.NET Core 10.0
- **Frontend**: Razor Pages, JavaScript, CSS
- **Database**: SQLite with Entity Framework Core
- **Real-Time**: SignalR for WebSocket communication
- **API Integration**: AeroDataBox (RapidAPI)
- **Infrastructure**: Docker, Terraform (AWS ECS, RDS, VPC)

### 📦 Installation

1. Clone the repository
2. Configure `appsettings.json` with your AeroDataBox API credentials
3. Run `dotnet restore` to install dependencies
4. Run `dotnet build` to build the project
5. Run `dotnet run` to start the application
6. Navigate to `https://localhost:5001`

### 🔧 Configuration

Update `appsettings.json`:
```json
{
  "AeroDataBox": {
    "ApiKey": "your-rapidapi-key",
    "ApiHost": "aerodatabox.p.rapidapi.com",
    "DefaultAirport": "EGLL"
  }
}
```