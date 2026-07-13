// Polls for new pending orders so the admin doesn't have to manually refresh the page
(function () {
    const POLL_INTERVAL_MS = 10000;
    const badges = () => document.querySelectorAll('.js-pending-badge');

    let lastKnownCount = null;
    // Seed lastKnownCount from whatever is already rendered server-side
    const firstBadge = badges()[0];
    if (firstBadge) {
        lastKnownCount = parseInt(firstBadge.textContent, 10) || 0;
    }

    function updateBadges(count) {
        badges().forEach(function (badge) {
            badge.textContent = count;
            badge.style.display = count > 0 ? '' : 'none';
        });
    }

    function isListPage() {
        const path = window.location.pathname.toLowerCase();
        return path.includes('/admin/orders') ||
               path.includes('/admin/notifications') ||
               path.includes('/admin/dashboard') ||
               path === '/admin';
    }

    function poll() {
        fetch('/Admin/Notifications/PendingCount', { headers: { 'X-Requested-With': 'XMLHttpRequest' }, cache: 'no-store' })
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (data) {
                if (!data) return;

                updateBadges(data.count);

                if (lastKnownCount !== null && data.count !== lastKnownCount) {
                    if (data.count > lastKnownCount && window.Swal) {
                        Swal.fire({
                            toast: true,
                            position: 'top-start',
                            icon: 'info',
                            title: 'وصل طلب جديد!',
                            showConfirmButton: false,
                            timer: 4000,
                            timerProgressBar: true
                        });
                    }

                    // Refresh the list/summary pages automatically so the admin never has to hit F5
                    if (isListPage()) {
                        setTimeout(function () { window.location.reload(); }, 1200);
                    }
                }

                lastKnownCount = data.count;
            })
            .catch(function () { /* ignore transient network errors */ });
    }

    setInterval(poll, POLL_INTERVAL_MS);
})();
