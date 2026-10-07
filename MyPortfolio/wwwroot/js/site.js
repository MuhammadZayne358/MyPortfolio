(function () {
    'use strict';

    // ── Show / hide password ────────────────────────────────────────────
    document.querySelectorAll('[data-toggle-password]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var wrap = btn.closest('.input-icon');
            var input = wrap && wrap.querySelector('input');
            if (!input) { return; }
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            var icon = btn.querySelector('i');
            if (icon) { icon.className = show ? 'fas fa-eye-slash' : 'fas fa-eye'; }
            btn.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        });
    });

    // ── Confirm before destructive form submits ─────────────────────────
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!window.confirm(form.getAttribute('data-confirm'))) { e.preventDefault(); }
        });
    });

    // ── Hero watch dial shows the visitor's local time ──────────────────
    var dial = document.querySelector('.dial');
    if (dial) {
        var hourHand = dial.querySelector('.hand-hour');
        var minuteHand = dial.querySelector('.hand-min');
        var secondHand = dial.querySelector('.hand-sec');

        var setTime = function () {
            var now = new Date();
            var s = now.getSeconds() + now.getMilliseconds() / 1000;
            var m = now.getMinutes() + s / 60;
            var h = (now.getHours() % 12) + m / 60;
            hourHand.setAttribute('transform', 'rotate(' + (h * 30) + ' 200 200)');
            minuteHand.setAttribute('transform', 'rotate(' + (m * 6) + ' 200 200)');
            return s;
        };

        var seconds = setTime();
        if (secondHand) { secondHand.style.animationDelay = (-seconds) + 's'; }
        window.setInterval(setTime, 15000);
    }

    // ── Image preview (add / edit product) ──────────────────────────────
    var upload = document.getElementById('imgUpload');
    if (upload) {
        upload.addEventListener('change', function (e) {
            var file = e.target.files && e.target.files[0];
            if (!file) { return; }
            var reader = new FileReader();
            reader.onload = function (ev) {
                var img = document.getElementById('imgPreview');
                var text = document.getElementById('uploadText');
                if (img) { img.src = ev.target.result; img.style.display = 'block'; }
                if (text) { text.textContent = file.name; }
            };
            reader.readAsDataURL(file);
        });
    }

    // ── Collection page: search, filters and sorting ────────────────────
    var grid = document.getElementById('productGrid');
    if (grid) {
        var items = Array.prototype.slice.call(grid.querySelectorAll('.product-col'));
        var search = document.getElementById('productSearch');
        var price = document.getElementById('priceRange');
        var priceLabel = document.getElementById('priceLabel');
        var sort = document.getElementById('sortSelect');
        var clearBtn = document.getElementById('clearFilters');
        var count = document.getElementById('resultsCount');
        var empty = document.getElementById('noResults');

        var checked = function (name) {
            return Array.prototype.map.call(document.querySelectorAll('input[name="' + name + '"]:checked'), function (i) { return i.value; });
        };
        var money = function (n) { return '$' + Number(n).toLocaleString('en-US'); };

        var apply = function () {
            var q = search ? search.value.trim().toLowerCase() : '';
            var cats = checked('categoryIds');
            var brands = checked('brandIds');
            var max = price ? Number(price.value) : Infinity;
            if (priceLabel && price) { priceLabel.textContent = 'Up to ' + money(max); }

            var visible = 0;
            items.forEach(function (el) {
                var ok = (!q || el.dataset.search.indexOf(q) !== -1) &&
                    (!cats.length || cats.indexOf(el.dataset.cat) !== -1) &&
                    (!brands.length || brands.indexOf(el.dataset.brand) !== -1) &&
                    Number(el.dataset.price) <= max;
                el.hidden = !ok;
                if (ok) { visible++; }
            });

            if (count) { count.textContent = 'Showing ' + visible + ' of ' + items.length + ' watches'; }
            if (empty) { empty.hidden = visible !== 0; }
        };

        var reorder = function () {
            var mode = sort ? sort.value : 'newest';
            var sorted = items.slice().sort(function (a, b) {
                if (mode === 'low') { return a.dataset.price - b.dataset.price; }
                if (mode === 'high') { return b.dataset.price - a.dataset.price; }
                if (mode === 'name') { return a.dataset.name.localeCompare(b.dataset.name); }
                return a.dataset.order - b.dataset.order;
            });
            sorted.forEach(function (el) { grid.appendChild(el); });
        };

        if (search) { search.addEventListener('input', apply); }
        if (price) { price.addEventListener('input', apply); }
        document.querySelectorAll('input[name="categoryIds"], input[name="brandIds"]').forEach(function (i) {
            i.addEventListener('change', apply);
        });
        if (sort) { sort.addEventListener('change', reorder); }
        if (clearBtn) {
            clearBtn.addEventListener('click', function () {
                if (search) { search.value = ''; }
                if (price) { price.value = price.max; }
                document.querySelectorAll('input[name="categoryIds"], input[name="brandIds"]').forEach(function (i) { i.checked = false; });
                if (sort) { sort.value = 'newest'; }
                reorder();
                apply();
            });
        }
        apply();
    }

    // ── Admin: product table filter ─────────────────────────────────────
    var adminRows = Array.prototype.slice.call(document.querySelectorAll('tr[data-admin-row]'));
    if (adminRows.length) {
        var aSearch = document.getElementById('adminSearch');
        var aCat = document.getElementById('adminCategory');
        var aBrand = document.getElementById('adminBrand');
        var aCount = document.getElementById('adminCount');

        var filterAdmin = function () {
            var q = aSearch ? aSearch.value.trim().toLowerCase() : '';
            var visible = 0;
            adminRows.forEach(function (row) {
                var ok = (!q || row.dataset.search.indexOf(q) !== -1) &&
                    (!aCat || !aCat.value || row.dataset.cat === aCat.value) &&
                    (!aBrand || !aBrand.value || row.dataset.brand === aBrand.value);
                row.hidden = !ok;
                if (ok) { visible++; }
            });
            if (aCount) { aCount.textContent = 'Showing ' + visible + ' of ' + adminRows.length; }
        };

        [aSearch, aCat, aBrand].forEach(function (el) {
            if (el) { el.addEventListener(el.tagName === 'INPUT' ? 'input' : 'change', filterAdmin); }
        });
    }

    // ── Admin: mobile sidebar ───────────────────────────────────────────
    var sidebar = document.getElementById('adminSidebar');
    var backdrop = document.getElementById('adminBackdrop');
    if (sidebar && backdrop) {
        var setOpen = function (open) {
            sidebar.classList.toggle('is-open', open);
            backdrop.classList.toggle('is-open', open);
        };
        document.querySelectorAll('[data-admin-toggle]').forEach(function (btn) {
            btn.addEventListener('click', function () { setOpen(!sidebar.classList.contains('is-open')); });
        });
        backdrop.addEventListener('click', function () { setOpen(false); });
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape') { setOpen(false); } });
    }
})();
