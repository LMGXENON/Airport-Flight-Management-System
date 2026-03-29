// Modal logic split into manager, renderer and progress tracker.

class FlightModalManager {
    constructor(modalId = 'flightModal') {
        this.modalId = modalId;
        this.modal = document.getElementById(modalId);
        this.isAttached = false;
        
        if (!this.modal) {
            console.warn(`Modal element with id "${modalId}" not found`);
        }
    }

    openFromFlightRow(flightRowElement) {
        if (!this.modal) return;

        const flightData = this.extractFlightDataFromRow(flightRowElement);
        const renderer = new FlightModalRenderer(this.modal);
        renderer.renderFlightDetails(flightData);

        this.modal.classList.add('active');
        
        // Only run the timer when both UTC timestamps exist.
        if (flightData.depUtc && flightData.arrUtc) {
            FlightProgressTracker.start(flightData);
        }
    }

    close() {
        if (!this.modal) return;

        FlightProgressTracker.stop();
        this.modal.classList.remove('active');
    }

    attachHandlers(rowSelector = '.flight-row', manageLinkSelector = '.manage-col') {
        if (this.isAttached) return;
        this.isAttached = true;

        document.addEventListener('click', (event) => {
            const flightRow = event.target.closest(rowSelector);
            
            if (!flightRow || event.target.closest(manageLinkSelector)) {
                return;
            }

            this.openFromFlightRow(flightRow);
        });

        const closeButton = this.modal?.querySelector('.modal-close');
        if (closeButton) {
            closeButton.addEventListener('click', () => this.close());
        }

        if (this.modal) {
            this.modal.addEventListener('click', (event) => {
                if (event.target === this.modal) {
                    this.close();
                }
            });
        }
    }

    extractFlightDataFromRow(rowElement) {
        return {
            flightNumber: rowElement.getAttribute('data-flight-number') || '',
            airline: rowElement.getAttribute('data-airline') || '',
            direction: rowElement.getAttribute('data-direction') || '',
            depIata: rowElement.getAttribute('data-dep-iata') || '',
            arrIata: rowElement.getAttribute('data-arr-iata') || '',
            depCity: rowElement.getAttribute('data-dep-city') || '',
            arrCity: rowElement.getAttribute('data-arr-city') || '',
            depTime: rowElement.getAttribute('data-dep-time') || '',
            arrTime: rowElement.getAttribute('data-arr-time') || '',
            depUtc: rowElement.getAttribute('data-dep-utc') || '',
            arrUtc: rowElement.getAttribute('data-arr-utc') || '',
            aircraft: rowElement.getAttribute('data-aircraft') || '',
            runwayDepUtc: rowElement.getAttribute('data-runway-dep-utc') || '',
            runwayArrUtc: rowElement.getAttribute('data-runway-arr-utc') || '',
            gate: rowElement.getAttribute('data-gate') || '',
            terminal: rowElement.getAttribute('data-terminal') || '',
            statusClass: rowElement.getAttribute('data-status-class') || '',
            statusLabel: rowElement.getAttribute('data-status-label') || ''
        };
    }
}

class FlightModalRenderer {
    constructor(modalElement) {
        this.modal = modalElement;
    }

    renderFlightDetails(flightData) {
        this.renderHeader(flightData);
        this.renderStatusBanner(flightData);
        this.renderAirportBlocks(flightData);
    }

    renderHeader(flightData) {
        this.setText('modalAirline', flightData.airline);
        this.setText('modalFlightNumber', flightData.flightNumber);
        this.setText('modalAircraft', `Aircraft: ${flightData.aircraft && flightData.aircraft !== '-' ? flightData.aircraft : 'Unknown'}`);
        this.setText('modalRoute', `${flightData.depIata} to ${flightData.arrIata}`);
    }

    renderStatusBanner(flightData) {
        const statusBanner = document.getElementById('modalStatusBanner');
        if (statusBanner) {
            statusBanner.className = `modal-status-banner ${flightData.statusClass}`;
            this.setText('modalStatus', flightData.statusLabel);
        }
    }

    renderAirportBlocks(flightData) {
        const isDep = flightData.direction === 'Departure';

        this.setText('modalDepCode', flightData.depIata);
        this.setText('modalArrCode', flightData.arrIata);
        this.setText('modalDepCity', flightData.depCity);
        this.setText('modalArrCity', flightData.arrCity);

        const depLabel = flightData.depCity && flightData.depCity !== flightData.depIata 
            ? `${flightData.depIata} ${flightData.depCity}` 
            : flightData.depIata;
        const arrLabel = flightData.arrCity && flightData.arrCity !== flightData.arrIata 
            ? `${flightData.arrIata} ${flightData.arrCity}` 
            : flightData.arrIata;
        
        this.setText('progressDepCode', depLabel);
        this.setText('progressArrCode', arrLabel);

        this.setText('modalDepTerminal', isDep ? flightData.terminal : '-');
        this.setText('modalDepGate', isDep ? flightData.gate : '-');
        this.setText('modalArrTerminal', !isDep ? flightData.terminal : '-');
        this.setText('modalArrGate', !isDep ? flightData.gate : '-');

        this.renderFlightTimes(flightData, isDep);
    }

