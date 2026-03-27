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
