// Clock update
function toLocalDateTimeValue(date) {
    var offset = date.getTimezoneOffset();
    var localTime = new Date(date.getTime() - offset * 60000);
    return localTime.toISOString().slice(0, 19);
}

function updateClock() {
    var now = new Date();
    var hours = String(now.getHours()).padStart(2, '0');
    var minutes = String(now.getMinutes()).padStart(2, '0');
    var seconds = String(now.getSeconds()).padStart(2, '0');
    var el = document.getElementById('currentTime');
    if (el) {
        el.textContent = hours + ':' + minutes + ':' + seconds;
        el.setAttribute('datetime', toLocalDateTimeValue(now));
        el.setAttribute('aria-label', 'Current time ' + hours + ':' + minutes + ':' + seconds);
    }
}
setInterval(updateClock, 1000);
updateClock();

// Form submission handlers to prevent double-submission and provide feedback
(function () {
    // Reset Search button logic for Advanced Search form
    document.addEventListener('DOMContentLoaded', function () {
        var resetBtn = document.getElementById('resetSearchBtn');
        var searchForm = document.getElementById('searchForm');
        if (resetBtn && searchForm) {
            resetBtn.addEventListener('click', function () {
                // Clear all input fields
                searchForm.reset();
                // Clear datalist-backed fields manually
                var inputs = searchForm.querySelectorAll('input[type="text"], input[type="date"], input[type="time"]');
                inputs.forEach(function (input) { input.value = ''; });
                // Clear selects
                var selects = searchForm.querySelectorAll('select');
                selects.forEach(function (select) { select.selectedIndex = 0; });
                // Clear hidden fields
                var hiddens = searchForm.querySelectorAll('input[type="hidden"]');
                hiddens.forEach(function (hidden) { if (hidden.name !== 'search') hidden.value = ''; });
                // Remove all status filter selections
                var statusBtns = document.querySelectorAll('.status-btn.active');
                statusBtns.forEach(function (btn) { btn.classList.remove('active'); btn.setAttribute('aria-pressed', 'false'); });
                var statusInputs = searchForm.querySelectorAll('.status-hidden-input');
                statusInputs.forEach(function (input) { input.remove(); });
                // Reset direction toggle
                var directionBtns = document.querySelectorAll('.direction-btn');
                directionBtns.forEach(function (btn) { btn.classList.remove('active'); btn.setAttribute('aria-pressed', 'false'); });
                var directionInput = document.getElementById('directionInput');
                if (directionInput) directionInput.value = '';

                // Focus the first input for better UX
                var firstInput = searchForm.querySelector('input:not([type=hidden]):not([type=submit]):not([type=button]), select');
                if (firstInput) firstInput.focus();
            });
        }
    });
    document.addEventListener('submit', function (e) {
        var form = e.target;
        var submitBtn = form.querySelector('button[type="submit"]');
        
        if (form.classList.contains('logout-form') || form.classList.contains('login-form')) {
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.disabled = true;
                var originalText = submitBtn.textContent;
                submitBtn.textContent = form.classList.contains('logout-form') ? 'Logging out...' : 'Logging in...';
                
                // Re-enable if form submission fails (e.g., validation errors)
                setTimeout(function () {
                    if (form.querySelector('input.input-validation-error')) {
                        submitBtn.disabled = false;
                        submitBtn.textContent = originalText;
                    }
                }, 500);
            }
        }
    });
})();

// Show loading spinner for flights table during fetches
document.addEventListener('DOMContentLoaded', function () {
    var flightsSection = document.querySelector('.flights-section');
    var spinner = document.getElementById('flightsLoadingSpinner');
    if (flightsSection && spinner) {
        // Show spinner on search form submit
        var searchForm = document.querySelector('.flight-search-form');
        if (searchForm) {
            searchForm.addEventListener('submit', function () {
                spinner.style.display = 'flex';
            });
        }
        // Show spinner on pagination link click
        var pagination = document.querySelector('.pagination');
        if (pagination) {
            pagination.addEventListener('click', function (e) {
                var target = e.target;
                if (target.tagName === 'A' && target.classList.contains('page-btn')) {
                    spinner.style.display = 'flex';
                }
            });
        }

        // (Optional) If flights table is ever loaded via AJAX, expose a global function to show/hide spinner
        window.showFlightsLoadingSpinner = function () {
            spinner.style.display = 'flex';
        };
        window.hideFlightsLoadingSpinner = function () {
            spinner.style.display = 'none';
        };
    }
});

// Sidebar toggle for mobile
(function () {
    var sidebar = document.getElementById('sidebar');
    var toggle = document.getElementById('sidebarToggle');
    var closeBtn = document.getElementById('sidebarClose');

    if (toggle && sidebar) {
        toggle.addEventListener('click', function () {
            sidebar.classList.toggle('open');
        });
    }

    if (closeBtn && sidebar) {
        closeBtn.addEventListener('click', function () {
            sidebar.classList.remove('open');
        });
    }
})();

// Avatar dropdown menu
(function () {
    var container = document.getElementById('avatarMenuContainer');
    var button = document.getElementById('avatarMenuButton');
    var menu = document.getElementById('avatarDropdownMenu');

    if (!container || !button || !menu) {
        return;
    }

    function setMenuOpen(isOpen) {
        button.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        menu.hidden = !isOpen;
    }

    button.addEventListener('click', function () {
        setMenuOpen(menu.hidden);
    });

    document.addEventListener('click', function (event) {
        if (!container.contains(event.target)) {
            setMenuOpen(false);
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            setMenuOpen(false);
        }
    });
})();

// Theme toggle (dark/light)
(function () {
    var themeToggle = document.getElementById('themeToggle');
    if (!themeToggle) {
        return;
    }

    var themeIcon = themeToggle.querySelector('.theme-toggle-icon');
    var themeText = themeToggle.querySelector('.theme-toggle-text');
    var prefersLight = window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches;
    var savedTheme = localStorage.getItem('afms-theme');
    var currentTheme = savedTheme === 'light' || savedTheme === 'dark'
        ? savedTheme
        : (prefersLight ? 'light' : 'dark');

    function renderToggle(theme) {
        var isLight = theme === 'light';
        themeToggle.setAttribute('aria-pressed', isLight ? 'true' : 'false');
        themeToggle.setAttribute('aria-label', isLight ? 'Switch to dark mode' : 'Switch to light mode');

        if (themeIcon) {
            themeIcon.textContent = isLight ? '☀' : '☾';
        }

        if (themeText) {
            themeText.textContent = isLight ? 'Light' : 'Dark';
        }
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('afms-theme', theme);
        renderToggle(theme);
    }

    applyTheme(currentTheme);

    themeToggle.addEventListener('click', function () {
        currentTheme = document.documentElement.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
        applyTheme(currentTheme);
    });
})();
