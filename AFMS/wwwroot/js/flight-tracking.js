// Real-time Flight Tracking with SignalR
(function() {
    'use strict';

    // Check if we're on the flight index page
    const flightTable = document.querySelector('.flights-table tbody');
    if (!flightTable) {
        return; // Not on the flights page
    }

    // Create SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/flightHub")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Handle flight updates
    connection.on("FlightUpdated", function(flights) {
        console.log("Received flight updates:", flights);
        
        flights.forEach(flight => {
            updateFlightRow(flight);
        });

        showNotification(`${flights.length} flight(s) updated`, 'info');
    });

    // Handle new flights
    connection.on("FlightAdded", function(flights) {
        console.log("Received new flights:", flights);
        
        flights.forEach(flight => {
            addFlightRow(flight);
        });

        showNotification(`${flights.length} new flight(s) added`, 'success');
    });

    // Start connection
    startConnection();

    async function startConnection() {
        try {
            await connection.start();
            console.log("SignalR Connected");
            
            // Subscribe to flight updates
            await connection.invoke("SubscribeToFlightUpdates");
        } catch (err) {
            console.error("SignalR Connection Error:", err);
            // Retry after 5 seconds
            setTimeout(startConnection, 5000);
        }
    }

    // Handle reconnection
    connection.onreconnecting(() => {
        console.log("SignalR Reconnecting...");
    });

    connection.onreconnected(() => {
        console.log("SignalR Reconnected");
        connection.invoke("SubscribeToFlightUpdates");
    });

    connection.onclose(() => {
        console.log("SignalR Disconnected");
        setTimeout(startConnection, 5000);
    });

    function updateFlightRow(flight) {
        const rows = flightTable.querySelectorAll('tr');
        let found = false;

        rows.forEach(row => {
            const flightNumberCell = row.querySelector('.flight-number');
            if (flightNumberCell && flightNumberCell.textContent.trim() === flight.flightNumber) {
                found = true;
                
                // Update cells
                const cells = row.querySelectorAll('td');
                cells[1].textContent = flight.airline; // Airline
                cells[2].textContent = flight.destination; // Destination
                cells[3].textContent = formatDateTime(flight.departureTime); // Departure
                cells[4].textContent = formatDateTime(flight.arrivalTime); // Arrival
                cells[5].textContent = flight.gate || 'TBD'; // Gate
                cells[6].textContent = `Terminal ${flight.terminal}`; // Terminal
                
                // Update status badge
                const statusBadge = cells[7].querySelector('.status-badge');
                if (statusBadge) {
                    statusBadge.className = 'status-badge ' + getStatusClass(flight.status);
                    statusBadge.textContent = flight.status;
                }

                // Add update animation
                row.classList.add('flight-updated');
                setTimeout(() => row.classList.remove('flight-updated'), 2000);
            }
        });

        if (!found) {
            console.log("Flight not found in table, adding...");
            addFlightRow(flight);
        }
    }

    function addFlightRow(flight) {
        const row = document.createElement('tr');
        row.className = 'flight-new';
        
        row.innerHTML = `
            <td class="flight-number">${flight.flightNumber}</td>
            <td>${flight.airline}</td>
            <td>${flight.destination}</td>
            <td>${formatDateTime(flight.departureTime)}</td>
            <td>${formatDateTime(flight.arrivalTime)}</td>
            <td>${flight.gate || 'TBD'}</td>
            <td>Terminal ${flight.terminal}</td>
            <td>
                <span class="status-badge ${getStatusClass(flight.status)}">${flight.status}</span>
            </td>
            <td class="actions-cell">
                <a href="/Flight/Details/${flight.id}" class="action-link view">View</a>
                <a href="/Flight/Edit/${flight.id}" class="action-link edit">Edit</a>
                <a href="/Flight/Delete/${flight.id}" class="action-link delete">Delete</a>
            </td>
        `;

        flightTable.appendChild(row);
        
        // Remove "new" highlight after 3 seconds
        setTimeout(() => row.classList.remove('flight-new'), 3000);
    }

    function formatDateTime(dateString) {
        const date = new Date(dateString);
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const month = months[date.getMonth()];
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        
        return `${month} ${day}, ${hours}:${minutes}`;
    }

    function getStatusClass(status) {
        const statusLower = (status || '').toLowerCase().replace(/\s+/g, '');
        
        const statusMap = {
            'ontime': 'status-ontime',
            'delayed': 'status-delayed',
            'cancelled': 'status-cancelled',
            'boarding': 'status-boarding',
            'departed': 'status-departed',
            'arrived': 'status-arrived',
            'expected': 'status-ontime',
            'scheduled': 'status-scheduled'
        };

        return statusMap[statusLower] || 'status-scheduled';
    }

    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `flight-notification flight-notification-${type}`;
        notification.textContent = message;
        
        document.body.appendChild(notification);
        
        // Trigger animation
        setTimeout(() => notification.classList.add('show'), 10);
        
        // Remove after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    // Handle manual refresh button
    const refreshButton = document.getElementById('refreshFlights');
    if (refreshButton) {
        refreshButton.addEventListener('click', async function() {
            this.disabled = true;
            this.textContent = '⏳ Refreshing...';
            
            try {
                const response = await fetch('/Flight/RefreshFlights', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
                
                const data = await response.json();
                
                if (data.success) {
                    showNotification('Flight data refreshed successfully!', 'success');
                    // Reload the page after a short delay to show updated data
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showNotification('Failed to refresh flights: ' + data.message, 'error');
                }
            } catch (error) {
                showNotification('Error refreshing flights', 'error');
            } finally {
                this.disabled = false;
                this.textContent = '🔄 Refresh';
            }
        });
    }
})();
