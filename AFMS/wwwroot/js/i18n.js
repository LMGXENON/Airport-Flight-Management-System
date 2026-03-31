/* ─────────────────────────────────────────────────────────────────
   AFMS  –  Client-side i18n (English / Spanish / French)
   Language is stored in localStorage as 'afms-language'.
   Add  data-i18n="key"           → replaces textContent
        data-i18n-placeholder="key" → replaces placeholder attribute
        data-i18n-title="key"      → replaces title attribute
        data-i18n-html="key"       → replaces innerHTML (links inside)
   ───────────────────────────────────────────────────────────────── */
(function () {
    'use strict';

    /* ── Translation dictionary ──────────────────────────────────── */
    var T = {
        en: {
            /* Navigation */
            'nav.dashboard':      'Dashboard',
            'nav.addFlight':      'Add Flight',
            'nav.flightDetails':  'Flight Details',
            'nav.advancedSearch': 'Advanced Search',
            'nav.collapse':       'Collapse',
            'nav.expand':         'Expand',
            'nav.logout':         'Logout',
            'nav.loggedInAs':     'Logged in as',

            /* Top bar */
            'topbar.currentTime': 'Current Time',
            'topbar.profile':     'Profile',
            'topbar.settings':    'Settings',
            'topbar.logout':      'Logout',

            /* AI Chat */
            'ai.title':               'Flight Assistant',
            'ai.subtitle':            'Search helper for flights and filters',
            'ai.welcome.title':       'AI Flight Assistant',
            'ai.welcome.text':        'Ask me about flights, schedules, and airport information.',
            'ai.inputPlaceholder':    'Describe your flight search...',
            'ai.inputHint':           'Press Enter to send. Shift+Enter adds a new line.',
            'ai.clearChat':           'Clear chat',
            'ai.unavailable.title':   'Flight Assistant is unavailable here',
            'ai.unavailable.text':    'Use one of these pages to chat with the assistant:',
            'ai.unavailable.goAdv':   'Go to Advanced Search',
            'ai.unavailable.goAdd':   'Go to Add Flight',

            /* Dashboard */
            'dash.title':              'Dashboard',
            'dash.subtitle':           'London Heathrow Airport (LHR/EGLL) — Live Flight Data',
            'dash.totalFlights':       'Total Flights',
            'dash.departures':         'Departures',
            'dash.arrivals':           'Arrivals',
            'dash.airlines':           'Airlines',
            'dash.liveData':           'Live Data',
            'dash.dataUnavailable':    'Data Unavailable',
            'dash.liveFlights':        'Live Flights',
            'dash.lastUpdated':        'Last updated',
            'dash.filterAll':          'All',
            'dash.filterDep':          'Departures',
            'dash.filterArr':          'Arrivals',
            'dash.phFlight':           'Filter by flight number',
            'dash.phAirline':          'Filter by airline',
            'dash.allStatuses':        'All statuses',
            'dash.statusScheduled':    'Scheduled',
            'dash.statusBoarding':     'Boarding',
            'dash.statusDeparted':     'Departed',
            'dash.statusArrived':      'Arrived',
            'dash.statusDelayed':      'Delayed',
            'dash.statusCanceled':     'Canceled',
            'dash.statusInFlight':     'In Flight',
            'dash.statusApproaching':  'Approaching',
            'dash.clearFilters':       'Clear',
            'dash.colTime':            'TIME',
            'dash.colFlight':          'FLIGHT',
            'dash.colFrom':            'FROM',
            'dash.colTo':              'TO',
            'dash.colAirline':         'AIRLINE',
            'dash.colAircraft':        'AIRCRAFT',
            'dash.colStatus':          'STATUS',
            'dash.colManage':          'MANAGE',
            'dash.errorTitle':         'Live data unavailable',
            'dash.errorText':          'Flight data temporarily unavailable — quota resets shortly.',
            'dash.retryBtn':           '↺ Retry',
            'dash.noDataTitle':        'No live flight data available',
            'dash.noDataText':         'The API did not return flights for the current time window.',

            /* Flight Details page */
            'flight.pageTitle':    'Managed Flights',
            'flight.pageSubtitle': 'Search, edit or remove flights. Changes appear live on the dashboard.',
            'flight.addBtn':       '+ Add New Flight',
            'flight.phSearch':     'e.g. BA123, search by flight number...',
            'flight.searchBtn':    'Search',
            'flight.clearSearch':  'Clear',
            'flight.colNum':       'Flight #',
            'flight.colAirline':   'Airline',
            'flight.colDest':      'Destination',
            'flight.colAircraft':  'Aircraft',
            'flight.colDep':       'Departure',
            'flight.colArr':       'Arrival',
            'flight.colGate':      'Gate',
            'flight.colTerminal':  'Terminal',
            'flight.colStatus':    'Status',
            'flight.colActions':   'Actions',
            'flight.viewBtn':      'View Flight',
            'flight.editBtn':      'Edit Flight',
            'flight.deleteBtn':    'Delete Flight',
            'flight.noFlights':    'No flights found',
            'flight.addFirst':     'Click "Add New Flight" to create your first flight.',

            /* Settings */
            'settings.title':       'Settings',
            'settings.subtitle':    'Manage your AFMS user preferences.',
            'settings.prefs':       'Preferences',
            'settings.theme':       'Theme',
            'settings.themeLight':  'Light',
            'settings.themeDark':   'Dark',
            'settings.timeFormat':  'Time format',
            'settings.time24':      '24-hour',
            'settings.time12':      '12-hour',
            'settings.language':    'Language',
            'settings.langEn':      'English',
            'settings.langEs':      'Spanish',
            'settings.langFr':      'French',
            'settings.saveBtn':     'Save Settings',
            'settings.saved':       'Settings saved.',

            /* Profile */
            'profile.title':          'Profile',
            'profile.subtitle':       'Your authenticated account details in AFMS.',
            'profile.userIdentity':   'User Identity',
            'profile.username':       'Username',
            'profile.role':           'Role',
            'profile.sessionSec':     'Session and Security',
            'profile.lastLogin':      'Last Login',
            'profile.sessionExp':     'Session Expires In',
            'profile.sessionDur':     'Session Duration',
            'profile.lastRefreshed':  'Last Refreshed',
            'profile.changePwd':      'Change Password',
            'profile.currentPwd':     'Current password',
            'profile.newPwd':         'New password',
            'profile.confirmPwd':     'Confirm new password',
            'profile.updatePwd':      'Update Password',
            'profile.loginHistory':   'Login History',
            'profile.historyTs':      'Timestamp (UTC)',
            'profile.historyIp':      'IP Address',
            'profile.noHistory':      'No login history available yet.',
            'profile.pwdChanged':     'Password changed successfully.',
            'profile.hours':          'hours',

            /* Login */
            'login.title':    'AFMS Login',
            'login.subtitle': 'Sign in to access the dashboard, flight management, and advanced search.',
            'login.username': 'Username',
            'login.password': 'Password',
            'login.btn':      'Login',
        },

        es: {
            'nav.dashboard':      'Panel',
            'nav.addFlight':      'Agregar Vuelo',
            'nav.flightDetails':  'Detalles de Vuelo',
            'nav.advancedSearch': 'Búsqueda Avanzada',
            'nav.collapse':       'Colapsar',
            'nav.expand':         'Expandir',
            'nav.logout':         'Cerrar Sesión',
            'nav.loggedInAs':     'Sesión iniciada como',

            'topbar.currentTime': 'Hora Actual',
            'topbar.profile':     'Perfil',
            'topbar.settings':    'Configuración',
            'topbar.logout':      'Cerrar Sesión',

            'ai.title':               'Asistente de Vuelos',
            'ai.subtitle':            'Ayudante de búsqueda de vuelos y filtros',
            'ai.welcome.title':       'Asistente IA de Vuelos',
            'ai.welcome.text':        'Pregúntame sobre vuelos, horarios e información del aeropuerto.',
            'ai.inputPlaceholder':    'Describe tu búsqueda de vuelo...',
            'ai.inputHint':           'Presiona Enter para enviar. Shift+Enter añade una nueva línea.',
            'ai.clearChat':           'Limpiar chat',
            'ai.unavailable.title':   'El Asistente de Vuelos no está disponible aquí',
            'ai.unavailable.text':    'Usa una de estas páginas para chatear con el asistente:',
            'ai.unavailable.goAdv':   'Ir a Búsqueda Avanzada',
            'ai.unavailable.goAdd':   'Ir a Agregar Vuelo',

            'dash.title':              'Panel',
            'dash.subtitle':           'Aeropuerto de Londres Heathrow (LHR/EGLL) — Datos de Vuelo en Vivo',
            'dash.totalFlights':       'Vuelos Totales',
            'dash.departures':         'Salidas',
            'dash.arrivals':           'Llegadas',
            'dash.airlines':           'Aerolíneas',
            'dash.liveData':           'Datos en Vivo',
            'dash.dataUnavailable':    'Datos No Disponibles',
            'dash.liveFlights':        'Vuelos en Vivo',
            'dash.lastUpdated':        'Última actualización',
            'dash.filterAll':          'Todos',
            'dash.filterDep':          'Salidas',
            'dash.filterArr':          'Llegadas',
            'dash.phFlight':           'Filtrar por número de vuelo',
            'dash.phAirline':          'Filtrar por aerolínea',
            'dash.allStatuses':        'Todos los estados',
            'dash.statusScheduled':    'Programado',
            'dash.statusBoarding':     'Embarcando',
            'dash.statusDeparted':     'Partido',
            'dash.statusArrived':      'Llegado',
            'dash.statusDelayed':      'Retrasado',
            'dash.statusCanceled':     'Cancelado',
            'dash.statusInFlight':     'En Vuelo',
            'dash.statusApproaching':  'Aproximando',
            'dash.clearFilters':       'Limpiar',
            'dash.colTime':            'HORA',
            'dash.colFlight':          'VUELO',
            'dash.colFrom':            'DESDE',
            'dash.colTo':              'HASTA',
            'dash.colAirline':         'AEROLÍNEA',
            'dash.colAircraft':        'AERONAVE',
            'dash.colStatus':          'ESTADO',
            'dash.colManage':          'GESTIONAR',
            'dash.errorTitle':         'Datos en vivo no disponibles',
            'dash.errorText':          'Datos de vuelo temporalmente no disponibles — la cuota se restablece pronto.',
            'dash.retryBtn':           '↺ Reintentar',
            'dash.noDataTitle':        'No hay datos de vuelo en vivo disponibles',
            'dash.noDataText':         'La API no devolvió vuelos para la ventana de tiempo actual.',

            'flight.pageTitle':    'Vuelos Gestionados',
            'flight.pageSubtitle': 'Buscar, editar o eliminar vuelos. Los cambios aparecen en tiempo real en el panel.',
            'flight.addBtn':       '+ Agregar Nuevo Vuelo',
            'flight.phSearch':     'Ej. BA123, buscar por número de vuelo...',
            'flight.searchBtn':    'Buscar',
            'flight.clearSearch':  'Limpiar',
            'flight.colNum':       'Vuelo #',
            'flight.colAirline':   'Aerolínea',
            'flight.colDest':      'Destino',
            'flight.colAircraft':  'Aeronave',
            'flight.colDep':       'Salida',
            'flight.colArr':       'Llegada',
            'flight.colGate':      'Puerta',
            'flight.colTerminal':  'Terminal',
            'flight.colStatus':    'Estado',
            'flight.colActions':   'Acciones',
            'flight.viewBtn':      'Ver Vuelo',
            'flight.editBtn':      'Editar Vuelo',
            'flight.deleteBtn':    'Eliminar Vuelo',
            'flight.noFlights':    'No se encontraron vuelos',
            'flight.addFirst':     'Haz clic en "Agregar Nuevo Vuelo" para crear tu primer vuelo.',

            'settings.title':       'Configuración',
            'settings.subtitle':    'Gestione sus preferencias de usuario de AFMS.',
            'settings.prefs':       'Preferencias',
            'settings.theme':       'Tema',
            'settings.themeLight':  'Claro',
            'settings.themeDark':   'Oscuro',
            'settings.timeFormat':  'Formato de hora',
            'settings.time24':      '24 horas',
            'settings.time12':      '12 horas',
            'settings.language':    'Idioma',
            'settings.langEn':      'Inglés',
            'settings.langEs':      'Español',
            'settings.langFr':      'Francés',
            'settings.saveBtn':     'Guardar Configuración',
            'settings.saved':       'Configuración guardada.',

            'profile.title':          'Perfil',
            'profile.subtitle':       'Sus detalles de cuenta autenticados en AFMS.',
            'profile.userIdentity':   'Identidad de Usuario',
            'profile.username':       'Nombre de Usuario',
            'profile.role':           'Rol',
            'profile.sessionSec':     'Sesión y Seguridad',
            'profile.lastLogin':      'Último Inicio de Sesión',
            'profile.sessionExp':     'Sesión Expira En',
            'profile.sessionDur':     'Duración de Sesión',
            'profile.lastRefreshed':  'Última Actualización',
            'profile.changePwd':      'Cambiar Contraseña',
            'profile.currentPwd':     'Contraseña actual',
            'profile.newPwd':         'Nueva contraseña',
            'profile.confirmPwd':     'Confirmar nueva contraseña',
            'profile.updatePwd':      'Actualizar Contraseña',
            'profile.loginHistory':   'Historial de Inicio de Sesión',
            'profile.historyTs':      'Marca de tiempo (UTC)',
            'profile.historyIp':      'Dirección IP',
            'profile.noHistory':      'No hay historial de inicio de sesión disponible aún.',
            'profile.pwdChanged':     'Contraseña cambiada exitosamente.',
            'profile.hours':          'horas',

            'login.title':    'Inicio de Sesión AFMS',
            'login.subtitle': 'Inicia sesión para acceder al panel, gestión de vuelos y búsqueda avanzada.',
            'login.username': 'Nombre de Usuario',
            'login.password': 'Contraseña',
            'login.btn':      'Iniciar Sesión',
        },

        fr: {
            'nav.dashboard':      'Tableau de Bord',
            'nav.addFlight':      'Ajouter un Vol',
            'nav.flightDetails':  'Détails des Vols',
            'nav.advancedSearch': 'Recherche Avancée',
            'nav.collapse':       'Réduire',
            'nav.expand':         'Développer',
            'nav.logout':         'Déconnexion',
            'nav.loggedInAs':     'Connecté en tant que',

            'topbar.currentTime': 'Heure Actuelle',
            'topbar.profile':     'Profil',
            'topbar.settings':    'Paramètres',
            'topbar.logout':      'Déconnexion',

            'ai.title':               'Assistant de Vol',
            'ai.subtitle':            'Aide à la recherche de vols et de filtres',
            'ai.welcome.title':       'Assistant IA de Vol',
            'ai.welcome.text':        'Interrogez-moi sur les vols, les horaires et les informations aéroportuaires.',
            'ai.inputPlaceholder':    'Décrivez votre recherche de vol...',
            'ai.inputHint':           'Appuyez sur Entrée pour envoyer. Maj+Entrée ajoute une nouvelle ligne.',
            'ai.clearChat':           'Effacer la discussion',
            'ai.unavailable.title':   "L'Assistant de Vol n'est pas disponible ici",
            'ai.unavailable.text':    "Utilisez l'une de ces pages pour discuter avec l'assistant :",
            'ai.unavailable.goAdv':   'Aller à la Recherche Avancée',
            'ai.unavailable.goAdd':   'Aller à Ajouter un Vol',

            'dash.title':              'Tableau de Bord',
            'dash.subtitle':           'Aéroport de Londres Heathrow (LHR/EGLL) — Données de Vol en Direct',
            'dash.totalFlights':       'Total des Vols',
            'dash.departures':         'Départs',
            'dash.arrivals':           'Arrivées',
            'dash.airlines':           'Compagnies Aériennes',
            'dash.liveData':           'Données en Direct',
            'dash.dataUnavailable':    'Données Indisponibles',
            'dash.liveFlights':        'Vols en Direct',
            'dash.lastUpdated':        'Dernière mise à jour',
            'dash.filterAll':          'Tous',
            'dash.filterDep':          'Départs',
            'dash.filterArr':          'Arrivées',
            'dash.phFlight':           'Filtrer par numéro de vol',
            'dash.phAirline':          'Filtrer par compagnie',
            'dash.allStatuses':        'Tous les statuts',
            'dash.statusScheduled':    'Programmé',
            'dash.statusBoarding':     'Embarquement',
            'dash.statusDeparted':     'Parti',
            'dash.statusArrived':      'Arrivé',
            'dash.statusDelayed':      'Retardé',
            'dash.statusCanceled':     'Annulé',
            'dash.statusInFlight':     'En Vol',
            'dash.statusApproaching':  'En Approche',
            'dash.clearFilters':       'Effacer',
            'dash.colTime':            'HEURE',
            'dash.colFlight':          'VOL',
            'dash.colFrom':            'DE',
            'dash.colTo':              'À',
            'dash.colAirline':         'COMPAGNIE',
            'dash.colAircraft':        'APPAREIL',
            'dash.colStatus':          'STATUT',
            'dash.colManage':          'GÉRER',
            'dash.errorTitle':         'Données en direct indisponibles',
            'dash.errorText':          'Données de vol temporairement indisponibles — le quota se réinitialise bientôt.',
            'dash.retryBtn':           '↺ Réessayer',
            'dash.noDataTitle':        'Aucune donnée de vol en direct disponible',
            'dash.noDataText':         "L'API n'a pas renvoyé de vols pour la fenêtre de temps actuelle.",

            'flight.pageTitle':    'Vols Gérés',
            'flight.pageSubtitle': 'Rechercher, modifier ou supprimer des vols. Les modifications apparaissent en direct sur le tableau de bord.',
            'flight.addBtn':       '+ Ajouter un Nouveau Vol',
            'flight.phSearch':     'Ex. BA123, rechercher par numéro de vol...',
            'flight.searchBtn':    'Rechercher',
            'flight.clearSearch':  'Effacer',
            'flight.colNum':       'Vol #',
            'flight.colAirline':   'Compagnie',
            'flight.colDest':      'Destination',
            'flight.colAircraft':  'Appareil',
            'flight.colDep':       'Départ',
            'flight.colArr':       'Arrivée',
            'flight.colGate':      'Porte',
            'flight.colTerminal':  'Terminal',
            'flight.colStatus':    'Statut',
            'flight.colActions':   'Actions',
            'flight.viewBtn':      'Voir le Vol',
            'flight.editBtn':      'Modifier le Vol',
            'flight.deleteBtn':    'Supprimer le Vol',
            'flight.noFlights':    'Aucun vol trouvé',
            'flight.addFirst':     'Cliquez sur "Ajouter un Nouveau Vol" pour créer votre premier vol.',

            'settings.title':       'Paramètres',
            'settings.subtitle':    'Gérez vos préférences utilisateur AFMS.',
            'settings.prefs':       'Préférences',
            'settings.theme':       'Thème',
            'settings.themeLight':  'Clair',
            'settings.themeDark':   'Sombre',
            'settings.timeFormat':  "Format d'heure",
            'settings.time24':      '24 heures',
            'settings.time12':      '12 heures',
            'settings.language':    'Langue',
            'settings.langEn':      'Anglais',
            'settings.langEs':      'Espagnol',
            'settings.langFr':      'Français',
            'settings.saveBtn':     'Enregistrer les Paramètres',
            'settings.saved':       'Paramètres enregistrés.',

            'profile.title':          'Profil',
            'profile.subtitle':       'Vos détails de compte authentifié dans AFMS.',
            'profile.userIdentity':   'Identité Utilisateur',
            'profile.username':       "Nom d'Utilisateur",
            'profile.role':           'Rôle',
            'profile.sessionSec':     'Session et Sécurité',
            'profile.lastLogin':      'Dernière Connexion',
            'profile.sessionExp':     'Expiration de Session',
            'profile.sessionDur':     'Durée de Session',
            'profile.lastRefreshed':  'Dernière Actualisation',
            'profile.changePwd':      'Changer de Mot de Passe',
            'profile.currentPwd':     'Mot de passe actuel',
            'profile.newPwd':         'Nouveau mot de passe',
            'profile.confirmPwd':     'Confirmer le nouveau mot de passe',
            'profile.updatePwd':      'Mettre à Jour le Mot de Passe',
            'profile.loginHistory':   'Historique de Connexion',
            'profile.historyTs':      'Horodatage (UTC)',
            'profile.historyIp':      'Adresse IP',
            'profile.noHistory':      "Aucun historique de connexion disponible pour l'instant.",
            'profile.pwdChanged':     'Mot de passe modifié avec succès.',
            'profile.hours':          'heures',

            'login.title':    'Connexion AFMS',
            'login.subtitle': 'Connectez-vous pour accéder au tableau de bord, à la gestion des vols et à la recherche avancée.',
            'login.username': "Nom d'Utilisateur",
            'login.password': 'Mot de Passe',
            'login.btn':      'Connexion',
        }
    };

    /* ── Helpers ─────────────────────────────────────────────────── */
    function readCookie(name) {
        var m = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]+)'));
        return m ? decodeURIComponent(m[1]) : null;
    }

    function getCurrentLang() {
        var ls = localStorage.getItem('afms-language');
        if (ls && T[ls]) return ls;
        var ck = readCookie('afms_language');
        if (ck && T[ck]) return ck;
        return 'en';
    }

    /* ── Apply translations to the DOM ─────────────────────────── */
    function applyTranslations(lang) {
        var dict = T[lang] || T['en'];

        /* text content */
        document.querySelectorAll('[data-i18n]').forEach(function (el) {
            var v = dict[el.getAttribute('data-i18n')];
            if (v !== undefined) el.textContent = v;
        });

        /* placeholder attribute */
        document.querySelectorAll('[data-i18n-placeholder]').forEach(function (el) {
            var v = dict[el.getAttribute('data-i18n-placeholder')];
            if (v !== undefined) el.placeholder = v;
        });

        /* title attribute */
        document.querySelectorAll('[data-i18n-title]').forEach(function (el) {
            var v = dict[el.getAttribute('data-i18n-title')];
            if (v !== undefined) el.title = v;
        });

        /* innerHTML (for elements containing anchor tags etc.) */
        document.querySelectorAll('[data-i18n-html]').forEach(function (el) {
            var v = dict[el.getAttribute('data-i18n-html')];
            if (v !== undefined) el.innerHTML = v;
        });

        /* update <html lang> attribute */
        document.documentElement.lang = lang;
    }

    /* ── Public API ──────────────────────────────────────────────── */
    window.afmsI18n = {
        apply: applyTranslations,
        lang:  getCurrentLang,
        dict:  T
    };

    /* Auto-apply on DOM ready */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            applyTranslations(getCurrentLang());
        });
    } else {
        applyTranslations(getCurrentLang());
    }
})();
