// Trang quản trị - toàn bộ tương tác chạy bằng AJAX (không tải lại cả trang):
//   - đổi tab, tìm kiếm khi gõ, phân trang  -> GET  /Admin?tab=...  (server trả partial _AdminTabBody)
//   - duyệt / từ chối / ẩn / hiện / khóa / xóa -> POST (server trả JSON { success, message })
//   - sau mỗi thao tác: tải lại đúng tab đang xem (giữ nguyên từ khóa + trang) và cập nhật 4 ô thống kê
(function ($) {
    'use strict';

    var $root = $('.admin-dashboard-container');
    if (!$root.length) return;

    var indexUrl = $root.attr('data-index-url');
    var loginUrl = $root.attr('data-login-url');

    // Tên tham số query của từng tab (phải khớp với AdminController.Index)
    var TABS = {
        approval:     { q: 'pq', p: 'pp', panel: 'tab-content-approval' },
        users:        { q: 'uq', p: 'up', panel: 'tab-user-management' },
        content:      { q: 'cq', p: 'cp', panel: 'tab-content-management' },
        applications: { q: 'aq', p: 'ap', panel: 'tab-applications' }
    };

    var activeTab = $root.attr('data-active-tab') || 'approval';
    var state = {};        // tab -> { q, page }  (từ khóa + trang đang xem của từng tab)
    var dirty = {};        // tab -> true nếu dữ liệu đã cũ (có thao tác thay đổi dữ liệu ở tab khác)
    var requestIds = {};   // tab -> id request mới nhất (bỏ qua phản hồi cũ khi gõ / bấm liên tục)
    var searchTimers = {};

    // ===================== Thông báo (toast) =====================

    function toast(type, message) {
        var isError = type === 'error';
        var $t = $('<div class="admin-toast"></div>')
            .addClass(isError ? 'admin-toast-error' : 'admin-toast-success')
            .attr('role', isError ? 'alert' : 'status');
        $('<i class="fa-solid" aria-hidden="true"></i>')
            .addClass(isError ? 'fa-circle-exclamation' : 'fa-circle-check').appendTo($t);
        $('<span class="admin-toast-text"></span>').text(message).appendTo($t);   // .text(): an toàn, không chèn HTML
        $('<button type="button" class="admin-toast-close" aria-label="Đóng">&times;</button>').appendTo($t);

        var timer;
        function remove() {
            clearTimeout(timer);
            $t.removeClass('show');
            setTimeout(function () { $t.remove(); }, 200);
        }
        $t.on('click', '.admin-toast-close', remove);

        $('#adminToasts').append($t);
        requestAnimationFrame(function () { $t.addClass('show'); });
        timer = setTimeout(remove, isError ? 6000 : 4000);
    }

    function parseJson(xhr) {
        if (xhr.responseJSON) return xhr.responseJSON;
        try { return JSON.parse(xhr.responseText); } catch (e) { return null; }
    }

    // Lỗi từ server / mạng. Trả về true nếu là lỗi nặng đã được xử lý (hết phiên đăng nhập)
    function handleError(xhr, fallback) {
        if (xhr.status === 401) {
            // Phiên đăng nhập hết hạn -> về trang đăng nhập rồi quay lại đúng trang này
            window.location.href = loginUrl + '?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
            return true;
        }
        var json = parseJson(xhr);
        var msg = json && json.message;
        if (!msg) {
            if (xhr.status === 0) msg = 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra mạng và thử lại.';
            else if (xhr.status === 403) msg = 'Bạn không có quyền thực hiện thao tác này.';
            else if (xhr.status === 400) msg = 'Phiên làm việc đã hết hạn. Vui lòng tải lại trang rồi thử lại.';
            else msg = fallback;
        }
        toast('error', msg);
        return false;
    }

    // ===================== Trạng thái từng tab =====================

    // Đọc .admin-payload do server nhúng trong mỗi lần render tab:
    // nhớ từ khóa / trang hiện tại (server có thể đã chỉnh lại số trang) và cập nhật ô thống kê
    function readPayload($body) {
        var $p = $body.find('.admin-payload').first();
        if (!$p.length) return;

        var tab = $p.attr('data-tab');
        if (!TABS[tab]) return;
        state[tab] = {
            q: $p.attr('data-q') || '',
            page: parseInt($p.attr('data-page'), 10) || 1
        };

        $('#stat-total-users').text($p.attr('data-total-users'));
        $('#stat-total-jobs').text($p.attr('data-total-jobs'));
        $('#stat-pending').text($p.attr('data-pending'));
        $('#stat-total-applications').text($p.attr('data-total-applications'));
        $('#badge-pending').text($p.attr('data-pending-raw'));
    }

    // Giữ địa chỉ trên thanh trình duyệt luôn khớp với trạng thái đang xem (F5 / copy link vẫn đúng)
    function syncUrl(tab) {
        var cfg = TABS[tab], st = state[tab] || { q: '', page: 1 };
        var url = new URL(window.location.href);
        url.searchParams.set('tab', tab);
        if (st.q) url.searchParams.set(cfg.q, st.q); else url.searchParams.delete(cfg.q);
        if (st.page > 1) url.searchParams.set(cfg.p, st.page); else url.searchParams.delete(cfg.p);
        history.replaceState(null, '', url);
    }

    // ===================== Tải nội dung tab bằng AJAX =====================

    function loadTab(tab, opts) {
        opts = opts || {};
        var cfg = TABS[tab];
        var st = state[tab] || (state[tab] = { q: '', page: 1 });
        if (opts.q !== undefined) st.q = opts.q;
        if (opts.page !== undefined) st.page = opts.page;

        var data = { tab: tab };
        if (st.q) data[cfg.q] = st.q;
        if (st.page > 1) data[cfg.p] = st.page;

        var current = requestIds[tab] = (requestIds[tab] || 0) + 1;
        var $body = $('#body-' + tab);
        $body.addClass('is-loading').attr('aria-busy', 'true');

        return $.ajax({ url: indexUrl, type: 'GET', data: data, dataType: 'html' })
            .done(function (html) {
                if (current !== requestIds[tab]) return;
                $body.html(html);
                readPayload($body);
                dirty[tab] = false;
                syncUrl(tab);
            })
            .fail(function (xhr) {
                if (current === requestIds[tab]) handleError(xhr, 'Không tải được dữ liệu, vui lòng thử lại.');
            })
            .always(function () {
                if (current === requestIds[tab]) $body.removeClass('is-loading').removeAttr('aria-busy');
            });
    }

    // Sau khi dữ liệu thay đổi: tải lại tab đang xem, đánh dấu các tab còn lại là "cũ" (tải khi chuyển sang)
    function refreshAfterChange() {
        $.each(TABS, function (tab) { if (tab !== activeTab) dirty[tab] = true; });
        return loadTab(activeTab);
    }

    // ===================== Đổi tab =====================

    function switchTab(tab) {
        if (!TABS[tab]) return;
        activeTab = tab;
        $('.tab-content').removeClass('active');
        $('.tab-btn').removeClass('active');
        $('#' + TABS[tab].panel).addClass('active');
        $('.tab-btn[data-tab="' + tab + '"]').addClass('active');

        if (dirty[tab]) loadTab(tab); else syncUrl(tab);
    }

    $(document).on('click', '.tab-btn[data-tab]', function () {
        switchTab($(this).attr('data-tab'));
    });

    // ===================== Phân trang =====================

    $root.on('click', '.tab-body a.pager-btn[data-page]', function (e) {
        e.preventDefault();
        var $body = $(this).closest('.tab-body');
        var page = parseInt($(this).attr('data-page'), 10);
        if (!(page > 0)) return;

        loadTab($body.attr('data-tab'), { page: page });

        // Đang ở cuối danh sách dài -> đưa người dùng về đầu bảng mới
        if ($body[0].getBoundingClientRect().top < 0) {
            $body[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });

    // ===================== Tìm kiếm khi gõ =====================

    function scheduleSearch($form, delay) {
        var tab = $form.attr('data-tab');
        if (!TABS[tab]) return;

        clearTimeout(searchTimers[tab]);
        searchTimers[tab] = setTimeout(function () {
            var q = $.trim($form.find('.admin-search-input').val());
            if (q === (state[tab] ? state[tab].q : '')) return;   // từ khóa không đổi -> khỏi gọi lại
            loadTab(tab, { q: q, page: 1 });
        }, delay);
    }

    $root.on('input', '.admin-search-input', function (e) {
        // Đang gõ dở tiếng Việt (IME / Telex): chờ gõ xong
        if (e.originalEvent && e.originalEvent.isComposing) return;
        scheduleSearch($(this).closest('form'), 350);
    });
    $root.on('compositionend', '.admin-search-input', function () {
        scheduleSearch($(this).closest('form'), 350);
    });
    $root.on('submit', 'form[data-admin-search]', function (e) {
        e.preventDefault();
        scheduleSearch($(this), 0);
    });

    // ===================== Modal xem chi tiết tin =====================

    var $modal = $('#jobDetailModal');

    function openJobDetail(jobId) {
        var tpl = document.getElementById('job-detail-' + jobId);
        if (!tpl) return;
        document.getElementById('jobDetailModalBody').replaceChildren(tpl.content.cloneNode(true));
        $modal.find('.job-modal-id').val(jobId);
        $modal.addClass('open').attr('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
    }

    function closeJobDetail() {
        $modal.removeClass('open').attr('aria-hidden', 'true');
        document.body.style.overflow = '';
    }

    $(document).on('click', '.btn-view[data-job-id]', function () {
        openJobDetail($(this).attr('data-job-id'));
    });
    $(document).on('click', '[data-modal-close]', closeJobDetail);
    $modal.on('click', function (e) { if (e.target === this) closeJobDetail(); });
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $modal.hasClass('open')) closeJobDetail();
    });

    // ===================== Thao tác: duyệt / từ chối / ẩn-hiện / khóa / xóa =====================
    // Mọi form có thuộc tính data-ajax-action đều được gửi bằng AJAX (token CSRF nằm sẵn trong form)

    $(document).on('submit', 'form[data-ajax-action]', function (e) {
        e.preventDefault();

        var $form = $(this);
        var message = $form.attr('data-confirm');
        if (message && !window.confirm(message)) return;

        if ($form.data('busy')) return;                       // chống bấm 2 lần
        var $buttons = $form.find('button[type="submit"]');
        $form.data('busy', true);
        $buttons.prop('disabled', true);

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(),
            dataType: 'json'
        }).done(function (res) {
            toast('success', (res && res.message) || 'Thao tác thành công.');
            if ($form.closest('#jobDetailModal').length) closeJobDetail();
            refreshAfterChange();
        }).fail(function (xhr) {
            if (handleError(xhr, 'Không thực hiện được thao tác, vui lòng thử lại.')) return;
            // 404 / 409: dữ liệu trên màn hình đã cũ (người khác vừa xử lý / xóa) -> làm mới cho khớp
            if (xhr.status === 404 || xhr.status === 409) {
                if ($form.closest('#jobDetailModal').length) closeJobDetail();
                refreshAfterChange();
            }
        }).always(function () {
            $form.data('busy', false);
            $buttons.prop('disabled', false);
        });
    });

    // ===================== Khởi tạo =====================

    $('.tab-body').each(function () { readPayload($(this)); });
})(jQuery);
