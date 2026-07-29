// Web Push opt-in flow: shows a small dismissible banner asking permission, registers the
// service worker, subscribes via the browser's Push API, and saves the subscription on the
// server. Works for both the storefront (customer) and the admin panel - the difference is
// just which endpoint #push-notifications-root points to (see _Layout.cshtml / _AdminLayout.cshtml).
(function () {
    var root = document.getElementById('push-notifications-root');
    if (!root) return; // person isn't logged in / this page doesn't support push

    var subscribeEndpoint = root.getAttribute('data-subscribe-endpoint');
    if (!subscribeEndpoint) return;

    if (!('serviceWorker' in navigator) || !('PushManager' in window) || !('Notification' in window)) {
        return; // browser doesn't support Web Push at all (e.g. some older mobile browsers)
    }

    var DISMISSED_KEY = 'push-notifications-dismissed';

    function getCsrfToken() {
        var meta = document.querySelector('meta[name="csrf-token"]');
        return meta ? meta.getAttribute('content') : '';
    }

    function urlBase64ToUint8Array(base64String) {
        var padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        var base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        var rawData = window.atob(base64);
        var outputArray = new Uint8Array(rawData.length);
        for (var i = 0; i < rawData.length; i++) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }

    function showBanner() {
        if (localStorage.getItem(DISMISSED_KEY)) return;
        if (Notification.permission !== 'default') return; // already allowed or blocked, nothing to ask

        var banner = document.createElement('div');
        banner.setAttribute('dir', 'rtl');
        banner.style.cssText =
            'position:fixed;bottom:16px;left:16px;right:16px;max-width:420px;margin:0 auto;' +
            'background:#212529;color:#fff;border-radius:14px;padding:14px 16px;z-index:1080;' +
            'box-shadow:0 8px 24px rgba(0,0,0,.25);display:flex;align-items:center;gap:12px;' +
            'font-family:inherit;';

        banner.innerHTML =
            '<i class="fa-solid fa-bell" style="font-size:20px;color:#ffc107;flex-shrink:0;"></i>' +
            '<span style="flex:1;font-size:.9rem;">هل تحب تفعيل الإشعارات عشان توصلك آخر التحديثات فورًا؟</span>' +
            '<button type="button" data-action="yes" style="background:#ffc107;color:#000;border:0;border-radius:8px;padding:6px 14px;font-size:.85rem;font-weight:600;white-space:nowrap;">تفعيل</button>' +
            '<button type="button" data-action="no" style="background:transparent;color:#adb5bd;border:0;font-size:.85rem;white-space:nowrap;">لأ شكرًا</button>';

        document.body.appendChild(banner);

        banner.querySelector('[data-action="yes"]').addEventListener('click', function () {
            banner.remove();
            subscribe();
        });
        banner.querySelector('[data-action="no"]').addEventListener('click', function () {
            banner.remove();
            localStorage.setItem(DISMISSED_KEY, '1');
        });
    }

    function subscribe() {
        navigator.serviceWorker.register('/sw.js')
            .then(function (registration) {
                return fetch('/push/vapid-public-key')
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        return registration.pushManager.subscribe({
                            userVisibleOnly: true,
                            applicationServerKey: urlBase64ToUint8Array(data.publicKey)
                        });
                    });
            })
            .then(function (subscription) {
                var json = subscription.toJSON();
                return fetch(subscribeEndpoint, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-TOKEN': getCsrfToken()
                    },
                    body: JSON.stringify({ endpoint: json.endpoint, keys: json.keys })
                });
            })
            .then(function () {
                localStorage.setItem(DISMISSED_KEY, '1');
            })
            .catch(function (err) {
                // Permission denied, or something failed - don't keep nagging either way
                localStorage.setItem(DISMISSED_KEY, '1');
                console.warn('Push subscription failed:', err);
            });
    }

    // Register the service worker up front (cheap, idempotent) so it's ready once permission is granted
    navigator.serviceWorker.register('/sw.js').catch(function () { });

    if (Notification.permission === 'default') {
        // Small delay so the banner doesn't feel like a jarring pop-up the instant the page loads
        setTimeout(showBanner, 2500);
    }
})();
