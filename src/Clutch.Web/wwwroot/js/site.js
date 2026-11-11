// Toast
window.clToast = function (msg) {
  const el = document.getElementById('cl-toast');
  if (!el || !msg) return;
  el.textContent = msg; el.hidden = false;
  clearTimeout(window.__clToastT);
  window.__clToastT = setTimeout(() => { el.hidden = true; }, 3500);
};

// Leaderboard: re-rank rows with a FLIP animation. Phase 7: call from the SignalR "ranking" message.
window.clRenderBoard = function (rows) {
  const board = document.getElementById('cl-board');
  if (!board) return;
  const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const els = [...board.children];
  const first = new Map(els.map(e => [e.dataset.team, e.getBoundingClientRect().top]));
  const byTeam = new Map(els.map(e => [e.dataset.team, e]));
  rows.sort((a, b) => b.score - a.score).forEach((r, i) => {
    const el = byTeam.get(r.team); if (!el) return;
    const scoreEl = el.querySelector('.cl-score');
    const changed = scoreEl.dataset.value !== String(r.score);
    scoreEl.dataset.value = r.score;
    scoreEl.textContent = r.score.toFixed(1);
    el.querySelector('.cl-rank').textContent = i + 1;
    board.appendChild(el);
    if (changed && !reduce) { el.classList.remove('cl-row--updated'); void el.offsetWidth; el.classList.add('cl-row--updated'); }
  });
  if (reduce) return;
  [...board.children].forEach(el => {
    const dy = first.get(el.dataset.team) - el.getBoundingClientRect().top;
    if (!dy) return;
    el.style.transition = 'none'; el.style.transform = `translateY(${dy}px)`;
    requestAnimationFrame(() => { el.style.transition = ''; el.style.transform = ''; });
  });
};

// Judge page: live total preview
document.addEventListener('input', e => {
  if (!e.target.matches('[data-criterion]')) return;
  const form = e.target.closest('form'); if (!form) return;
  let total = 0;
  form.querySelectorAll('[data-criterion]').forEach(i => {
    total += (Number(i.value) / Number(i.max)) * Number(i.dataset.weight);
    const out = document.getElementById(i.id + '-out'); if (out) out.textContent = i.value + '/' + i.max;
  });
  const t = document.getElementById('cl-total'); if (t) t.textContent = total.toFixed(1);
});
