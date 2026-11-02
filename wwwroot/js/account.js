// Tài khoản & Xác thực - các form Đăng nhập, Đăng ký, Quên mật khẩu, Xác thực OTP, Đặt lại mật khẩu
// được gửi bằng AJAX (không tải lại trang khi có lỗi). Áp dụng cho mọi <form data-ajax-form>.
//
// Quy ước phản hồi của AccountController:
//   thành công           -> 200 { success: true, redirectUrl }
//   dữ liệu không hợp lệ -> 400 { success: false, errors: [{ field, message }] }   (field rỗng = lỗi chung của form)
(function ($) {
    'use strict';

    var GENERIC_ERROR = 'Có lỗi xảy ra, vui lòng thử lại.';

    function parseJson(xhr) {
        if (xhr.responseJSON) return xhr.responseJSON;
        try { return JSON.parse(xhr.responseText); } catch (e) { return null; }
    }

    // ---------- Tìm phần tử theo tên trường (không phân biệt hoa/thường) ----------

    function findSpan($form, field) {
        var key = String(field).toLowerCase();
        return $form.find('[data-valmsg-for]').filter(function () {
            return String($(this).attr('data-valmsg-for')).toLowerCase() === key;
        }).first();
    }

    function findInput($form, field) {
        var key = String(field).toLowerCase();
        return $form.find(':input[name]').filter(function () {
            return String(this.name).toLowerCase() === key;
        }).first();
    }

    // ---------- Hiển thị / xóa lỗi (dùng đúng các phần tử asp-validation-* đã có sẵn trong view) ----------

    function clearField($form, field) {
        findSpan($form, field).text('').removeClass('field-validation-error').addClass('field-validation-valid');
        findInput($form, field).removeClass('input-validation-error');
    }

    function clearErrors($form) {
        $form.find('[data-valmsg-for]').text('').removeClass('field-validation-error').addClass('field-validation-valid');
        $form.find('.input-validation-error').removeClass('input-validation-error');
        setSummary($form, []);
    }

    // Lỗi chung của form (vd: "Email hoặc mật khẩu không đúng") -> vùng validation-summary
    function setSummary($form, messages) {
        var $s = $form.find('[data-valmsg-summary="true"]').first();
        if (!$s.length) {
            if (!messages.length) return;
            $s = $('<div class="text-danger" data-valmsg-summary="true"><ul></ul></div>').prependTo($form);
        }
        var $ul = $s.children('ul').first();
        if (!$ul.length) $ul = $('<ul></ul>').appendTo($s);

        $ul.empty();
        $.each(messages, function (_, m) { $('<li></li>').text(m).appendTo($ul); });   // .text(): an toàn, không chèn HTML
        $s.toggleClass('validation-summary-errors', messages.length > 0)
          .toggleClass('validation-summary-valid', messages.length === 0);
    }

    function showErrors($form, errors) {
        clearErrors($form);

        var general = [];
        var $firstInput = null;

        $.each(errors, function (_, err) {
            var message = err.message || 'Dữ liệu không hợp lệ';
            var $span = err.field ? findSpan($form, err.field) : $();

            if (!$span.length) { general.push(message); return; }        // không có chỗ hiện riêng -> đưa vào lỗi chung
            if ($span.text()) return;                                    // mỗi trường chỉ hiện lỗi đầu tiên

            $span.text(message).removeClass('field-validation-valid').addClass('field-validation-error');
            var $input = findInput($form, err.field).addClass('input-validation-error');
            if (!$firstInput && $input.length) $firstInput = $input;
        });

        setSummary($form, general);
        if ($firstInput) $firstInput.trigger('focus');
    }

    // ---------- Trạng thái đang gửi ----------

    function setBusy($form, busy) {
        var $btn = $form.find('button[type="submit"]').first();
        $form.data('busy', busy);

        if (busy) {
            $btn.data('label', $btn.html()).prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Đang xử lý...');
        } else {
            $btn.prop('disabled', false);
            if ($btn.data('label') !== undefined) $btn.html($btn.data('label'));
        }
    }

    // ---------- Gửi form ----------

    $(document).on('submit', 'form[data-ajax-form]', function (e) {
        e.preventDefault();

        var $form = $(this);
        if ($form.data('busy')) return;                                   // chống bấm 2 lần
        if ($.fn.valid && !$form.valid()) return;                         // kiểm tra phía client (jquery-validation) trước

        clearErrors($form);
        setBusy($form, true);
        var redirecting = false;

        $.ajax({
            url: $form.attr('action') || window.location.href,
            type: 'POST',
            data: $form.serialize(),                                      // gồm cả token CSRF của form
            dataType: 'json'
        }).done(function (res) {
            if (res && res.redirectUrl) {
                redirecting = true;                                       // giữ nút ở trạng thái "đang xử lý" tới khi sang trang mới
                window.location.assign(res.redirectUrl);
            }
        }).fail(function (xhr) {
            var json = parseJson(xhr);
            if (json && $.isArray(json.errors) && json.errors.length) {
                showErrors($form, json.errors);
                return;
            }

            var msg = GENERIC_ERROR;
            if (xhr.status === 0) msg = 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra mạng và thử lại.';
            else if (xhr.status === 429) msg = 'Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.';
            else if (xhr.status === 400) msg = 'Phiên làm việc đã hết hạn. Vui lòng tải lại trang rồi thử lại.';
            setSummary($form, [msg]);
        }).always(function () {
            if (!redirecting) setBusy($form, false);
        });
    });

    // Người dùng sửa lại trường nào thì xóa lỗi của trường đó
    $(document).on('input change', 'form[data-ajax-form] :input[name]', function () {
        clearField($(this).closest('form'), this.name);
    });

    // Quay lại bằng nút Back (trang được khôi phục từ bộ nhớ đệm) -> không để nút bị kẹt ở trạng thái "đang xử lý"
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;
        $('form[data-ajax-form]').each(function () { setBusy($(this), false); });
    });
})(jQuery);
