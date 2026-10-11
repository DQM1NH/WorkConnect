// UC05 - Tìm kiếm và lọc việc làm bằng AJAX (không tải lại toàn bộ trang)
(function ($) {
    'use strict';

    var $form = $('#jobFilterForm');
    if (!$form.length) return;

    var $results = $('#jobResults');
    var $loading = $('#jobLoading');
    var searchUrl = $form.data('search-url');
    var debounceTimer = null;
    var requestId = 0;               // bỏ qua phản hồi cũ nếu người dùng đổi bộ lọc liên tục

    function runSearch() {
        var current = ++requestId;
        $loading.prop('hidden', false);

        $.ajax({
            url: searchUrl,
            type: 'GET',
            data: $form.serialize(),
            dataType: 'html'
        }).done(function (html) {
            if (current === requestId) $results.html(html);
        }).fail(function () {
            if (current === requestId) {
                $results.html('<div class="job-empty"><p>Có lỗi xảy ra, vui lòng thử lại.</p></div>');
            }
        }).always(function () {
            if (current === requestId) $loading.prop('hidden', true);
        });
    }

    // Ô chọn: tìm ngay khi thay đổi
    $form.on('change', 'select', runSearch);

    // Ô nhập (từ khóa, lương): chờ người dùng ngừng gõ 400ms
    $form.on('input', 'input[type="text"], input[type="number"]', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(runSearch, 400);
    });

    // Nhấn Enter hoặc nút Tìm kiếm
    $form.on('submit', function (e) {
        e.preventDefault();
        clearTimeout(debounceTimer);
        runSearch();
    });

    // Xóa bộ lọc
    $('#btnResetFilter').on('click', function (e) {
        e.preventDefault();
        $form.find('input[type="text"], input[type="number"]').val('');
        $form.find('select').not('[name="Sort"]').val('');
        $form.find('select[name="Sort"]').val('moi-nhat');
        runSearch();
    });
})(jQuery);