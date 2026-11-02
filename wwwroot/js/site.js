// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ===== Menu điều hướng trên màn hình nhỏ =====
(function () {
    'use strict';

    var toggle = document.getElementById('navToggle');
    var nav = document.getElementById('mainNav');
    if (!toggle || !nav) return;

    var icon = toggle.querySelector('i');

    function setOpen(open) {
        nav.classList.toggle('is-open', open);
        toggle.setAttribute('aria-expanded', String(open));
        toggle.setAttribute('aria-label', open ? 'Đóng menu' : 'Mở menu');
        if (icon) {
            icon.classList.toggle('fa-bars', !open);
            icon.classList.toggle('fa-xmark', open);
        }
    }

    toggle.addEventListener('click', function () {
        setOpen(!nav.classList.contains('is-open'));
    });

    // Bấm vào một mục menu -> đóng lại
    nav.addEventListener('click', function (e) {
        if (e.target.closest('a')) setOpen(false);
    });

    // Bấm ra ngoài hoặc nhấn Esc -> đóng
    document.addEventListener('click', function (e) {
        if (nav.classList.contains('is-open') && !e.target.closest('.header-container')) setOpen(false);
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && nav.classList.contains('is-open')) { setOpen(false); toggle.focus(); }
    });

    // Quay lại màn hình rộng -> đặt lại trạng thái
    window.matchMedia('(min-width: 992px)').addEventListener('change', function (e) {
        if (e.matches) setOpen(false);
    });
})();
