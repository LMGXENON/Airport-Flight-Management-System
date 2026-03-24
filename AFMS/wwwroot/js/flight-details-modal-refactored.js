/**
 * flight-details-modal-refactored.js
 * 
 * Refactored modal management with improved separation of concerns:
 * - FlightModalManager: Core modal lifecycle
 * - FlightModalRenderer: Populating modal with data
 * - FlightProgressTracker: Real-time progress updates
 */

// ============================================================================
// FlightModalManager: Core modal lifecycle management
// ============================================================================

class FlightModalManager {
    constructor(modalId = 'flightModal') {
        this.modalId = modalId;
        this.modal = document.getElementById(modalId);
        this.isAttached = false;
        
        if (!this.modal) {
            console.warn(`Modal element with id "${modalId}" not found`);
        }
    }

    /**
     * Opens the modal and populates it with flight data from a row element.
     */
    openFromFlightRow(flightRowElement) {
        if (!this.modal) return;

        const flightData = this.extractFlightDataFromRow(flightRowElement);
        const renderer = new FlightModalRenderer(this.modal);
        renderer.renderFlightDetails(flightData);

        this.modal.classList.add('active');
        
        // Start progress tracking if data available
        if (flightData.depUtc && flightData.arrUtc) {
            FlightProgressTracker.start(flightData);
        }
    }

    /**
     * Closes the modal and stops all background operations.
     */
    close() {
        if (!this.modal) return;

        FlightProgressTracker.stop();
        this.modal.classList.remove('active');
    }

    /**
     * Attaches click handlers to flight rows and modal overlay.
     */
    attachHandlers(rowSelector = '.flight-row', manageLinkSelector = '.manage-col') {
        if (this.isAttached) return;
        this.isAttached = true;

        // Flight row click handler
        document.addEventListener('click', (event) => {
            const flightRow = event.target.closest(rowSelector);
            
            if (!flightRow || event.target.closest(manageLinkSelector)) {
                return;
            }

            this.openFromFlightRow(flightRow);
        });

        // Modal close button
        const closeButton = this.modal?.querySelector('.modal-close');
        if (closeButton) {
            closeButton.addEventListener('click', () => this.close());
        }

        // Modal overlay click
        if (this.modal) {
            this.modal.addEventListener('click', (event) => {
                if (event.target === this.modal) {
                    this.close();
                }
            });
        }
    }

    /**
     * Extracts flight data from a flight row element's data attributes.
     */
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

// ============================================================================
// FlightModalRenderer: Populates modal with flight data
// ============================================================================

class FlightModalRenderer {
    constructor(modalElement) {
        this.modal = modalElement;
    }

    /**
     * Renders all flight details into the modal.
     */
    renderFlightDetails(flightData) {
        this.renderHeader(flightData);
        this.renderStatusBanner(flightData);
        this.renderAirportBlocks(flightData);
    }

    /**
     * Renders modal header information.
     */
    renderHeader(flightData) {
        this.setText('modalAirline', flightData.airline);
        this.setText('modalFlightNumber', flightData.flightNumber);
        this.setText('modalAircraft', `Aircraft: ${flightData.aircraft && flightData.aircraft !== '-' ? flightData.aircraft : 'Unknown'}`);
        this.setText('modalRoute', `${flightData.depIata} to ${flightData.arrIata}`);
    }

    /**
     * Renders status banner with appropriate styling.
     */
    renderStatusBanner(flightData) {
        const statusBanner = document.getElementById('modalStatusBanner');
        if (statusBanner) {
            statusBanner.className = `modal-status-banner ${flightData.statusClass}`;
            this.setText('modalStatus', flightData.statusLabel);
        }
    }

    /**
     * Renders departure and arrival airport blocks.
     */
    renderAirportBlocks(flightData) {
        const isDep = flightData.direction === 'Departure';

        // Airport codes
        this.setText('modalDepCode', flightData.depIata);
        this.setText('modalArrCode', flightData.arrIata);
        this.setText('modalDepCity', flightData.depCity);
        this.setText('modalArrCity', flightData.arrCity);

        // Progress section
        const depLabel = flightData.depCity && flightData.depCity !== flightData.depIata 
            ? `${flightData.depIata} ${flightData.depCity}` 
            : flightData.depIata;
        const arrLabel = flightData.arrCity && flightData.arrCity !== flightData.arrIata 
            ? `${flightData.arrIata} ${flightData.arrCity}` 
            : flightData.arrIata;
        
        this.setText('progressDepCode', depLabel);
        this.setText('progressArrCode', arrLabel);

        // Terminal and gate
        this.setText('modalDepTerminal', isDep ? flightData.terminal : '-');
        this.setText('modalDepGate', isDep ? flightData.gate : '-');
        this.setText('modalArrTerminal', !isDep ? flightData.terminal : '-');
        this.setText('modalArrGate', !isDep ? flightData.gate : '-');

        // Times
        this.renderFlightTimes(flightData, isDep);
    }

    /**
     * Renders flight departure/arrival times and actual vs scheduled labels.
     */
    renderFlightTimes(flightData, isDep) {
        const depTimeDisplay = flightData.depTime.split(' ').pop();
        const arrTimeDisplay = flightData.arrTime.split(' ').pop();

        this.setText('modalDepTime', depTimeDisplay);
        this.setText('modalArrTime', arrTimeDisplay);

        this.renderTimeWithActual('dep', flightData.runwayDepUtc, 'Scheduled departure');
        this.renderTimeWithActual('arr', flightData.runwayArrUtc, 'Scheduled arrival');
    }

    /**
     * Renders time display with actual vs scheduled label.
     */
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

    /**
     * Helper to set text content of an element by ID.
     */
    setText(elementId, text) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = text;
        }
    }
}

// ============================================================================
// FlightProgressTracker: Real-time flight progress updates
// ============================================================================

class FlightProgressTracker {
    static progressRefreshTimer = null;
    static activeFlightData = null;
    static REFRESH_INTERVAL = 30000; // 30 seconds

    /**
     * Starts progress tracking for a flight.
     */
    static start(flightData) {
        this.stop();
        this.activeFlightData = flightData;
        this.updateProgress();
        this.progressRefreshTimer = setInterval(() => this.updateProgress(), this.REFRESH_INTERVAL);
    }

    /**
     * Stops progress tracking.
     */
    static stop() {
        if (this.progressRefreshTimer) {
            clearInterval(this.progressRefreshTimer);
            this.progressRefreshTimer = null;
        }
        this.activeFlightData = null;
    }

    /**
     * Updates flight progress display.
     */
    static updateProgress() {
        if (!this.activeFlightData) return;

        const progress = this.calculateProgress(this.activeFlightData);
        this.renderProgressBar(progress);
        this.renderProgressTiming(progress, this.activeFlightData);
    }

    /**
     * Calculates flight progress as a percentage and time information.
     */
    static calculateProgress(flightData) {
        const depTime = new Date(flightData.depUtc).getTime();
        const arrTime = new Date(flightData.arrUtc).getTime();
        const now = Date.now();

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

    /**
     * Renders the progress bar fill and indicator.
     */
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

    /**
     * Renders progress timing information.
     */
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
