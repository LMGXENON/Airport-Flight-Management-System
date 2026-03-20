/**
 * flight-details-modal.js
 * Shared modal logic used on both the Dashboard and Advanced Search pages.
 * Attach flight row click handlers via attachFlightRowHandlers() after
 * rendering rows (including after AJAX updates).
 */

let progressRefreshTimer = null;
let activeFlightProgressData = null;
let flightRowHandlersAttached = false;
let modalHandlersAttached = false;

function closeFlightModal() {
    stopFlightProgressRefresh();
    const modal = document.getElementById('flightModal');
    if (modal) {
        modal.classList.remove('active');
    }
}

function stopFlightProgressRefresh() {
    if (progressRefreshTimer) {
        clearInterval(progressRefreshTimer);
        progressRefreshTimer = null;
    }
}

function startFlightProgressRefresh() {
    stopFlightProgressRefresh();
    progressRefreshTimer = setInterval(() => {
        if (activeFlightProgressData) {
            calculateFlightProgress(activeFlightProgressData);
        }
    }, 30000);
}

function attachFlightRowHandlers() {
    if (flightRowHandlersAttached) {
        return;
    }

    flightRowHandlersAttached = true;

    document.addEventListener('click', event => {
        const row = event.target.closest('.flight-row');
        if (!row || event.target.closest('.manage-col')) {
            return;
        }

        openFlightModal(row);
    });
}

function attachModalHandlers() {
    if (modalHandlersAttached) {
        return;
    }

    modalHandlersAttached = true;

    const modal = document.getElementById('flightModal');
    if (modal) {
        const closeButton = modal.querySelector('.modal-close');
        if (closeButton) {
            closeButton.addEventListener('click', closeFlightModal);
        }

        modal.addEventListener('click', event => {
            if (event.target === modal) {
                closeFlightModal();
            }
        });
    }
}

function openFlightModal(row) {
    const modal = document.getElementById('flightModal');

    const flightNumber    = row.getAttribute('data-flight-number');
    const airline         = row.getAttribute('data-airline');
    const direction       = row.getAttribute('data-direction');
    const depIata         = row.getAttribute('data-dep-iata');
    const arrIata         = row.getAttribute('data-arr-iata');
    const depCity         = row.getAttribute('data-dep-city');
    const arrCity         = row.getAttribute('data-arr-city');
    const depTime         = row.getAttribute('data-dep-time');
    const arrTime         = row.getAttribute('data-arr-time');
    const depUtc          = row.getAttribute('data-dep-utc');
    const arrUtc          = row.getAttribute('data-arr-utc');
    const runwayDepUtc    = row.getAttribute('data-runway-dep-utc');
    const runwayArrUtc    = row.getAttribute('data-runway-arr-utc');
    const revisedDepUtc   = row.getAttribute('data-revised-dep-utc');
    const revisedArrUtc   = row.getAttribute('data-revised-arr-utc');
    const predictedDepUtc = row.getAttribute('data-predicted-dep-utc');
    const predictedArrUtc = row.getAttribute('data-predicted-arr-utc');
    const rawStatus       = row.getAttribute('data-raw-status');
    const gate            = row.getAttribute('data-gate');
    const terminal        = row.getAttribute('data-terminal');
    const statusClass     = row.getAttribute('data-status-class');
    const statusLabel     = row.getAttribute('data-status-label');

    const isDep = direction === 'Departure';

    document.getElementById('modalAirline').textContent      = airline;
    document.getElementById('modalFlightNumber').textContent = flightNumber;
    document.getElementById('modalRoute').textContent        = `${depIata} to ${arrIata}`;

    const statusBanner = document.getElementById('modalStatusBanner');
    statusBanner.className = 'modal-status-banner ' + statusClass;
    document.getElementById('modalStatus').textContent = statusLabel;

    document.getElementById('modalDepCode').textContent = depIata;
    document.getElementById('modalArrCode').textContent = arrIata;
    document.getElementById('modalDepCity').textContent = depCity;
    document.getElementById('modalArrCity').textContent = arrCity;

    const depLabel = depCity && depCity !== depIata ? `${depIata} ${depCity}` : depIata;
    const arrLabel = arrCity && arrCity !== arrIata ? `${arrIata} ${arrCity}` : arrIata;
    document.getElementById('progressDepCode').textContent = depLabel;
    document.getElementById('progressArrCode').textContent = arrLabel;

    document.getElementById('modalDepTerminal').textContent = isDep ? terminal : '-';
    document.getElementById('modalDepGate').textContent     = isDep ? gate     : '-';
    document.getElementById('modalArrTerminal').textContent = !isDep ? terminal : '-';
    document.getElementById('modalArrGate').textContent     = !isDep ? gate     : '-';

    document.getElementById('modalDepTime').textContent = depTime.split(' ').pop();
    document.getElementById('modalArrTime').textContent = arrTime.split(' ').pop();

    const depTimeOriginal = document.getElementById('modalDepTimeOriginal');
    const arrTimeOriginal = document.getElementById('modalArrTimeOriginal');

    if (runwayDepUtc) {
        depTimeOriginal.textContent = new Date(runwayDepUtc).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
        depTimeOriginal.style.display = 'inline';
        document.getElementById('depTimeLabel').textContent = 'Actual departure';
    } else {
        depTimeOriginal.style.display = 'none';
        document.getElementById('depTimeLabel').textContent = 'Scheduled departure';
    }

    if (runwayArrUtc) {
        arrTimeOriginal.textContent = new Date(runwayArrUtc).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
        arrTimeOriginal.style.display = 'inline';
        document.getElementById('arrTimeLabel').textContent = 'Actual arrival';
    } else {
        arrTimeOriginal.style.display = 'none';
        document.getElementById('arrTimeLabel').textContent = 'Scheduled arrival';
    }

    activeFlightProgressData = {
        depUtc, arrUtc,
        runwayDepUtc, runwayArrUtc,
        revisedDepUtc, revisedArrUtc,
        predictedDepUtc, predictedArrUtc,
        rawStatus
    };

    calculateFlightProgress(activeFlightProgressData);
    startFlightProgressRefresh();
    modal.classList.add('active');
}

