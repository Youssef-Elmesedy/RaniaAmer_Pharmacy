// Safety net: if the loader is ever shown (e.g. before a guaranteed delete-confirm submit),
// make sure it can never get stuck - hide it on load and on bfcache restores.
function hidePageLoader() {
    var loader = document.getElementById('page-loader');
    if (loader) {
        loader.style.opacity = '0';
        setTimeout(function () { loader.style.display = 'none'; }, 200);
    }
}

window.addEventListener('pageshow', hidePageLoader);

// NOTE: We intentionally do NOT show the loader on every link click / form submit / beforeunload
// anymore. That approach kept getting stuck permanently in real-world use:
//   - a form failing client-side validation cancels the submit, but the loader was already shown
//   - the browser's back/forward cache restores a frozen DOM snapshot where the loader was left
//     visible, and no further load event fires to hide it again
// The browser's own native loading indicator (tab spinner / progress bar) is sufficient feedback
// for regular navigation, so we rely on that instead.

document.addEventListener('DOMContentLoaded', function () {
    var loader = document.getElementById('page-loader');

    function showLoader() {
        if (loader) {
            loader.style.display = 'flex';
            loader.style.opacity = '1';
            // Hard safety net: never let it stay stuck longer than a few seconds no matter what
            setTimeout(hidePageLoader, 5000);
        }
    }

    // SweetAlert2 toast for success / error TempData messages
    var toastData = document.getElementById('toast-data');
    if (toastData) {
        var success = toastData.getAttribute('data-success');
        var error = toastData.getAttribute('data-error');

        var Toast = Swal.mixin({
            toast: true,
            position: 'top-start',
            showConfirmButton: false,
            timer: 3500,
            timerProgressBar: true
        });

        if (success) {
            Toast.fire({ icon: 'success', title: success });
        }
        if (error) {
            Toast.fire({ icon: 'error', title: error });
        }
    }

    // SweetAlert2 delete confirmation for any form with the "js-delete-form" class
    document.querySelectorAll('.js-delete-form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            Swal.fire({
                title: 'تأكيد الحذف',
                text: 'هل أنت متأكد من الحذف؟ لا يمكن التراجع عن هذا الإجراء',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'نعم، حذف',
                cancelButtonText: 'إلغاء',
                confirmButtonColor: '#8b1e1e'
            }).then(function (result) {
                if (result.isConfirmed) {
                    showLoader();
                    form.submit();
                }
            });
        });
    });

    // Live search: auto-submits the parent form shortly after typing stops (no need to press "بحث")
    document.querySelectorAll('.js-live-search').forEach(function (input) {
        var timer = null;
        input.addEventListener('input', function () {
            clearTimeout(timer);
            timer = setTimeout(function () {
                var form = input.closest('form');
                if (form) form.requestSubmit ? form.requestSubmit() : form.submit();
            }, 450);
        });
    });
});
