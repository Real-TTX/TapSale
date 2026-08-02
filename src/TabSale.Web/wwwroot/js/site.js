(() => {
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
