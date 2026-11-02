// UC05 - Tìm kiếm và lọc việc làm bằng AJAX (không tải lại toàn bộ trang)
(function ($) {
    'use strict';

    var $form = $('#jobFilterForm');
    if (!$form.length) return;

    var $results = $('#jobResults');
    var $loading = $('#jobLoading');
    var $salaryError = $('#salaryError');
    var $from = $form.find('[name="LuongTuTrieu"]');
    var $to = $form.find('[name="LuongDenTrieu"]');
    var searchUrl = $form.data('search-url');
    var requestId = 0;               // bỏ qua phản hồi cũ nếu người dùng bấm tìm liên tục

    var SALARY_MSG = 'Mức lương "Từ" phải nhỏ hơn mức lương "Đến".';

    function showSalaryError(msg) {
        $salaryError.text(msg).prop('hidden', false);
        $from.add($to).addClass('is-invalid');
    }

    function clearSalaryError() {
        $salaryError.text('').prop('hidden', true);
        $from.add($to).removeClass('is-invalid');
    }

    // Nhập cả 2 ô thì Từ phải < Đến
    function isSalaryRangeValid() {
        var a = $.trim($from.val());
        var b = $.trim($to.val());
        if (a === '' || b === '') return true;     
        var from = parseFloat(a), to = parseFloat(b);
        if (isNaN(from) || isNaN(to)) return true;
        return from < to;
    }

    function runSearch() {
        if (!isSalaryRangeValid()) {
            requestId++;                             
            $loading.prop('hidden', true);
            showSalaryError(SALARY_MSG);
            $results.empty();
            $from.trigger('focus');
            return;
        }
        clearSalaryError();

        var current = ++requestId;
        $loading.prop('hidden', false);

        $.ajax({
            url: searchUrl,
            type: 'GET',
            data: $form.serialize(),
            dataType: 'html'
        }).done(function (html) {
            if (current === requestId) $results.html(html);
        }).fail(function (xhr) {
            if (current !== requestId) return;
            if (xhr.status === 400 && xhr.responseText) {
                // lỗi kiểm tra từ server (phòng khi bỏ qua kiểm tra phía client)
                showSalaryError(xhr.responseText);
                $results.empty();
            } else {
                $results.html('<div class="job-empty"><p>Có lỗi xảy ra, vui lòng thử lại.</p></div>');
            }
        }).always(function () {
            if (current === requestId) $loading.prop('hidden', true);
        });
    }

    // Chỉ lọc khi nhấn nút Tìm kiếm (hoặc Enter trong ô từ khóa)
    $form.on('submit', function (e) {
        e.preventDefault();
        runSearch();
    });

    // Gõ lại thì xóa thông báo lỗi
    $from.add($to).on('input', clearSalaryError);

    // Xóa bộ lọc
    $('#btnResetFilter').on('click', function (e) {
        e.preventDefault();
        clearSalaryError();
        $form.find('input[type="text"], input[type="number"]').val('');
        $form.find('select').not('[name="Sort"]').val('');
        $form.find('select[name="Sort"]').val('moi-nhat');
        runSearch();
    });
})(jQuery);