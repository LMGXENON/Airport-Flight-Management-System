(function () {
    const path = window.location.pathname.toLowerCase();
    const isAdvancedSearch = path.includes('advancedsearch');

    document.addEventListener('DOMContentLoaded', function () {
        const toggle     = document.getElementById('aiChatToggle');
        const chatWindow = document.getElementById('aiChatWindow');
        const closeBtn   = document.getElementById('aiChatClose');
        const clearBtn   = document.getElementById('aiChatClear');
        const sendBtn    = document.getElementById('chatSendBtn');
        const inputEl    = document.getElementById('chatInput');
        const messages   = document.getElementById('chatMessages');
        let isBusy       = false;

        if (!toggle || !chatWindow) return;

        const welcome = messages.querySelector('.welcome-message');
        const welcomePara = chatWindow.querySelector('.welcome-message p');
        const welcomeTitle = chatWindow.querySelector('.welcome-message h4');
        if (welcomePara) {
            welcomePara.textContent = isAdvancedSearch
                ? 'Describe the flight you want to find and I\'ll apply search filters. I only handle flight search requests.'
                : 'The AI assistant is only available on the Advanced Search page.';
        }

        if (welcomeTitle && isAdvancedSearch) {
            welcomeTitle.textContent = 'Flight Search Assistant';
        }

        function setWindowOpen(isOpen) {
            chatWindow.classList.toggle('active', isOpen);
            toggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            if (isOpen && inputEl && !inputEl.disabled) {
                inputEl.focus();
            }
        }

        function showWelcomeState() {
            if (!welcome) return;
            welcome.style.display = '';
            messages.style.justifyContent = 'center';
            messages.style.alignItems = 'center';
        }

        function hideWelcomeState() {
            if (welcome) welcome.style.display = 'none';
            messages.style.justifyContent = 'flex-start';
            messages.style.alignItems = 'flex-start';
        }

        function setBusyState(busy) {
            isBusy = busy;
            sendBtn.disabled = busy || !isAdvancedSearch;
            if (inputEl) {
                inputEl.disabled = busy || !isAdvancedSearch;
                if (busy) {
                    inputEl.placeholder = 'Working on your search request...';
                } else if (isAdvancedSearch) {
                    inputEl.placeholder = 'Describe your flight search...';
                }
            }

            if (clearBtn) {
                clearBtn.disabled = busy;
            }
        }

        toggle.addEventListener('click', () => setWindowOpen(!chatWindow.classList.contains('active')));
        if (closeBtn) closeBtn.addEventListener('click', () => setWindowOpen(false));

        document.addEventListener('keydown', event => {
            if (event.key === 'Escape' && chatWindow.classList.contains('active')) {
                setWindowOpen(false);
            }
        });

        if (!isAdvancedSearch) {
            if (inputEl) {
                inputEl.disabled = true;
                inputEl.placeholder = 'Only available on Advanced Search';
            }
            if (sendBtn) sendBtn.disabled = true;
            if (clearBtn) clearBtn.disabled = true;
            return;
        }

        showWelcomeState();

        function escapeHtml(str) {
            return str.replace(/&/g, '&amp;')
                      .replace(/</g, '&lt;')
                      .replace(/>/g, '&gt;')
                      .replace(/"/g, '&quot;');
        }

        function appendMessage(role, html, tone = '') {
            hideWelcomeState();

            const wrap = document.createElement('div');
            wrap.className = `chat-message ${role}-message${tone ? ` ${tone}-message` : ''}`;
            wrap.innerHTML = `<div class="message-bubble">${html}</div>`;
            messages.appendChild(wrap);
            messages.scrollTop = messages.scrollHeight;
            return wrap;
        }

        function appendThinking() {
            hideWelcomeState();

            const wrap = document.createElement('div');
            wrap.className = 'chat-message assistant-message thinking';
            wrap.innerHTML = '<div class="message-bubble"><div class="thinking-row"><span class="dot-flashing"></span><span class="thinking-label">Thinking...</span></div><small>Reading your request and turning it into search filters.</small></div>';
            messages.appendChild(wrap);
            messages.scrollTop = messages.scrollHeight;
            return wrap;
        }

        function clearChat() {
            messages.querySelectorAll('.chat-message').forEach(node => node.remove());
            showWelcomeState();
            if (inputEl) {
                inputEl.value = '';
                inputEl.style.height = 'auto';
                inputEl.focus();
            }
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
                    const isActive = b.dataset.direction === params.direction;
                    b.classList.toggle('active', isActive);
                    b.setAttribute('aria-pressed', isActive ? 'true' : 'false');
                });
                const dirInput = document.getElementById('directionInput');
                if (dirInput) dirInput.value = params.direction;
            }

            const selectedStatuses = Array.isArray(params.statuses) ? params.statuses : [];
            document.querySelectorAll('#statusButtons .status-btn').forEach(b => {
                const isActive = selectedStatuses.includes(b.dataset.status);
                b.classList.toggle('active', isActive);
                b.setAttribute('aria-pressed', isActive ? 'true' : 'false');
            });

            if (typeof window.syncAdvancedSearchSelections === 'function') {
                window.syncAdvancedSearchSelections();
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

        function hasSearchFilters(params) {
            return Boolean(
                params.flight || params.airline || params.destination ||
                params.departureDate || params.arrivalDate || params.terminal ||
                params.direction || params.timeRangeStart || params.timeRangeEnd ||
                (Array.isArray(params.statuses) && params.statuses.length)
            );
        }

        async function callDeepSeek(userMessage) {
            const controller = new AbortController();
            const timeoutId = window.setTimeout(() => controller.abort(), 15000);

            const res = await fetch('/Home/ProcessAIQuery', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ query: userMessage }),
                signal: controller.signal
            });

            window.clearTimeout(timeoutId);

            if (!res.ok) {
                let errorText = '';
                try {
                    const payload = await res.json();
                    errorText = payload?.error || payload?.details || '';
                } catch {
                    errorText = await res.text();
                }

                const error = new Error(errorText || `Server returned ${res.status}`);
                error.status = res.status;
                throw error;
            }

            return await res.json();
        }

        function getFriendlyErrorMessage(err) {
            if (err?.name === 'AbortError') {
                return 'That took too long. Try a shorter request like “British Airways departures today” or send it again.';
            }

            if (err?.status === 429) {
                return 'The assistant is busy right now. Wait a few seconds and try again.';
            }

            if (err?.status === 400) {
                return 'I could not turn that into search filters. Try naming an airline, destination, date, time, terminal, or status.';
            }

            if (err?.status >= 500) {
                return 'The assistant hit a server problem. Try again in a moment, or use the filters manually.';
            }

            return 'I could not process that search request. Try rephrasing it more clearly, for example “arrivals from Doha today after 18:00”.';
        }

        async function handleSend() {
            const text = inputEl.value.trim();
            if (!text || isBusy) return;

            inputEl.value = '';
            inputEl.style.height = 'auto';
            setBusyState(true);

            appendMessage('user', escapeHtml(text));
            const thinkingEl = appendThinking();

            try {
                const params = await callDeepSeek(text);
                thinkingEl.remove();

                if (params.isSearchRequest === false || !hasSearchFilters(params)) {
                    appendMessage('assistant', escapeHtml(params.message || 'I can only help with flight searches. Try asking by airline, destination, flight number, date, time, terminal or status.'), 'info');
                    return;
                }

                window.scrollTo({ top: 0, behavior: 'smooth' });
                const clearBtn = document.getElementById('clearFiltersBtn');
                if (clearBtn) clearBtn.click();

                fillFormFields(params);
                appendMessage('assistant', buildSummary(params), 'success');
                setTimeout(triggerSearch, 350);
            } catch (err) {
                thinkingEl.remove();
                appendMessage('assistant', escapeHtml(getFriendlyErrorMessage(err)), 'error');
                console.error('[AI Chat] DeepSeek error:', err);
            } finally {
                setBusyState(false);
                inputEl.focus();
            }
        }

        if (clearBtn) {
            clearBtn.addEventListener('click', clearChat);
        }

        sendBtn.addEventListener('click', handleSend);
        inputEl.addEventListener('keydown', e => {
            if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSend(); }
        });

        inputEl.addEventListener('input', () => {
            inputEl.style.height = 'auto';
            inputEl.style.height = Math.min(inputEl.scrollHeight, 120) + 'px';
        });

        setBusyState(false);
    });
})();
