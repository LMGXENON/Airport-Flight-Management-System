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

// Form submission handlers to prevent double-submission and provide feedback
(function () {
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
