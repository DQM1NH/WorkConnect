
(function ($) {
    'use strict';

    function errorText(xhr) {
        try { var j = JSON.parse(xhr.responseText); if (j && j.error) return j.error; } catch (e) { }
        return 'Có lỗi xảy ra, vui lòng thử lại.';
    }

    // ---------- UC04: CV ----------
    var $cvForm = $('#cvUploadForm');
    var $cvList = $('#cvList');
    var $msg = $('#cvMessage');

    function show(type, text) {
        $msg.removeClass('d-none alert-success alert-danger').addClass('alert-' + type).text(text);
    }
    function token() { return $cvForm.find('input[name="__RequestVerificationToken"]').val(); }

    $cvForm.on('submit', function (e) {
        e.preventDefault();
        var file = $cvForm.find('input[type="file"]')[0].files[0];
        if (!file) { show('danger', 'Vui lòng chọn tệp CV.'); return; }
        var ext = file.name.split('.').pop().toLowerCase();
        if (ext !== 'pdf' && ext !== 'docx') { show('danger', 'Chỉ chấp nhận CV định dạng PDF hoặc DOCX.'); return; }
        if (file.size > 5 * 1024 * 1024) { show('danger', 'CV vượt quá dung lượng tối đa 5 MB.'); return; }

        $.ajax({
            url: $cvForm.attr('action'), type: 'POST', dataType: 'html',
            data: new FormData(this), processData: false, contentType: false   // FormData đã có sẵn token CSRF
        }).done(function (html) {
            $cvList.html(html);
            $cvForm[0].reset();
            show('success', 'Đã tải CV lên.');
        }).fail(function (xhr) { show('danger', errorText(xhr)); });
    });

    $cvList.on('click', '.btn-cv-delete', function () {
        if (!window.confirm('Bạn có chắc muốn xóa CV này?')) return;
        $.ajax({
            url: $cvList.data('delete-url'), type: 'POST', dataType: 'html',
            data: { maCv: $(this).data('id'), __RequestVerificationToken: token() }
        }).done(function (html) {
            $cvList.html(html);
            show('success', 'Đã xóa CV.');
        }).fail(function (xhr) { show('danger', errorText(xhr)); });
    });

    // ---------- UC08: lọc đơn theo trạng thái ----------
    var $status = $('#statusFilter');
    var requestId = 0;
    $status.on('change', function () {
        var current = ++requestId;
        $('#appLoading').prop('hidden', false);
        $.ajax({ url: $status.data('url'), type: 'GET', data: { status: $status.val() }, dataType: 'html' })
            .done(function (html) { if (current === requestId) $('#appList').html(html); })
            .fail(function () {
                if (current === requestId)
                    $('#appList').html('<div class="job-empty"><p>Có lỗi xảy ra, vui lòng thử lại.</p></div>');
            })
            .always(function () { if (current === requestId) $('#appLoading').prop('hidden', true); });
    });
})(jQuery);