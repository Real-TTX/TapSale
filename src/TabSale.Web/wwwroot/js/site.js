(() => {
  const colorDefaults = { ink:'#102a2a', brand:'#167d6d', lime:'#e4f26b', paper:'#f4f6f1', danger:'#bc3c3c' };
  const colorNames = Object.keys(colorDefaults);
  const validColor = value => /^#[0-9a-f]{6}$/i.test(value);
  const readCustomColors = () => {
    try { const values = JSON.parse(localStorage.tabsaleCustomTheme || '{}'); return Object.fromEntries(colorNames.map(name => [name, validColor(values[name]) ? values[name] : colorDefaults[name]])); }
    catch { return { ...colorDefaults }; }
  };
  const applyCustomColors = colors => colorNames.forEach(name => document.documentElement.style.setProperty(`--${name}`, colors[name]));
  const clearCustomColors = () => colorNames.forEach(name => document.documentElement.style.removeProperty(`--${name}`));
  const syncColorInputs = colors => document.querySelectorAll('[data-theme-color]').forEach(input => input.value = colors[input.dataset.themeColor]);
  const setTheme = theme => {
    const value = ['classic', 'winter', 'market', 'contrast', 'custom'].includes(theme) ? theme : 'classic';
    document.documentElement.dataset.theme = value;
    localStorage.tabsaleTheme = value;
    if (value === 'custom') applyCustomColors(readCustomColors()); else clearCustomColors();
    document.querySelectorAll('.theme-picker').forEach(picker => picker.value = value);
  };
  setTheme(localStorage.tabsaleTheme || 'classic');
  syncColorInputs(readCustomColors());
  document.querySelectorAll('.theme-picker').forEach(picker => picker.addEventListener('change', () => setTheme(picker.value)));
  document.querySelectorAll('[data-theme-color]').forEach(input => input.addEventListener('input', () => {
    const colors = readCustomColors(); colors[input.dataset.themeColor] = input.value;
    localStorage.tabsaleCustomTheme = JSON.stringify(colors); syncColorInputs(colors); setTheme('custom');
  }));
  document.querySelectorAll('.theme-reset').forEach(button => button.addEventListener('click', () => {
    localStorage.tabsaleCustomTheme = JSON.stringify(colorDefaults); syncColorInputs(colorDefaults); setTheme('custom');
  }));
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
