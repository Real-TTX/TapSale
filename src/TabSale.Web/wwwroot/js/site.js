(() => {
  const sidebar = document.getElementById('sidebar');
  document.getElementById('menuToggle')?.addEventListener('click', () => sidebar?.classList.toggle('open'));
  const dot = document.getElementById('networkDot');
  const paintNetwork = () => dot?.classList.toggle('offline', !navigator.onLine);
  addEventListener('online', paintNetwork); addEventListener('offline', paintNetwork); paintNetwork();
  if ('serviceWorker' in navigator) navigator.serviceWorker.register('/service-worker.js');
})();
