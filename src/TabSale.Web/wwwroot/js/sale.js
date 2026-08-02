(() => {
  const app = document.getElementById('saleApp');
  if (!app) return;

  const catalog = JSON.parse(document.getElementById('catalogData').textContent || '[]');
  const select = document.getElementById('saleListSelect');
  const grid = document.getElementById('productGrid');
  const categoryFilter = document.getElementById('categoryFilter');
  const payBar = document.getElementById('payBar');
  const dialog = document.getElementById('paymentDialog');
  const csrf = document.querySelector('meta[name="csrf-token"]')?.content || '';
  const euro = new Intl.NumberFormat(document.documentElement.lang || 'en', { style:'currency', currency:'EUR' });
  const deviceToken = localStorage.tabsaleDeviceToken ||= crypto.randomUUID();
  const userId = Number(app.dataset.userId);
  const words = {
    ready:app.dataset.ready,
    pending:app.dataset.pending,
    synced:app.dataset.synced,
    all:app.dataset.allCategories,
    other:app.dataset.otherCategory
  };
  const iconSymbols = {
    tag:'🏷️', drink:'🥤', beer:'🍺', food:'🍽️', sausage:'🌭', dessert:'🍰',
    coffee:'☕', wine:'🍷', snack:'🥨', icecream:'🍦', ticket:'🎟️', deposit:'↩️'
  };

  let listId = Number(select?.value || app.dataset.activeList || 0);
  let cart = {}, given = 0, inputDigits = '', activeCategory = 'all';
  const cartKey = () => `tabsale-cart-${userId}-${listId}`;
  const categoryKey = () => `tabsale-category-${userId}-${listId}`;
  const currentList = () => catalog.find(x => x.id === listId) || { products:[] };
  const signedPrice = product => product.kind === 'DepositReturn' ? -product.priceCents : product.priceCents;
  const total = () => currentList().products.reduce((sum, product) => sum + signedPrice(product) * (cart[product.id] || 0), 0);
  const storeCart = () => localStorage.setItem(cartKey(), JSON.stringify(cart));
  const loadCart = () => { try { cart = JSON.parse(localStorage.getItem(cartKey()) || '{}'); } catch { cart = {}; } };
  const loadCategory = () => activeCategory = localStorage.getItem(categoryKey()) || 'all';

  function renderCategoryFilter() {
    const products = currentList().products;
    const categories = [...new Map(products.filter(x => x.categoryId).map(x => [String(x.categoryId), {
      id:String(x.categoryId), name:x.categoryName, icon:x.categoryIcon, color:x.categoryColor, sortOrder:x.categorySortOrder
    }])).values()].sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name));
    const hasOther = products.some(x => !x.categoryId);
    if (!categories.length) {
      categoryFilter.hidden = true;
      activeCategory = 'all';
      return;
    }

    const valid = activeCategory === 'all' || categories.some(x => x.id === activeCategory) || (activeCategory === 'other' && hasOther);
    if (!valid) activeCategory = 'all';
    categoryFilter.hidden = false;
    categoryFilter.innerHTML = '';

    const addButton = (id, label, symbol, color) => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'category-chip';
      button.classList.toggle('active', activeCategory === id);
      button.style.setProperty('--category-color', color || 'var(--brand)');
      const icon = document.createElement('span'); icon.textContent = symbol;
      const name = document.createElement('strong'); name.textContent = label;
      button.append(icon, name);
      button.onclick = () => {
        activeCategory = id;
        localStorage.setItem(categoryKey(), id);
        render();
      };
      categoryFilter.append(button);
    };

    addButton('all', words.all, '▦', 'var(--brand)');
    categories.forEach(category => addButton(category.id, category.name, iconSymbols[category.icon] || iconSymbols.tag, category.color));
    if (hasOther) addButton('other', words.other, '•••', 'var(--muted)');
  }

  function render() {
    const mode = localStorage.tabsaleView || 'tiles';
    grid.className = `product-grid ${mode}`;
    grid.innerHTML = '';
    renderCategoryFilter();
    const products = currentList().products.filter(product => activeCategory === 'all'
      || (activeCategory === 'other' ? !product.categoryId : String(product.categoryId) === activeCategory));

    for (const product of products) {
      const count = cart[product.id] || 0;
      const element = document.createElement('article');
      element.className = `product-card ${product.kind === 'DepositReturn' ? 'return' : ''} ${count > 0 ? 'has-count' : ''}`;
      element.style.setProperty('--product-color', product.color);
      const visual = product.imageUrl
        ? `<img class="product-image" src="${product.imageUrl}" alt="" draggable="false">`
        : (iconSymbols[product.icon] || iconSymbols.tag);
      element.innerHTML = `<button class="product-add" type="button" aria-label="Add"><span class="product-icon">${visual}</span><span class="product-copy"><strong></strong><small>${euro.format(signedPrice(product)/100)}</small><em class="product-count">${count}×</em></span><b aria-hidden="true">+</b></button><button class="product-minus" type="button" aria-label="Remove" ${count === 0 ? 'disabled' : ''}>−</button>`;
      element.querySelector('strong').textContent = product.name;
      element.querySelector('.product-add').onclick = () => { cart[product.id] = count + 1; storeCart(); render(); };
      element.querySelector('.product-minus').onclick = event => {
        event.stopPropagation();
        if (count <= 1) delete cart[product.id]; else cart[product.id] = count - 1;
        storeCart(); render();
      };
      grid.append(element);
    }

    const value = total();
    document.getElementById('topTotal').textContent = euro.format(value/100);
    document.getElementById('bottomTotal').textContent = euro.format(value/100);
    payBar.disabled = Object.keys(cart).length === 0;
  }

  document.querySelectorAll('[data-view]').forEach(button => button.onclick = () => {
    localStorage.tabsaleView = button.dataset.view;
    document.querySelectorAll('[data-view]').forEach(item => item.classList.toggle('active', item === button));
    render();
  });
  document.querySelector(`[data-view="${localStorage.tabsaleView || 'tiles'}"]`)?.click();
  select?.addEventListener('change', () => { listId = Number(select.value); loadCart(); loadCategory(); render(); });
  payBar.onclick = () => { given = 0; inputDigits = ''; updatePayment(); dialog.showModal(); };
  document.querySelectorAll('[data-cash]').forEach(button => button.onclick = () => { given = Number(button.dataset.cash); inputDigits = String(given); updatePayment(); });
  document.querySelectorAll('[data-key]').forEach(button => button.onclick = () => {
    const key = button.dataset.key;
    if (key === 'C') inputDigits = '';
    else if (key === '⌫') inputDigits = inputDigits.slice(0, -1);
    else if (inputDigits.length < 8) inputDigits += key;
    given = Number(inputDigits || 0);
    updatePayment();
  });

  function updatePayment() {
    const due = total(), payout = due < 0;
    document.getElementById('paymentNormal').hidden = payout;
    document.getElementById('paymentPayout').hidden = !payout;
    document.getElementById('dueTotal').textContent = euro.format(due/100);
    document.getElementById('payoutTotal').textContent = euro.format(Math.abs(due)/100);
    document.getElementById('givenDisplay').textContent = euro.format(given/100);
    document.getElementById('changeTotal').textContent = euro.format(Math.max(0, given-due)/100);
    document.getElementById('completeSale').disabled = due > 0 && given < due;
  }

  document.getElementById('completeSale').onclick = async () => {
    const products = currentList().products, due = total();
    const sale = {
      token:crypto.randomUUID(), deviceToken, saleListId:listId, soldDate:new Date().toISOString(), tenderedCents:due > 0 ? given : null,
      lines:Object.entries(cart).map(([id, quantity]) => { const product = products.find(x => x.id === Number(id)); return { productId:product.id, version:product.version, quantity }; })
    };
    sale.ownerId = userId;
    await queueSale(sale);
    cart = {}; storeCart(); dialog.close(); render(); await sync();
  };

  const openDb = () => new Promise((resolve, reject) => {
    const request = indexedDB.open('TabSale', 1);
    request.onupgradeneeded = () => request.result.createObjectStore('pending', { keyPath:'token' });
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
  async function queueSale(sale) { const database=await openDb(); await new Promise((resolve,reject)=>{const transaction=database.transaction('pending','readwrite');transaction.objectStore('pending').put(sale);transaction.oncomplete=resolve;transaction.onerror=()=>reject(transaction.error);}); }
  async function pendingSales() { const database=await openDb(); return new Promise((resolve,reject)=>{const request=database.transaction('pending').objectStore('pending').getAll();request.onsuccess=()=>resolve(request.result.filter(x=>x.ownerId===userId));request.onerror=()=>reject(request.error);}); }
  async function removeSales(tokens) { const database=await openDb(); await new Promise((resolve,reject)=>{const transaction=database.transaction('pending','readwrite');tokens.forEach(x=>transaction.objectStore('pending').delete(x));transaction.oncomplete=resolve;transaction.onerror=()=>reject(transaction.error);}); }
  async function sync() {
    const state=document.getElementById('syncState'), sales=await pendingSales();
    if (!navigator.onLine || !sales.length) { state.innerHTML=sales.length?`● <span>${sales.length} ${words.pending}</span>`:`● <span>${words.ready}</span>`;state.classList.toggle('pending',sales.length>0);return; }
    try { const response=await fetch('/api/sales/sync',{method:'POST',headers:{'Content-Type':'application/json','X-CSRF-TOKEN':csrf},body:JSON.stringify({sales})});if(!response.ok)throw new Error(await response.text());const data=await response.json();await removeSales(data.accepted);state.innerHTML=`● <span>${words.synced}</span>`;state.classList.remove('pending'); }
    catch { state.innerHTML=`● <span>${sales.length} ${words.pending}</span>`;state.classList.add('pending'); }
  }

  loadCart(); loadCategory(); render(); sync(); addEventListener('online', sync);
})();
