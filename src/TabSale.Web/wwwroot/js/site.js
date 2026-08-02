(() => {
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
