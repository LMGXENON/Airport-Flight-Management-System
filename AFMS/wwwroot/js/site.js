// Clock update
function updateClock() {
    var now = new Date();
    var hours = String(now.getHours()).padStart(2, '0');
    var minutes = String(now.getMinutes()).padStart(2, '0');
    var seconds = String(now.getSeconds()).padStart(2, '0');
    var el = document.getElementById('currentTime');
    if (el) {
        el.textContent = hours + ':' + minutes + ':' + seconds;
    }
}
setInterval(updateClock, 1000);
updateClock();

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
