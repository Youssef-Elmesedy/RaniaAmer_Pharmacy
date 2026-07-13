// Lets customers see admin changes (new/updated products, categories, offers, discounts)
// on public pages without needing to manually refresh.
(function () {
    const POLL_INTERVAL_MS = 15000;

    function isExcludedPage() {
        // Don't auto-reload pages where the customer might be filling out a form
        // (would lose their cart edits / typed info).
        const path = window.location.pathname.toLowerCase();
        return path.includes('/cart') || path.includes('/customeraccount');
    }

    if (isExcludedPage()) return;

    let lastKnownVersion = null;

    function poll() {
        // Never interrupt someone actively typing somewhere on the page
        var active = document.activeElement;
        if (active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.tagName === 'SELECT')) {
            return;
        }

        fetch('/Home/CatalogVersion', { headers: { 'X-Requested-With': 'XMLHttpRequest' }, cache: 'no-store' })
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (data) {
                if (!data) return;

                if (lastKnownVersion !== null && data.version !== lastKnownVersion) {
                    window.location.reload();
                    return;
                }

                lastKnownVersion = data.version;
            })
            .catch(function () { /* ignore transient network errors */ });
    }

    setInterval(poll, POLL_INTERVAL_MS);
})();
