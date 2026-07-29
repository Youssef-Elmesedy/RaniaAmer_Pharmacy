// Service worker for Web Push notifications - أولاد زمزم
// Registered at the site root (see push-notifications.js) so it can handle push events for
// every page on the site, for both the storefront and the admin panel.

const NOTIFICATION_ICON = '/Uplodes/logo/7154d8e0-7ee2-4cfe-a7e8-f6827c1aa810.png';

self.addEventListener('install', function () {
    self.skipWaiting();
});

self.addEventListener('activate', function (event) {
    event.waitUntil(self.clients.claim());
});

self.addEventListener('push', function (event) {
    var data = {};

    try {
        data = event.data ? event.data.json() : {};
    } catch (e) {
        data = { title: 'أولاد زمزم', body: event.data ? event.data.text() : 'لديك إشعار جديد' };
    }

    var title = data.title || 'أولاد زمزم';
    var options = {
        body: data.body || '',
        icon: NOTIFICATION_ICON,
        badge: NOTIFICATION_ICON,
        dir: 'rtl',
        lang: 'ar',
        data: { url: data.url || '/' }
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();

    var targetUrl = (event.notification.data && event.notification.data.url) || '/';

    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then(function (windows) {
            for (var i = 0; i < windows.length; i++) {
                var win = windows[i];
                if (win.url.indexOf(targetUrl) !== -1 && 'focus' in win) {
                    return win.focus();
                }
            }
            if (self.clients.openWindow) {
                return self.clients.openWindow(targetUrl);
            }
        })
    );
});
