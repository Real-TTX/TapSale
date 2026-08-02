(() => {
  const setTheme = theme => {
    const value = ['classic', 'winter', 'market', 'contrast'].includes(theme) ? theme : 'classic';
    document.documentElement.dataset.theme = value;
    localStorage.tabsaleTheme = value;
    document.querySelectorAll('.theme-picker').forEach(picker => picker.value = value);
  };
  setTheme(localStorage.tabsaleTheme || 'classic');
  document.querySelectorAll('.theme-picker').forEach(picker => picker.addEventListener('change', () => setTheme(picker.value)));
  const sidebar = document.getElementById('sidebar');
  document.getElementById('menuToggle')?.addEventListener('click', () => sidebar?.classList.toggle('open'));
  const collapseButton = document.getElementById('sidebarCollapse');
  const applySidebarState = collapsed => {
    document.body.classList.toggle('sidebar-collapsed', collapsed);
    collapseButton?.setAttribute('aria-expanded', String(!collapsed));
    if (collapseButton) collapseButton.textContent = collapsed ? '›' : '‹';
  };
  applySidebarState(localStorage.tabsaleSidebarCollapsed === 'true');
  collapseButton?.addEventListener('click', () => {
    const collapsed = !document.body.classList.contains('sidebar-collapsed');
    localStorage.tabsaleSidebarCollapsed = String(collapsed);
    applySidebarState(collapsed);
  });
  const dot = document.getElementById('networkDot');
  const paintNetwork = () => dot?.classList.toggle('offline', !navigator.onLine);
  addEventListener('online', paintNetwork); addEventListener('offline', paintNetwork); paintNetwork();
  if ('serviceWorker' in navigator) navigator.serviceWorker.register('/service-worker.js');
})();
