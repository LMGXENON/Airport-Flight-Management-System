(function () {
    'use strict';

    var supportedPaths = new Set([
        '/home/index',
        '/flight/index',
        '/home/advancedsearch'
    ]);

    function shouldEnableRealtime(pathname) {
        var normalizedPath = (pathname || '').toLowerCase();
        return supportedPaths.has(normalizedPath);
    }

    function scheduleRefresh(reason) {
        if (scheduleRefresh._queued) {
            return;
        }

        scheduleRefresh._queued = true;

        window.setTimeout(function () {
            try {
                if (typeof window.showFlightsLoadingSpinner === 'function') {
                    window.showFlightsLoadingSpinner();
                }

                var nextUrl = new URL(window.location.href);
                nextUrl.searchParams.set('rt', Date.now().toString());
                window.location.assign(nextUrl.toString());
            } catch (error) {
                console.warn('[AFMS realtime] failed to refresh page after hub event.', error);
                scheduleRefresh._queued = false;
            }
        }, 500);

        if (reason) {
            console.debug('[AFMS realtime] queued refresh after event:', reason);
        }
    }

    function startRealtime() {
        if (!shouldEnableRealtime(window.location.pathname)) {
            return;
        }

        if (!window.signalR || !window.signalR.HubConnectionBuilder) {
            console.warn('[AFMS realtime] SignalR browser client is unavailable.');
            return;
        }

        var connection = new window.signalR.HubConnectionBuilder()
            .withUrl('/flightHub')
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();

        connection.on('FlightUpdated', function (updatedFlights) {
            var count = Array.isArray(updatedFlights) ? updatedFlights.length : 0;
            console.debug('[AFMS realtime] FlightUpdated received:', count);
            scheduleRefresh('FlightUpdated');
        });

        connection.on('FlightAdded', function (newFlights) {
            var count = Array.isArray(newFlights) ? newFlights.length : 0;
            console.debug('[AFMS realtime] FlightAdded received:', count);
            scheduleRefresh('FlightAdded');
        });

        connection.onreconnecting(function (error) {
            console.warn('[AFMS realtime] reconnecting to flight hub.', error);
        });

        connection.onreconnected(function () {
            console.info('[AFMS realtime] reconnected to flight hub.');
            scheduleRefresh('reconnected');
        });

        connection.onclose(function (error) {
            console.warn('[AFMS realtime] connection closed.', error);
        });

        connection.start()
            .then(function () {
                console.info('[AFMS realtime] connected to /flightHub');
                return connection.invoke('SubscribeToFlightUpdates');
            })
            .catch(function (error) {
                console.error('[AFMS realtime] failed to connect to /flightHub.', error);
            });
    }

    document.addEventListener('DOMContentLoaded', startRealtime);
})();
