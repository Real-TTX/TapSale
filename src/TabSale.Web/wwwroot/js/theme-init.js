(() => {
  const allowed = new Set(['classic', 'winter', 'market', 'contrast']);
  document.documentElement.dataset.theme = allowed.has(localStorage.tabsaleTheme) ? localStorage.tabsaleTheme : 'classic';
})();
