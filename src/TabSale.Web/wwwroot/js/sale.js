(() => {
  const app = document.getElementById('saleApp'); if (!app) return;
  const catalog = JSON.parse(document.getElementById('catalogData').textContent || '[]');
  const select = document.getElementById('saleListSelect');
  const grid = document.getElementById('productGrid');
  const payBar = document.getElementById('payBar');
  const dialog = document.getElementById('paymentDialog');
  const csrf = document.querySelector('meta[name="csrf-token"]')?.content || '';
  const euro = new Intl.NumberFormat(document.documentElement.lang || 'en', { style: 'currency', currency: 'EUR' });
  const deviceToken = localStorage.tabsaleDeviceToken ||= crypto.randomUUID();
  const userId = Number(app.dataset.userId);
  const words = { ready: app.dataset.ready, pending: app.dataset.pending, synced: app.dataset.synced };
  let listId = Number(select?.value || app.dataset.activeList || 0), cart = {}, given = 0, inputDigits = '';
  const cartKey = () => `tabsale-cart-${userId}-${listId}`;
  const currentList = () => catalog.find(x => x.id === listId) || { products: [] };
  const signedPrice = p => p.kind === 'DepositReturn' ? -p.priceCents : p.priceCents;
  const total = () => currentList().products.reduce((sum, p) => sum + signedPrice(p) * (cart[p.id] || 0), 0);
  const storeCart = () => localStorage.setItem(cartKey(), JSON.stringify(cart));
  const loadCart = () => { try { cart = JSON.parse(localStorage.getItem(cartKey()) || '{}'); } catch { cart = {}; } };

  function render() {
    const mode = localStorage.tabsaleView || 'tiles'; grid.className = `product-grid ${mode}`; grid.innerHTML = '';
    for (const p of currentList().products) {
      const count = cart[p.id] || 0, el = document.createElement('article'); el.className = `product-card ${p.kind === 'DepositReturn' ? 'return' : ''} ${count > 0 ? 'has-count' : ''}`;
      el.style.setProperty('--product-color', p.color); el.innerHTML = `<button class="product-add" type="button" aria-label="Add"><span class="product-icon">${p.kind === 'DepositReturn' ? '↙' : p.kind === 'DepositCharge' ? '↗' : '●'}</span><span class="product-copy"><strong></strong><small>${euro.format(signedPrice(p)/100)}</small><em class="product-count">${count}×</em></span><b aria-hidden="true">+</b></button><button class="product-minus" type="button" aria-label="Remove" ${count === 0 ? 'disabled' : ''}>−</button>`;
      el.querySelector('strong').textContent = p.name;
      el.querySelector('.product-add').onclick = () => { cart[p.id] = count + 1; storeCart(); render(); };
      el.querySelector('.product-minus').onclick = event => { event.stopPropagation(); if (count <= 1) delete cart[p.id]; else cart[p.id] = count - 1; storeCart(); render(); };
      grid.append(el);
    }
    const value = total(); document.getElementById('topTotal').textContent = euro.format(value/100); document.getElementById('bottomTotal').textContent = euro.format(value/100);
    payBar.disabled = Object.keys(cart).length === 0;
  }

  document.querySelectorAll('[data-view]').forEach(button => button.onclick = () => {
    localStorage.tabsaleView = button.dataset.view; document.querySelectorAll('[data-view]').forEach(x => x.classList.toggle('active', x === button)); render();
  });
  document.querySelector(`[data-view="${localStorage.tabsaleView || 'tiles'}"]`)?.click();
  select?.addEventListener('change', () => { listId = Number(select.value); loadCart(); render(); });
  payBar.onclick = () => { given = 0; inputDigits = ''; updatePayment(); dialog.showModal(); };
  document.querySelectorAll('[data-cash]').forEach(b => b.onclick = () => { given = Number(b.dataset.cash); inputDigits = String(given); updatePayment(); });
  document.querySelectorAll('[data-key]').forEach(b => b.onclick = () => {
    const key = b.dataset.key; if (key === 'C') inputDigits = ''; else if (key === '⌫') inputDigits = inputDigits.slice(0, -1); else if (inputDigits.length < 8) inputDigits += key;
    given = Number(inputDigits || 0); updatePayment();
  });
  function updatePayment() {
    const due = total(), payout = due < 0; document.getElementById('paymentNormal').hidden = payout; document.getElementById('paymentPayout').hidden = !payout;
    document.getElementById('dueTotal').textContent = euro.format(due/100); document.getElementById('payoutTotal').textContent = euro.format(Math.abs(due)/100);
    document.getElementById('givenDisplay').textContent = euro.format(given/100); document.getElementById('changeTotal').textContent = euro.format(Math.max(0, given-due)/100);
    document.getElementById('completeSale').disabled = due > 0 && given < due;
  }
  document.getElementById('completeSale').onclick = async () => {
    const products = currentList().products, due = total();
    const sale = { token: crypto.randomUUID(), deviceToken, saleListId: listId, soldDate: new Date().toISOString(), tenderedCents: due > 0 ? given : null,
      lines: Object.entries(cart).map(([id, quantity]) => { const p = products.find(x => x.id === Number(id)); return { productId: p.id, version: p.version, quantity }; }) };
    sale.ownerId = userId; await queueSale(sale); cart = {}; storeCart(); dialog.close(); render(); await sync();
  };

  const openDb = () => new Promise((resolve, reject) => { const req = indexedDB.open('TabSale', 1); req.onupgradeneeded = () => req.result.createObjectStore('pending', { keyPath:'token' }); req.onsuccess = () => resolve(req.result); req.onerror = () => reject(req.error); });
  async function queueSale(sale) { const db = await openDb(); await new Promise((resolve,reject) => { const tx=db.transaction('pending','readwrite'); tx.objectStore('pending').put(sale); tx.oncomplete=resolve; tx.onerror=()=>reject(tx.error); }); }
  async function pendingSales() { const db=await openDb(); return new Promise((resolve,reject)=>{ const req=db.transaction('pending').objectStore('pending').getAll(); req.onsuccess=()=>resolve(req.result.filter(x=>x.ownerId===userId)); req.onerror=()=>reject(req.error); }); }
  async function removeSales(tokens) { const db=await openDb(); await new Promise((resolve,reject)=>{ const tx=db.transaction('pending','readwrite'); tokens.forEach(x=>tx.objectStore('pending').delete(x)); tx.oncomplete=resolve; tx.onerror=()=>reject(tx.error); }); }
  async function sync() {
    const state=document.getElementById('syncState'), sales=await pendingSales();
    if (!navigator.onLine || !sales.length) { state.innerHTML = sales.length ? `● <span>${sales.length} ${words.pending}</span>` : `● <span>${words.ready}</span>`; state.classList.toggle('pending', sales.length>0); return; }
    try { const res=await fetch('/api/sales/sync',{method:'POST',headers:{'Content-Type':'application/json','X-CSRF-TOKEN':csrf},body:JSON.stringify({sales})}); if(!res.ok) throw new Error(await res.text()); const data=await res.json(); await removeSales(data.accepted); state.innerHTML=`● <span>${words.synced}</span>`; state.classList.remove('pending'); }
    catch { state.innerHTML=`● <span>${sales.length} ${words.pending}</span>`; state.classList.add('pending'); }
  }
  loadCart(); render(); sync(); addEventListener('online', sync);
})();
