/* ═══════════════════════════════════════════════════════════════
   LMS — Global JS  (runs after jQuery + Bootstrap are loaded)
   ═══════════════════════════════════════════════════════════════ */

// ── Toast notification ────────────────────────────────────────────
function showToast(message, type) {
    type = type || 'success';
    var icons = {
        success: 'bi-check-circle-fill',
        error:   'bi-x-circle-fill',
        info:    'bi-info-circle-fill'
    };
    var id   = 'toast_' + Date.now();
    var html = '<div id="' + id + '" class="lms-toast toast-' + type + ' mb-2" role="alert">' +
        '<i class="bi ' + (icons[type] || icons.info) + ' toast-icon"></i>' +
        '<span class="flex-grow-1">' + message + '</span>' +
        '<button onclick="this.closest(\'.lms-toast\').remove()" ' +
            'style="background:none;border:none;color:#94a3b8;cursor:pointer;padding:0 0 0 8px;font-size:1rem;">' +
        '<i class="bi bi-x"></i></button>' +
        '</div>';
    $('#toastContainer').append(html);
    setTimeout(function () {
        var el = document.getElementById(id);
        if (el) el.remove();
    }, 3500);
}

// ── Confirm helper ────────────────────────────────────────────────
function confirmDelete(msg) {
    return confirm(msg || 'Delete this record?');
}

// ── Book Search widget initializer ────────────────────────────────
// The _BookSearch.cshtml partial emits a <script type="text/x-booksearch-init" data-wid="...">
// tag. We find all such tags here (after jQuery has loaded) and wire them up.
$(function () {
    $('script[type="text/x-booksearch-init"]').each(function () {
        var wid = $(this).data('wid');
        if (!wid) return;

        (function (wid) {
            function doBookSearch() {
                var criteria = $('#bsCriteria_' + wid).val();
                var keyword  = $('#bsKeyword_' + wid).val().trim();
                if (!keyword) {
                    showToast('Please enter a keyword.', 'info');
                    return;
                }
                $('#bsMsg_' + wid).text('Searching...').show();
                $('#bsTable_' + wid).hide();

                $.get('/Books/Search', { criteria: criteria, keyword: keyword }, function (res) {
                    if (!res || !res.length) {
                        $('#bsMsg_' + wid).text('No books found.').show();
                        $('#bsTable_' + wid).hide();
                        return;
                    }
                    $('#bsMsg_' + wid).hide();
                    var $body = $('#bsBody_' + wid).empty();
                    res.forEach(function (b) {
                        $body.append(
                            '<tr>' +
                            '<td>' + (b.isbn || '') + '</td>' +
                            '<td>' + b.title + '</td>' +
                            '<td>' + (b.authors || '') + '</td>' +
                            '<td class="text-center">' + b.totalQuantity + '</td>' +
                            '<td class="text-center ' + (b.remainingQuantity === 0 ? 'text-danger fw-bold' : '') + '">' +
                            b.remainingQuantity + '</td>' +
                            '</tr>'
                        );
                    });
                    $('#bsTable_' + wid).show();
                });
            }

            $(document).on('click', '#bsBtn_' + wid, doBookSearch);
            $(document).on('keypress', '#bsKeyword_' + wid, function (e) {
                if (e.which === 13) doBookSearch();
            });
        })(wid);
    });
});
