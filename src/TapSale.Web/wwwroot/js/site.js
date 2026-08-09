(() => {
  // Copy persisted browser preferences to the corrected app prefix without removing rollback data.
  try {
    const legacyKeys = [];
    for (let index = 0; index < localStorage.length; index += 1) {
      const key = localStorage.key(index);
      if (key?.startsWith('tabsale')) legacyKeys.push(key);
    }
    legacyKeys.forEach(key => {
      const replacement = `tapsale${key.slice('tabsale'.length)}`;
      if (localStorage.getItem(replacement) === null) localStorage.setItem(replacement, localStorage.getItem(key));
    });
  } catch { }

  const language = document.documentElement.lang === 'de' ? 'de' : 'en';
  const dialogWords = language === 'de'
    ? { confirmation:'Bitte bestätigen', confirm:'Bestätigen', cancel:'Abbrechen', okay:'OK', notice:'Hinweis' }
    : { confirmation:'Please confirm', confirm:'Confirm', cancel:'Cancel', okay:'OK', notice:'Notice' };

  const dialog = document.createElement('dialog');
  dialog.className = 'app-dialog';
  dialog.setAttribute('aria-labelledby', 'appDialogTitle');
  dialog.setAttribute('aria-describedby', 'appDialogMessage');
  dialog.innerHTML = `<div class="app-dialog-card"><div class="app-dialog-symbol" aria-hidden="true">?</div><div class="app-dialog-copy"><p class="eyebrow"></p><h2 id="appDialogTitle"></h2><p id="appDialogMessage"></p></div><div class="app-dialog-actions"><button type="button" class="btn btn-light app-dialog-cancel"></button><button type="button" class="btn app-dialog-confirm"></button></div></div>`;
  document.body.append(dialog);

  const dialogTitle = dialog.querySelector('#appDialogTitle');
  const dialogMessage = dialog.querySelector('#appDialogMessage');
  const dialogEyebrow = dialog.querySelector('.eyebrow');
  const dialogSymbol = dialog.querySelector('.app-dialog-symbol');
  const cancelDialog = dialog.querySelector('.app-dialog-cancel');
  const confirmDialog = dialog.querySelector('.app-dialog-confirm');
  let resolveDialog;

  const finishDialog = confirmed => {
    if (!dialog.open) return;
    dialog.close(confirmed ? 'confirm' : 'cancel');
  };

  const showDialog = (message, options = {}) => new Promise(resolve => {
    const alertMode = options.mode === 'alert';
    resolveDialog = resolve;
    dialogEyebrow.textContent = options.eyebrow ?? (alertMode ? dialogWords.notice : dialogWords.confirmation);
    dialogTitle.textContent = options.title ?? (alertMode ? dialogWords.notice : dialogWords.confirmation);
    dialogMessage.textContent = message;
    dialogSymbol.textContent = alertMode ? 'i' : '!';
    dialog.classList.toggle('is-alert', alertMode);
    dialog.classList.toggle('is-danger', options.danger !== false && !alertMode);
    cancelDialog.hidden = alertMode;
    cancelDialog.textContent = options.cancelText ?? dialogWords.cancel;
    confirmDialog.textContent = options.confirmText ?? (alertMode ? dialogWords.okay : dialogWords.confirm);
    confirmDialog.className = `btn app-dialog-confirm ${options.danger !== false && !alertMode ? 'btn-danger' : 'btn-primary'}`;
    dialog.showModal();
    confirmDialog.focus();
  });

  cancelDialog.addEventListener('click', () => finishDialog(false));
  confirmDialog.addEventListener('click', () => finishDialog(true));
  dialog.addEventListener('cancel', event => {
    event.preventDefault();
    finishDialog(false);
  });
  dialog.addEventListener('close', () => {
    const resolve = resolveDialog;
    resolveDialog = null;
    resolve?.(dialog.returnValue === 'confirm');
  });

  document.addEventListener('tapsale:confirm', event => {
    showDialog(event.detail.message, event.detail.options).then(event.detail.resolve);
  });
  document.addEventListener('tapsale:alert', event => {
    showDialog(event.detail.message, { ...event.detail.options, mode:'alert', danger:false }).then(event.detail.resolve);
  });

  document.addEventListener('click', async event => {
    const trigger = event.target.closest('[data-confirm]');
    if (!trigger || trigger.dataset.confirmed === 'true') return;
    event.preventDefault();
    event.stopPropagation();
    const confirmed = await showDialog(trigger.dataset.confirm, {
      title: trigger.dataset.confirmTitle,
      confirmText: trigger.dataset.confirmAction
    });
    if (!confirmed) return;
    trigger.dataset.confirmed = 'true';
    trigger.click();
    delete trigger.dataset.confirmed;
  }, true);

  const sidebar = document.getElementById('sidebar');
  const sidebarBackdrop = document.getElementById('sidebarBackdrop');
  const setMobileMenu = open => {
    sidebar?.classList.toggle('open', open);
    sidebarBackdrop?.classList.toggle('open', open);
    document.body.classList.toggle('mobile-menu-open', open);
  };
  document.getElementById('menuToggle')?.addEventListener('click', () => setMobileMenu(!sidebar?.classList.contains('open')));
  sidebarBackdrop?.addEventListener('click', () => setMobileMenu(false));
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') setMobileMenu(false);
  });
  matchMedia('(min-width: 761px)').addEventListener('change', event => {
    if (event.matches) setMobileMenu(false);
  });
  const collapseButton = document.getElementById('sidebarCollapse');
  const reopenButton = document.getElementById('sidebarReopen');
  const root = document.documentElement;
  const applySidebarState = collapsed => {
    root.classList.toggle('sidebar-collapsed', collapsed);
    collapseButton?.setAttribute('aria-expanded', String(!collapsed));
    reopenButton?.setAttribute('aria-expanded', String(!collapsed));
  };
  applySidebarState(localStorage.tapsaleSidebarCollapsed === 'true');
  collapseButton?.addEventListener('click', () => {
    const collapsed = !root.classList.contains('sidebar-collapsed');
    localStorage.tapsaleSidebarCollapsed = String(collapsed);
    applySidebarState(collapsed);
  });
  reopenButton?.addEventListener('click', () => {
    localStorage.tapsaleSidebarCollapsed = 'false';
    applySidebarState(false);
  });
  const dot = document.getElementById('networkDot');
  const paintNetwork = () => dot?.classList.toggle('offline', !navigator.onLine);
  addEventListener('online', paintNetwork); addEventListener('offline', paintNetwork); paintNetwork();
  if ('serviceWorker' in navigator) navigator.serviceWorker.register('/service-worker.js');
})();
