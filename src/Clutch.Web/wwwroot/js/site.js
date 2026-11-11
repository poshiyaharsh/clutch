// Clutch Clean Product UI — Client Script
(function () {
  'use strict';

  // 1. Theme Management
  const THEME_KEY = 'clutch-theme-preference';

  function getSystemTheme() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  function applyTheme(theme) {
    if (!theme || theme === 'system') {
      document.documentElement.removeAttribute('data-theme');
    } else {
      document.documentElement.setAttribute('data-theme', theme);
    }
    updateThemeToggleUI(theme || 'system');
  }

  function updateThemeToggleUI(currentTheme) {
    const buttons = document.querySelectorAll('[data-theme-value]');
    buttons.forEach(btn => {
      if (btn.dataset.themeValue === currentTheme) {
        btn.classList.add('active');
        btn.setAttribute('aria-pressed', 'true');
      } else {
        btn.classList.remove('active');
        btn.setAttribute('aria-pressed', 'false');
      }
    });
  }

  window.setTheme = function (theme) {
    if (theme === 'system') {
      localStorage.removeItem(THEME_KEY);
    } else {
      localStorage.setItem(THEME_KEY, theme);
    }
    applyTheme(theme);
  };

  // Listen to OS theme changes if user hasn't explicitly set a preference
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
    if (!localStorage.getItem(THEME_KEY)) {
      applyTheme('system');
    }
  });

  // 2. Toast Notification
  window.clToast = function (msg, variant) {
    const toast = document.getElementById('cl-toast');
    if (!toast || !msg) return;

    const messageEl = toast.querySelector('.cl-toast-message');
    if (messageEl) {
      messageEl.textContent = msg;
    } else {
      toast.textContent = msg;
    }

    toast.classList.add('show');
    clearTimeout(window._clToastTimer);
    window._clToastTimer = setTimeout(() => {
      toast.classList.remove('show');
    }, 3600);
  };

  // 3. Command Menu (Ctrl+K / Cmd+K)
  function initCommandMenu() {
    const backdrop = document.getElementById('cl-command-menu');
    const input = document.getElementById('cl-command-input');
    const triggers = document.querySelectorAll('[data-command-trigger]');

    if (!backdrop || !input) return;

    function openMenu() {
      backdrop.classList.add('active');
      input.value = '';
      input.focus();
      filterItems('');
    }

    function closeMenu() {
      backdrop.classList.remove('active');
    }

    triggers.forEach(trig => trig.addEventListener('click', openMenu));

    document.addEventListener('keydown', (e) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        if (backdrop.classList.contains('active')) {
          closeMenu();
        } else {
          openMenu();
        }
      } else if (e.key === 'Escape' && backdrop.classList.contains('active')) {
        closeMenu();
      }
    });

    backdrop.addEventListener('click', (e) => {
      if (e.target === backdrop) closeMenu();
    });

    function filterItems(query) {
      const q = query.toLowerCase().trim();
      const items = backdrop.querySelectorAll('.cl-command-item');
      items.forEach(item => {
        const text = item.textContent.toLowerCase();
        item.style.display = text.includes(q) ? 'flex' : 'none';
      });
    }

    input.addEventListener('input', (e) => filterItems(e.target.value));
  }

  // 4. Copy to Clipboard Utility
  window.copyText = function (text, successMsg) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(() => {
        window.clToast(successMsg || 'Copied to clipboard');
      }).catch(() => fallbackCopy(text, successMsg));
    } else {
      fallbackCopy(text, successMsg);
    }
  };

  function fallbackCopy(text, msg) {
    const el = document.createElement('textarea');
    el.value = text;
    document.body.appendChild(el);
    el.select();
    document.execCommand('copy');
    document.body.removeChild(el);
    window.clToast(msg || 'Copied to clipboard');
  }

  // 5. Password Show/Hide Toggle
  window.togglePasswordVisibility = function (inputId, btnEl) {
    const input = document.getElementById(inputId);
    if (!input) return;
    const isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';

    const iconUse = btnEl.querySelector('use');
    if (iconUse) {
      iconUse.setAttribute('href', isPassword ? '/img/icons.svg#icon-eye-off' : '/img/icons.svg#icon-eye');
    }
  };

  // 6. Judge Live Score Calculation & Keyboard Support
  function initJudgeScoring() {
    const form = document.querySelector('[data-judge-form]');
    if (!form) return;

    function recalculateTotal() {
      let total = 0;
      let count = 0;
      form.querySelectorAll('[data-criterion]').forEach(slider => {
        const max = parseFloat(slider.max) || 10;
        const weight = parseFloat(slider.dataset.weight) || 25;
        const val = parseFloat(slider.value) || 0;
        total += (val / max) * weight;

        const out = document.getElementById(slider.id + '-out');
        if (out) out.textContent = val.toFixed(1) + ' / ' + max;
        count++;
      });

      const totalEl = document.getElementById('cl-total');
      if (totalEl) totalEl.textContent = total.toFixed(1);
    }

    form.addEventListener('input', (e) => {
      if (e.target.matches('[data-criterion]')) {
        recalculateTotal();
      }
    });

    // Keyboard support for judge scoring
    form.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA') {
        const submitBtn = form.querySelector('[type="submit"]');
        if (submitBtn) submitBtn.click();
      }
    });
  }

  // 7. Live Leaderboard Smooth FLIP Animation & Score Count-up
  window.clRenderBoard = function (rows) {
    const board = document.getElementById('cl-board');
    if (!board || !rows) return;

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const currentEls = [...board.children];
    const initialTops = new Map(currentEls.map(el => [el.dataset.team, el.getBoundingClientRect().top]));
    const teamMap = new Map(currentEls.map(el => [el.dataset.team, el]));

    // Sort descending by score
    rows.sort((a, b) => b.score - a.score).forEach((row, idx) => {
      const el = teamMap.get(row.team);
      if (!el) return;

      const rankEl = el.querySelector('.cl-leaderboard-rank');
      if (rankEl) rankEl.textContent = (idx + 1).toString();

      // Top 3 style handling
      if (idx < 3) {
        el.classList.add('cl-leaderboard-row--top');
      } else {
        el.classList.remove('cl-leaderboard-row--top');
      }

      const scoreEl = el.querySelector('.cl-leaderboard-score');
      if (scoreEl) {
        const oldScore = parseFloat(scoreEl.dataset.value) || 0;
        const newScore = parseFloat(row.score) || 0;
        if (oldScore !== newScore) {
          animateValue(scoreEl, oldScore, newScore, 400);
          scoreEl.dataset.value = newScore.toString();
          if (!reduceMotion) {
            el.classList.remove('cl-row--updated');
            void el.offsetWidth;
            el.classList.add('cl-row--updated');
          }
        }
      }

      board.appendChild(el);
    });

    if (reduceMotion) return;

    // Apply FLIP inversion
    [...board.children].forEach(el => {
      const oldTop = initialTops.get(el.dataset.team);
      if (oldTop === undefined) return;
      const newTop = el.getBoundingClientRect().top;
      const dy = oldTop - newTop;
      if (dy !== 0) {
        el.style.transform = `translateY(${dy}px)`;
        el.style.transition = 'none';
        requestAnimationFrame(() => {
          el.style.transition = 'transform 240ms cubic-bezier(0.2, 0, 0, 1)';
          el.style.transform = '';
        });
      }
    });
  };

  function animateValue(element, start, end, duration) {
    if (start === end) {
      element.textContent = end.toFixed(1);
      return;
    }
    const startTime = performance.now();
    function step(now) {
      const progress = Math.min((now - startTime) / duration, 1);
      const current = start + (end - start) * progress;
      element.textContent = current.toFixed(1);
      if (progress < 1) {
        requestAnimationFrame(step);
      } else {
        element.textContent = end.toFixed(1);
      }
    }
    requestAnimationFrame(step);
  }

  // Initialization on DOM ready
  document.addEventListener('DOMContentLoaded', () => {
    initCommandMenu();
    initJudgeScoring();
  });
})();
