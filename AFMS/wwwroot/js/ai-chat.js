// AI Chat Widget - Simple Toggle
document.addEventListener('DOMContentLoaded', function() {
    const toggle = document.getElementById('aiChatToggle');
    const window = document.getElementById('aiChatWindow');
    const close = document.getElementById('aiChatClose');

    if (toggle && window) {
        toggle.addEventListener('click', () => window.classList.toggle('active'));
    }

    if (close && window) {
        close.addEventListener('click', () => window.classList.remove('active'));
    }
});
