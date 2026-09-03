/* ═══════════════════════════════════════════════════════════════
   LMS — Global JS
   ═══════════════════════════════════════════════════════════════ */

// Professional toast notification
function showToast(message, type) {
    type = type || 'success';
    var icons = { success: 'bi-check-circle-fill', error: 'bi-x-circle-fill', info: 'bi-info-circle-fill' };
    var id = 'toast_' + Date.now();
    var html = '<div id="' + id + '" class="lms-toast toast-' + type + ' mb-2" role="alert">' +
        '<i class="bi ' + (icons[type] || icons.info) + ' toast-icon"></i>' +
        '<span class="flex-grow-1">' + message + '</span>' +
        '<button onclick="this.closest(\'.lms-toast\').remove()" style="background:none;border:none;color:#94a3b8;cursor:pointer;padding:0 0 0 8px;font-size:1rem;">' +
        '<i class="bi bi-x"></i></button>' +
        '</div>';
    $('#toastContainer').append(html);
    setTimeout(function() { var el = document.getElementById(id); if(el) el.remove(); }, 3500);
}

// Confirm dialog
function confirmDelete(msg) {
    return confirm(msg || 'Delete this record?');
}
