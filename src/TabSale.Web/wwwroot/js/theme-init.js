(() => {
  const allowed = new Set(['classic', 'winter', 'market', 'contrast', 'custom']);
  const theme = allowed.has(localStorage.tabsaleTheme) ? localStorage.tabsaleTheme : 'classic';
  document.documentElement.dataset.theme = theme;
  if (theme !== 'custom') return;
  try {
    const colors = JSON.parse(localStorage.tabsaleCustomTheme || '{}');
    for (const [name, value] of Object.entries(colors))
      if (['ink','brand','lime','paper','danger'].includes(name) && /^#[0-9a-f]{6}$/i.test(value)) document.documentElement.style.setProperty(`--${name}`, value);
  } catch { localStorage.removeItem('tabsaleCustomTheme'); }
})();
