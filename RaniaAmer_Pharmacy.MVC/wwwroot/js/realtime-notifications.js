// Live, in-tab companion to the Web Push notifications (push-notifications.js): this connects
// to the SignalR hub and shows an instant toast the moment something happens WHILE the tab is
// already open — no page reload, no waiting for a push. Push still covers the case where the
// tab/browser is closed; this just makes the open-tab experience feel instant.
(function () {
    var root = document.getElementById('push-notifications-root');
    if (!root) return; // person isn't logged in / this page doesn't support notifications

    if (typeof signalR === 'undefined') return; // SignalR client script didn't load

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notifications')
        .withAutomaticReconnect()
        .build();

    connection.on('ReceiveNotification', function (data) {
        if (typeof Swal === 'undefined') return;

        var Toast = Swal.mixin({
            toast: true,
            position: 'top-start',
            showConfirmButton: false,
            timer: 6000,
            timerProgressBar: true
        });

        Toast.fire({
            icon: 'info',
            title: data.title || '',
            text: data.body || ''
        });
    });

    connection.start().catch(function (err) {
        // Not fatal - push notifications still work regardless of this live connection
        console.warn('SignalR connection failed:', err);
    });
})();