function calculateFlightProgress(flightData) {
    const {
        depUtc, arrUtc,
        runwayDepUtc, runwayArrUtc,
        revisedDepUtc, revisedArrUtc,
        predictedDepUtc, predictedArrUtc,
        rawStatus
    } = flightData;

    if (!depUtc || !arrUtc) {
        document.getElementById('progressLabel').textContent = 'No data';
        document.getElementById('progressBarFill').style.width = '0%';
        document.getElementById('progressPlane').style.left    = '0%';
        document.getElementById('progressTimeInfo').textContent = '';
        document.getElementById('flightDuration').textContent   = '-';
        document.getElementById('flightDistance').textContent   = '-';
        return;
    }

    const depTime = new Date(depUtc);
    const arrTime = new Date(arrUtc);
    const now     = new Date();

    const totalMins = Math.max(0, (arrTime - depTime) / 60000);
    const hrs  = Math.floor(totalMins / 60);
    const mins = Math.round(totalMins % 60);
    document.getElementById('flightDuration').textContent = `${hrs}h ${mins}m`;

    let progress = 0;
    let label = 'Scheduled';
    let info  = '';

    const hasActuallyArrived  = (runwayArrUtc && runwayArrUtc !== '') || rawStatus === 'arrived';
    const hasActuallyDeparted = (runwayDepUtc && runwayDepUtc !== '')
        || rawStatus === 'enroute'
        || rawStatus === 'approaching'
        || rawStatus === 'departed'
        || rawStatus === 'gateclosed';

    if (hasActuallyArrived) {
        progress = 100;
        label    = 'Arrived';
        info     = 'Flight completed';
    } else if (hasActuallyDeparted) {
        let actualDep;
        if      (runwayDepUtc   && runwayDepUtc   !== '') actualDep = new Date(runwayDepUtc);
        else if (revisedDepUtc  && revisedDepUtc  !== '') actualDep = new Date(revisedDepUtc);
        else if (predictedDepUtc && predictedDepUtc !== '') actualDep = new Date(predictedDepUtc);
        else actualDep = depTime;

        let estimatedArr;
        if      (revisedArrUtc  && revisedArrUtc  !== '') estimatedArr = new Date(revisedArrUtc);
        else if (predictedArrUtc && predictedArrUtc !== '') estimatedArr = new Date(predictedArrUtc);
        else estimatedArr = arrTime;

        const elapsed        = (now - actualDep) / 60000;
        const flightDuration = (estimatedArr - actualDep) / 60000;

        if (elapsed > 0 && flightDuration > 0) {
            progress = Math.min(100, Math.max(0, (elapsed / flightDuration) * 100));
            label    = rawStatus === 'approaching' ? 'Approaching' : 'In Flight';
            const remaining = Math.max(0, flightDuration - elapsed);
            const remHrs  = Math.floor(remaining / 60);
            const remMins = Math.round(remaining % 60);
            info = remHrs > 0 ? `${remHrs}h ${remMins}m remaining` : `${remMins} min remaining`;
        } else {
            progress = 0;
            label    = 'Departed';
            info     = 'Flight in progress';
        }
    } else {
        progress = 0;
        label    = rawStatus === 'boarding' ? 'Boarding' : rawStatus === 'delayed' ? 'Delayed' : 'Scheduled';

        let effectiveDep = depTime;
        if      (revisedDepUtc   && revisedDepUtc   !== '') effectiveDep = new Date(revisedDepUtc);
        else if (predictedDepUtc && predictedDepUtc !== '') effectiveDep = new Date(predictedDepUtc);

        const timeUntil = (effectiveDep - now) / 60000;
        if (timeUntil > 0 && timeUntil < 120) {
            info = timeUntil < 60
                ? `Departs in ${Math.round(timeUntil)} min`
                : `Departs in ${Math.floor(timeUntil / 60)}h ${Math.round(timeUntil % 60)}m`;
        } else if (timeUntil < 0) {
            info = 'Flight delayed or status unknown';
        } else {
            info = `Departs ${effectiveDep.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })}`;
        }
    }

    const averageCruiseKmh          = 850;
    const estimatedTotalDistanceKm  = Math.max(0, (totalMins / 60) * averageCruiseKmh);
    const estimatedTravelledKm      = (progress / 100) * estimatedTotalDistanceKm;
    document.getElementById('flightDistance').textContent = `~${Math.round(estimatedTravelledKm)} / ${Math.round(estimatedTotalDistanceKm)} km`;

    document.getElementById('progressLabel').textContent     = label;
    document.getElementById('progressBarFill').style.width   = progress + '%';
    document.getElementById('progressPlane').style.left      = progress + '%';
    document.getElementById('progressTimeInfo').textContent  = info;
}

// Close modal when clicking the backdrop
document.addEventListener('DOMContentLoaded', () => {
    attachModalHandlers();
    attachFlightRowHandlers();
});