    renderFlightTimes(flightData, isDep) {
        const depTimeDisplay = flightData.depTime.split(' ').pop();
        const arrTimeDisplay = flightData.arrTime.split(' ').pop();

        this.setText('modalDepTime', depTimeDisplay);
        this.setText('modalArrTime', arrTimeDisplay);

        this.renderTimeWithActual('dep', flightData.runwayDepUtc, 'Scheduled departure');
        this.renderTimeWithActual('arr', flightData.runwayArrUtc, 'Scheduled arrival');
    }

    renderTimeWithActual(prefix, actualUtc, defaultLabel) {
        const originalElement = document.getElementById(`modal${prefix.charAt(0).toUpperCase() + prefix.slice(1)}TimeOriginal`);
        const labelElement = document.getElementById(`${prefix}TimeLabel`);

        if (!originalElement || !labelElement) return;

        if (actualUtc) {
            const actualTime = new Date(actualUtc).toLocaleTimeString('en-GB', { 
                hour: '2-digit', 
                minute: '2-digit' 
            });
            originalElement.textContent = actualTime;
            originalElement.style.display = 'inline';
            labelElement.textContent = prefix === 'dep' ? 'Actual departure' : 'Actual arrival';
        } else {
            originalElement.style.display = 'none';
            labelElement.textContent = defaultLabel;
        }
    }

    setText(elementId, text) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = text;
        }
    }
}

class FlightProgressTracker {
    static progressRefreshTimer = null;
    static activeFlightData = null;
    static REFRESH_INTERVAL = 30000; // 30 seconds

    static start(flightData) {
        this.stop();
        this.activeFlightData = flightData;
        this.updateProgress();
        this.progressRefreshTimer = setInterval(() => this.updateProgress(), this.REFRESH_INTERVAL);
    }

    static stop() {
        if (this.progressRefreshTimer) {
            clearInterval(this.progressRefreshTimer);
            this.progressRefreshTimer = null;
        }
        this.activeFlightData = null;
    }

    static updateProgress() {
        if (!this.activeFlightData) return;

        const progress = this.calculateProgress(this.activeFlightData);
        this.renderProgressBar(progress);
        this.renderProgressTiming(progress, this.activeFlightData);
    }

    static calculateProgress(flightData) {
        const depTime = new Date(flightData.depUtc).getTime();
        const arrTime = new Date(flightData.arrUtc).getTime();
        const now = Date.now();

        // Keep progress in range even when data arrives late or out of order.
        const totalDuration = arrTime - depTime;
        const elapsed = Math.max(0, now - depTime);
        const remaining = Math.max(0, arrTime - now);

        const progressPercent = totalDuration > 0 
            ? Math.min(100, (elapsed / totalDuration) * 100) 
            : 0;

        return {
            percent: progressPercent,
            elapsed,
            remaining,
            totalDuration,
            isDeparted: now > depTime,
            hasArrived: now > arrTime
        };
    }

    static renderProgressBar(progress) {
        const progressBarFill = document.getElementById('progressBarFill');
        const progressPlane = document.getElementById('progressPlane');

        if (progressBarFill) {
            progressBarFill.style.width = `${progress.percent}%`;
        }

        if (progressPlane) {
            progressPlane.style.left = `${progress.percent}%`;
        }
    }

    static renderProgressTiming(progress, flightData) {
        const timeInfoElement = document.getElementById('progressTimeInfo');
        if (!timeInfoElement) return;

        let timeText = '';

        if (progress.hasArrived) {
            timeText = '✓ Flight has arrived';
        } else if (progress.isDeparted) {
            const remainingHours = Math.floor(progress.remaining / (1000 * 60 * 60));
            const remainingMinutes = Math.floor((progress.remaining % (1000 * 60 * 60)) / (1000 * 60));
            timeText = `~${remainingHours}h ${remainingMinutes}m remaining`;
        } else {
            const departingInHours = Math.floor(progress.remaining / (1000 * 60 * 60));
            const departingInMinutes = Math.floor((progress.remaining % (1000 * 60 * 60)) / (1000 * 60));
            timeText = `Departing in ~${departingInHours}h ${departingInMinutes}m`;
        }

        timeInfoElement.textContent = timeText;
    }
}

// ============================================================================
// Initialization
// ============================================================================

// Initialize modal manager when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    const modalManager = new FlightModalManager('flightModal');
    modalManager.attachHandlers();
});
