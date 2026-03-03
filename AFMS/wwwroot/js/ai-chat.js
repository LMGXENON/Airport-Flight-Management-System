(function () {
    const path = window.location.pathname.toLowerCase();
    const isAdvancedSearch = path.includes('advancedsearch');

    document.addEventListener('DOMContentLoaded', function () {
        const toggle     = document.getElementById('aiChatToggle');
        const chatWindow = document.getElementById('aiChatWindow');
        const closeBtn   = document.getElementById('aiChatClose');
        const sendBtn    = document.getElementById('chatSendBtn');
        const inputEl    = document.getElementById('chatInput');
        const messages   = document.getElementById('chatMessages');

        if (!toggle || !chatWindow) return;

        const welcomePara = chatWindow.querySelector('.welcome-message p');
        if (welcomePara) {
            welcomePara.textContent = isAdvancedSearch
                ? 'Describe the flight you\'re looking for and I\'ll search for it. e.g. "Flights from Heathrow to Dubai tomorrow"'
                : 'The AI assistant is only available on the Advanced Search page.';
        }

        toggle.addEventListener('click', () => chatWindow.classList.toggle('active'));
        if (closeBtn) closeBtn.addEventListener('click', () => chatWindow.classList.remove('active'));

        if (!isAdvancedSearch) {
            if (inputEl) {
                inputEl.disabled = true;
                inputEl.placeholder = 'Only available on Advanced Search';
            }
            if (sendBtn) sendBtn.disabled = true;
            return;
        }

        messages.style.justifyContent = 'flex-start';
        messages.style.alignItems     = 'flex-start';

        function escapeHtml(str) {
            return str.replace(/&/g, '&amp;')
                      .replace(/</g, '&lt;')
                      .replace(/>/g, '&gt;')
                      .replace(/"/g, '&quot;');
        }

        function appendMessage(role, html) {
            const welcome = messages.querySelector('.welcome-message');
            if (welcome) welcome.style.display = 'none';

            const wrap = document.createElement('div');
            wrap.className = `chat-message ${role}-message`;
            wrap.innerHTML = `<div class="message-bubble">${html}</div>`;
            messages.appendChild(wrap);
            messages.scrollTop = messages.scrollHeight;
            return wrap;
        }

        function appendThinking() {
            const welcome = messages.querySelector('.welcome-message');
            if (welcome) welcome.style.display = 'none';

            const wrap = document.createElement('div');
            wrap.className = 'chat-message assistant-message thinking';
            wrap.innerHTML = '<div class="message-bubble"><span class="dot-flashing"></span></div>';
            messages.appendChild(wrap);
            messages.scrollTop = messages.scrollHeight;
            return wrap;
        }

        function fillFormFields(params) {
            const set = (id, val) => {
                const el = document.getElementById(id);
                if (el && val !== undefined && val !== null && val !== '') el.value = val;
            };

            set('flight',         params.flight);
            set('airline',        params.airline);
            set('destination',    params.destination);
            set('departureDate',  params.departureDate);
            set('arrivalDate',    params.arrivalDate);
            set('terminal',       params.terminal);
            set('timeRangeStart', params.timeRangeStart);
            set('timeRangeEnd',   params.timeRangeEnd);

            if (params.direction !== undefined) {
                document.querySelectorAll('.direction-btn').forEach(b => {
                    b.classList.toggle('active', b.dataset.direction === params.direction);
                });
                const dirInput = document.getElementById('directionInput');
                if (dirInput) dirInput.value = params.direction;
            }

            if (Array.isArray(params.statuses) && params.statuses.length > 0) {
                document.querySelectorAll('#statusButtons .status-btn').forEach(b => {
                    b.classList.toggle('active', params.statuses.includes(b.dataset.status));
                });
            }
        }

        function triggerSearch() {
            const form = document.getElementById('searchForm');
            if (form) form.dispatchEvent(new Event('submit', { cancelable: true, bubbles: true }));
        }

        function buildSummary(params) {
            const parts = [];
            if (params.direction === 'Departure') parts.push('departing Heathrow');
            else if (params.direction === 'Arrival') parts.push('arriving at Heathrow');
            if (params.destination)   parts.push(`to/from <strong>${params.destination}</strong>`);
            if (params.airline)       parts.push(`on <strong>${params.airline}</strong>`);
            if (params.flight)        parts.push(`flight <strong>${params.flight}</strong>`);
            if (params.departureDate) parts.push(`departing <strong>${params.departureDate}</strong>`);
            if (params.arrivalDate)   parts.push(`arriving <strong>${params.arrivalDate}</strong>`);
            if (params.terminal)      parts.push(`terminal <strong>${params.terminal}</strong>`);
            if (params.timeRangeStart || params.timeRangeEnd) {
                parts.push(`between <strong>${params.timeRangeStart ?? '00:00'} – ${params.timeRangeEnd ?? '23:59'}</strong>`);
            }
            if (Array.isArray(params.statuses) && params.statuses.length > 0) {
                parts.push(`status: <strong>${params.statuses.join(', ')}</strong>`);
            }
            const summary = parts.length
                ? `Searching for flights ${parts.join(', ')}…`
                : 'Searching with those filters…';
            return `${summary}<br><small style="opacity:.65">Filters applied — results loading below.</small>`;
        }

        async function callDeepSeek(userMessage) {
            const res = await fetch('/Home/ProcessAIQuery', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ query: userMessage })
            });

            if (!res.ok) throw new Error(`Server returned ${res.status}`);
            return await res.json();
        }

        async function handleSend() {
            const text = inputEl.value.trim();
            if (!text) return;

            inputEl.value = '';
            inputEl.style.height = 'auto';
            sendBtn.disabled = true;

            appendMessage('user', escapeHtml(text));
            const thinkingEl = appendThinking();

            try {
                const params = await callDeepSeek(text);
                thinkingEl.remove();

                window.scrollTo({ top: 0, behavior: 'smooth' });
                const clearBtn = document.getElementById('clearFiltersBtn');
                if (clearBtn) clearBtn.click();

                fillFormFields(params);
                appendMessage('assistant', buildSummary(params));
                setTimeout(triggerSearch, 350);
            } catch (err) {
                thinkingEl.remove();
                appendMessage('assistant', '⚠️ Sorry, I couldn\'t process that. Please try rephrasing.');
                console.error('[AI Chat] DeepSeek error:', err);
            } finally {
                sendBtn.disabled = false;
                inputEl.focus();
            }
        }

        sendBtn.addEventListener('click', handleSend);
        inputEl.addEventListener('keydown', e => {
            if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSend(); }
        });

        inputEl.addEventListener('input', () => {
            inputEl.style.height = 'auto';
            inputEl.style.height = Math.min(inputEl.scrollHeight, 120) + 'px';
        });
    });
})();
