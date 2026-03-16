(function () {
    const path = window.location.pathname.toLowerCase();
    const isAdvancedSearch = path.includes('advancedsearch');
    const isAddFlight = path.includes('/flight/add');
    const isAiEnabledPage = isAdvancedSearch || isAddFlight;

    document.addEventListener('DOMContentLoaded', function () {
        const toggle     = document.getElementById('aiChatToggle');
        const chatWindow = document.getElementById('aiChatWindow');
        const closeBtn   = document.getElementById('aiChatClose');
        const clearBtn   = document.getElementById('aiChatClear');
        const sendBtn    = document.getElementById('chatSendBtn');
        const inputEl    = document.getElementById('chatInput');
        const inputHint  = document.getElementById('chatInputHint');
        const messages   = document.getElementById('chatMessages');
        let isBusy       = false;
        let activeThinkingEl = null;
        let awaitingAddFlightConfirmation = false;

        if (!toggle || !chatWindow) return;

        const welcome = messages.querySelector('.welcome-message');
        const welcomePara = chatWindow.querySelector('.welcome-message p');
        const welcomeTitle = chatWindow.querySelector('.welcome-message h4');

        function getReadyPlaceholder() {
            if (isAdvancedSearch) return 'Describe your flight search...';
            if (isAddFlight) return 'e.g. BA123 to JFK tomorrow at 14:30';
            return 'AI assistant unavailable on this page';
        }

        if (welcomePara) {
            if (isAdvancedSearch) {
                welcomePara.textContent = 'Describe the flight you want to find and I\'ll apply search filters. I only handle flight search requests.';
            } else if (isAddFlight) {
                welcomePara.textContent = 'Provide flight number, airline, destination, and departure time. I\'ll estimate arrival, gate, and terminal.';
            } else {
                welcomePara.textContent = 'The AI assistant is only available on the Advanced Search and Add Flight pages.';
            }
        }

        if (welcomeTitle) {
            if (isAdvancedSearch) {
                welcomeTitle.textContent = 'Flight Search Assistant';
            } else if (isAddFlight) {
                welcomeTitle.textContent = 'Add Flight Assistant';
            }
        }

        if (inputHint) {
            inputHint.textContent = isAddFlight
                ? 'Required: flight number, airline, destination, departure time. Press Enter to send.'
                : 'Press Enter to send. Shift+Enter adds a new line.';
        }

        if (inputEl) {
            inputEl.placeholder = getReadyPlaceholder();
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
            sendBtn.disabled = busy || !isAiEnabledPage;
            if (inputEl) {
                inputEl.disabled = busy || !isAiEnabledPage;
                if (busy) {
                    inputEl.placeholder = isAddFlight
                        ? 'Preparing flight details...'
                        : 'Working on your search request...';
                } else {
                    inputEl.placeholder = getReadyPlaceholder();
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

        if (!isAiEnabledPage) {
            if (inputEl) {
                inputEl.disabled = true;
                inputEl.placeholder = getReadyPlaceholder();
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

        function clearThinkingIndicators() {
            messages.querySelectorAll('.chat-message.assistant-message.thinking').forEach(node => node.remove());
            activeThinkingEl = null;
        }

        function clearAddFlightConfirmation() {
            awaitingAddFlightConfirmation = false;
        }

        function appendThinking() {
            hideWelcomeState();

            clearThinkingIndicators();

            const wrap = document.createElement('div');
            wrap.className = 'chat-message assistant-message thinking';
            wrap.innerHTML = '<div class="message-bubble"><div class="thinking-row"><span class="thinking-dots" aria-hidden="true"><span class="thinking-dot"></span><span class="thinking-dot"></span><span class="thinking-dot"></span></span><span class="thinking-label">Thinking...</span></div><small>Reading your request and preparing the reply.</small></div>';
            messages.appendChild(wrap);
            messages.scrollTop = messages.scrollHeight;
            activeThinkingEl = wrap;
            return wrap;
        }

        function clearChat() {
            clearThinkingIndicators();
            clearAddFlightConfirmation();
            messages.querySelectorAll('.chat-message').forEach(node => node.remove());
            showWelcomeState();
            if (inputEl) {
                inputEl.value = '';
                inputEl.style.height = 'auto';
                inputEl.focus();
            }
        }

        function fillFormFields(params) {
            const clearFields = new Set((Array.isArray(params.clearFields) ? params.clearFields : [])
                .map(field => String(field).toLowerCase()));
            const shouldClear = fieldName => clearFields.has(String(fieldName).toLowerCase());

            const set = (id, val, fieldName = id) => {
                const el = document.getElementById(id);
                if (!el) return;

                if (shouldClear(fieldName)) {
                    el.value = '';
                    return;
                }

                if (val !== undefined && val !== null && val !== '') el.value = val;
            };

            set('flight',         params.flight, 'flight');
            set('airline',        params.airline, 'airline');
            set('destination',    params.destination, 'destination');
            set('departureDate',  params.departureDate, 'departureDate');
            set('arrivalDate',    params.arrivalDate, 'arrivalDate');
            set('terminal',       params.terminal, 'terminal');
            set('timeRangeStart', params.timeRangeStart, 'timeRangeStart');
            set('timeRangeEnd',   params.timeRangeEnd, 'timeRangeEnd');

            if (params.direction !== undefined || shouldClear('direction')) {
                const selectedDirection = shouldClear('direction') ? '' : params.direction;
                document.querySelectorAll('.direction-btn').forEach(b => {
                    const isActive = b.dataset.direction === selectedDirection;
                    b.classList.toggle('active', isActive);
                    b.setAttribute('aria-pressed', isActive ? 'true' : 'false');
                });
                const dirInput = document.getElementById('directionInput');
                if (dirInput) dirInput.value = selectedDirection;
            }

            const selectedStatuses = shouldClear('statuses')
                ? []
                : (Array.isArray(params.statuses) ? params.statuses : []);
            document.querySelectorAll('#statusButtons .status-btn').forEach(b => {
                const isActive = selectedStatuses.includes(b.dataset.status);
                b.classList.toggle('active', isActive);
                b.setAttribute('aria-pressed', isActive ? 'true' : 'false');
            });

            if (typeof window.syncAdvancedSearchSelections === 'function') {
                window.syncAdvancedSearchSelections();
            }
        }

        function fillAddFlightFields(params) {
            const setInput = (id, val) => {
                const el = document.getElementById(id);
                if (!el || val === undefined || val === null || val === '') return;
                el.value = val;
            };

            const setSelect = (id, val) => {
                const el = document.getElementById(id);
                if (!el || val === undefined || val === null || val === '') return;

                const optionExists = Array.from(el.options).some(option =>
                    option.value.toLowerCase() === String(val).toLowerCase());

                if (!optionExists && id === 'Airline') {
                    const option = document.createElement('option');
                    option.value = val;
                    option.textContent = val;
                    el.appendChild(option);
                }

                el.value = val;
            };

            setInput('FlightNumber', params.flightNumber);
            setSelect('Airline', params.airline);
            setInput('Destination', params.destination);
            setInput('DepartureTime', params.departureTime);
            setInput('ArrivalTime', params.arrivalTime);
            setInput('Gate', params.gate);
            setSelect('Terminal', params.terminal);
        }

        function getAddFlightForm() {
            return document.getElementById('addFlightForm')
                || document.querySelector('form[action$="/Flight/Add"]')
                || document.querySelector('form[action="/Flight/Add"]')
                || document.querySelector('.form-card form');
        }

        function hasRequiredAddFlightFormValues() {
            const requiredIds = ['FlightNumber', 'Airline', 'Destination', 'DepartureTime'];
            return requiredIds.every(id => {
                const el = document.getElementById(id);
                return el && String(el.value || '').trim().length > 0;
            });
        }

        function isAffirmativeResponse(text) {
            return /^(yes|y|yeah|yep|sure|ok|okay|confirm|save|submit|go ahead|do it|please do)\b/i.test(text.trim());
        }

        function isNegativeResponse(text) {
            return /^(no|n|nope|nah|cancel|not now|don't save|do not save|keep editing)\b/i.test(text.trim());
        }

        function submitAddFlightForm() {
            const form = getAddFlightForm();
            if (!form) {
                appendMessage('assistant', 'I could not find the Add Flight form to submit. Please use the Save Flight button.', 'error');
                return;
            }

            if (typeof form.requestSubmit === 'function') {
                form.requestSubmit();
                return;
            }

            form.submit();
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

        function hasAddFlightFields(params) {
            return Boolean(
                params.flightNumber || params.airline || params.destination ||
                params.departureTime || params.arrivalTime || params.gate || params.terminal
            );
        }

        function getSearchContext() {
            if (!isAdvancedSearch) return null;

            const readTrimmed = id => {
                const el = document.getElementById(id);
                return el ? (el.value || '').trim() : '';
            };

            const statuses = Array.from(document.querySelectorAll('#statusButtons .status-btn.active'))
                .map(btn => btn.dataset.status)
                .filter(Boolean);

            const directionInput = document.getElementById('directionInput');
            const context = {
                flight: readTrimmed('flight'),
                airline: readTrimmed('airline'),
                destination: readTrimmed('destination'),
                departureDate: readTrimmed('departureDate'),
                arrivalDate: readTrimmed('arrivalDate'),
                terminal: readTrimmed('terminal'),
                direction: directionInput ? (directionInput.value || '').trim() : '',
                statuses,
                timeRangeStart: readTrimmed('timeRangeStart'),
                timeRangeEnd: readTrimmed('timeRangeEnd')
            };

            const hasAny = Boolean(
                context.flight || context.airline || context.destination ||
                context.departureDate || context.arrivalDate || context.terminal ||
                context.direction || context.timeRangeStart || context.timeRangeEnd ||
                context.statuses.length
            );

            return hasAny ? context : null;
        }

        function getAddFlightContext() {
            if (!isAddFlight) return null;

            const readValue = id => {
                const el = document.getElementById(id);
                return el ? (el.value || '').trim() : '';
            };

            const context = {
                flightNumber: readValue('FlightNumber'),
                airline: readValue('Airline'),
                destination: readValue('Destination'),
                departureTime: readValue('DepartureTime'),
                arrivalTime: readValue('ArrivalTime'),
                gate: readValue('Gate'),
                terminal: readValue('Terminal')
            };

            const hasAny = Object.values(context).some(Boolean);
            return hasAny ? context : null;
        }

        async function callAssistant(endpoint, payload) {
            const controller = new AbortController();
            const timeoutId = window.setTimeout(() => controller.abort(), 15000);

            const res = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
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

        async function callSearchAssistant(userMessage) {
            return await callAssistant('/Home/ProcessAIQuery', {
                query: userMessage,
                searchContext: getSearchContext()
            });
        }

        async function callAddFlightAssistant(userMessage) {
            return await callAssistant('/Home/ProcessAddFlightQuery', {
                query: userMessage,
                addFlightContext: getAddFlightContext()
            });
        }

        function getFriendlyErrorMessage(err) {
            if (err?.name === 'AbortError') {
                return isAddFlight
                    ? 'That took too long. Try a shorter request like "BA123 to JFK tomorrow 14:30".'
                    : 'That took too long. Try a shorter request like "British Airways departures today" or send it again.';
            }

            if (err?.status === 429) {
                return 'The assistant is busy right now. Wait a few seconds and try again.';
            }

            if (err?.status === 400) {
                return isAddFlight
                    ? 'I could not map that to add-flight fields. Include flight number, airline, destination, and departure time.'
                    : 'I could not turn that into search filters. Try naming an airline, destination, date, time, terminal, or status.';
            }

            if (err?.status >= 500) {
                return isAddFlight
                    ? 'The assistant hit a server problem. Try again in a moment, then review fields manually.'
                    : 'The assistant hit a server problem. Try again in a moment, or use the filters manually.';
            }

            return isAddFlight
                ? 'I could not process that add-flight request. Try "BA123 British Airways to JFK tomorrow 14:30".'
                : 'I could not process that search request. Try rephrasing it more clearly, for example "arrivals from Doha today after 18:00".';
        }

        function getMissingFieldLabel(fieldName) {
            switch ((fieldName || '').toLowerCase()) {
                case 'flightnumber':
                    return 'flight number';
                case 'airline':
                    return 'airline';
                case 'destination':
                    return 'destination';
                case 'departuretime':
                    return 'departure time';
                default:
                    return fieldName;
            }
        }

        function buildAddFlightSummary(params) {
            const parts = [];

            if (params.flightNumber) parts.push(`flight <strong>${params.flightNumber}</strong>`);
            if (params.airline) parts.push(`airline <strong>${params.airline}</strong>`);
            if (params.destination) parts.push(`destination <strong>${params.destination}</strong>`);
            if (params.departureTime) parts.push(`departure <strong>${params.departureTime}</strong>`);
            if (params.arrivalTime) {
                const label = params.arrivalEstimated ? 'estimated arrival' : 'arrival';
                parts.push(`${label} <strong>${params.arrivalTime}</strong>`);
            }
            if (params.gate) {
                const label = params.gateEstimated ? 'estimated gate' : 'gate';
                parts.push(`${label} <strong>${params.gate}</strong>`);
            }
            if (params.terminal) {
                const label = params.terminalEstimated ? 'estimated terminal' : 'terminal';
                parts.push(`${label} <strong>${params.terminal}</strong>`);
            }

            const mainSummary = parts.length
                ? `Updated form with ${parts.join(', ')}.`
                : 'No fields were extracted from that request.';

            const missing = Array.isArray(params.missingRequiredFields)
                ? params.missingRequiredFields.map(getMissingFieldLabel)
                : [];

            const details = [];
            if (params.message) details.push(`<small style="opacity:.85">${escapeHtml(params.message)}</small>`);
            if (missing.length) details.push(`<small style="opacity:.75">Still required: <strong>${missing.join(', ')}</strong>.</small>`);

            return `${mainSummary}${details.length ? `<br>${details.join('<br>')}` : ''}`;
        }

        function buildAddFlightConfirmationPrompt() {
            return 'Looks good. Should I save this flight now?<br><small style="opacity:.75">Reply <strong>yes</strong> to save automatically, or <strong>no</strong> to keep editing.</small>';
        }

        async function handleAdvancedSearchRequest(text) {
            const params = await callSearchAssistant(text);

            if (params.isSearchRequest === false || !hasSearchFilters(params)) {
                appendMessage('assistant', escapeHtml(params.message || 'I can only help with flight searches. Try asking by airline, destination, flight number, date, time, terminal or status.'), 'info');
                return;
            }

            window.scrollTo({ top: 0, behavior: 'smooth' });
            const clearFiltersButton = document.getElementById('clearFiltersBtn');
            if (clearFiltersButton) clearFiltersButton.click();

            fillFormFields(params);
            appendMessage('assistant', buildSummary(params), 'success');
            setTimeout(triggerSearch, 350);
        }

        async function handleAddFlightRequest(text) {
            const params = await callAddFlightAssistant(text);

            if (params.isAddFlightRequest === false && !hasAddFlightFields(params)) {
                appendMessage('assistant', escapeHtml(params.message || 'I can only help with adding flights. Include flight number, airline, destination, and departure time.'), 'info');
                return;
            }

            fillAddFlightFields(params);
            appendMessage('assistant', buildAddFlightSummary(params), 'success');

            const missingRequired = Array.isArray(params.missingRequiredFields)
                ? params.missingRequiredFields
                : [];

            if (missingRequired.length === 0 && hasRequiredAddFlightFormValues()) {
                awaitingAddFlightConfirmation = true;
                appendMessage('assistant', buildAddFlightConfirmationPrompt(), 'info');
            } else {
                clearAddFlightConfirmation();
            }
        }

        async function handleSend() {
            const text = inputEl.value.trim();
            if (!text || isBusy) return;

            inputEl.value = '';
            inputEl.style.height = 'auto';
            appendMessage('user', escapeHtml(text));

            if (isAddFlight && awaitingAddFlightConfirmation) {
                if (isAffirmativeResponse(text)) {
                    clearAddFlightConfirmation();
                    appendMessage('assistant', 'Saving this flight now.', 'success');
                    submitAddFlightForm();
                    return;
                }

                if (isNegativeResponse(text)) {
                    clearAddFlightConfirmation();
                    appendMessage('assistant', 'No problem, I will keep the form as-is. Share any changes you want.', 'info');
                    return;
                }

                clearAddFlightConfirmation();
            }

            setBusyState(true);
            appendThinking();

            try {
                if (isAdvancedSearch) {
                    await handleAdvancedSearchRequest(text);
                } else if (isAddFlight) {
                    await handleAddFlightRequest(text);
                }
            } catch (err) {
                appendMessage('assistant', escapeHtml(getFriendlyErrorMessage(err)), 'error');
                console.error('[AI Chat] DeepSeek error:', err);
            } finally {
                if (activeThinkingEl) activeThinkingEl.remove();
                activeThinkingEl = null;
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
