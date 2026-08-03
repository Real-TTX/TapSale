(() => {
  const app = document.getElementById('saleApp');
  if (!app) return;

  const catalog = JSON.parse(document.getElementById('catalogData').textContent || '[]');
  const listDialog = document.getElementById('saleListDialog');
  const listOpen = document.getElementById('saleListOpen');
  const listName = document.getElementById('saleListName');
  const cashShiftLink = document.getElementById('cashShiftLink');
  const grid = document.getElementById('productGrid');
  const categoryFilter = document.getElementById('categoryFilter');
  const payBar = document.getElementById('payBar');
  const dialog = document.getElementById('paymentDialog');
  const restaurantToggle = document.getElementById('restaurantToggle');
  const orderLines = document.getElementById('orderLines');
  const paymentLines = document.getElementById('paymentLines');
  const csrf = document.querySelector('meta[name="csrf-token"]')?.content || '';
  const euro = new Intl.NumberFormat(document.documentElement.lang || 'en', { style:'currency', currency:'EUR' });
  const deviceToken = localStorage.tabsaleDeviceToken ||= crypto.randomUUID();
  const userId = Number(app.dataset.userId);
  const words = {
    ready:app.dataset.ready,
    pending:app.dataset.pending,
    synced:app.dataset.synced,
    all:app.dataset.allCategories,
    other:app.dataset.otherCategory,
    backCategories:app.dataset.backCategories,
    order:app.dataset.order,
    emptyOrder:app.dataset.emptyOrder,
    item:app.dataset.item,
    items:app.dataset.items
  };
  const iconSymbols = {
    tag:'🏷️', drink:'🥤', beer:'🍺', food:'🍽️', sausage:'🌭', dessert:'🍰',
    coffee:'☕', wine:'🍷', snack:'🥨', icecream:'🍦', ticket:'🎟️', deposit:'↩️'
  };

  let listId = Number(app.dataset.activeList || 0);
  let cart = {}, given = 0, inputDigits = '', activeCategory = 'all';
  const restaurantMedia = matchMedia('(min-width: 900px) and (orientation: landscape)');
  let restaurantPreference = localStorage.getItem('tabsaleRestaurantMode');
  const cartKey = () => `tabsale-cart-${userId}-${listId}`;
  const categoryKey = () => `tabsale-category-${userId}-${listId}`;
  const currentList = () => catalog.find(x => x.id === listId) || { products:[] };
  const signedPrice = product => product.kind === 'DepositReturn' ? -product.priceCents : product.priceCents;
  const total = () => currentList().products.reduce((sum, product) => sum + signedPrice(product) * (cart[product.id] || 0), 0);
  const storeCart = () => localStorage.setItem(cartKey(), JSON.stringify(cart));
  const loadCart = () => { try { cart = JSON.parse(localStorage.getItem(cartKey()) || '{}'); } catch { cart = {}; } };
  const loadCategory = () => activeCategory = localStorage.getItem(categoryKey()) || 'all';
  const orderProducts = () => currentList().products.filter(product => (cart[product.id] || 0) > 0);
  const itemCount = () => orderProducts().reduce((sum, product) => sum + cart[product.id], 0);
  const itemCountText = () => { const count = itemCount(); return `${count} ${count === 1 ? words.item : words.items}`; };
  const applyRestaurantMode = () => {
    const enabled = restaurantPreference === 'on' || (restaurantPreference === null && restaurantMedia.matches);
    app.classList.toggle('restaurant-layout', enabled);
    restaurantToggle?.classList.toggle('active', enabled);
    restaurantToggle?.setAttribute('aria-pressed', String(enabled));
  };
  const setQuantity = (productId, quantity) => {
    if (quantity <= 0) delete cart[productId]; else cart[productId] = quantity;
    storeCart(); render();
  };

  function categoryInfo() {
    const products = currentList().products;
    const categories = [...new Map(products.filter(x => x.categoryId).map(x => [String(x.categoryId), {
      id:String(x.categoryId), name:x.categoryName, icon:x.categoryIcon, color:x.categoryColor, sortOrder:x.categorySortOrder
    }])).values()].sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name));
    const hasOther = products.some(x => !x.categoryId);
    return { categories, hasOther };
  }

  const matchesCategory = (product, categoryId) => categoryId === 'all'
    || (categoryId === 'other' ? !product.categoryId : String(product.categoryId) === categoryId);
  const selectCategory = categoryId => {
    activeCategory = categoryId;
    localStorage.setItem(categoryKey(), categoryId);
    render();
  };

  function normalizeCategory({ categories, hasOther }) {
    const valid = activeCategory === 'all' || categories.some(x => x.id === activeCategory) || (activeCategory === 'other' && hasOther);
    if (!valid) activeCategory = 'all';
  }

  function renderCategoryFilter(info) {
    const { categories, hasOther } = info;
    if (!categories.length) { categoryFilter.hidden = true; activeCategory = 'all'; return; }

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
      button.onclick = () => selectCategory(id);
      categoryFilter.append(button);
    };

    addButton('all', words.all, '▦', 'var(--brand)');
    categories.forEach(category => addButton(category.id, category.name, iconSymbols[category.icon] || iconSymbols.tag, category.color));
    if (hasOther) addButton('other', words.other, '•••', 'var(--muted)');
  }

  function renderCategoryBack(info) {
    categoryFilter.innerHTML = '';
    if (activeCategory === 'all') { categoryFilter.hidden = true; return; }
    const category = activeCategory === 'other' ? { name:words.other, icon:null, color:'var(--muted)' }
      : info.categories.find(x => x.id === activeCategory);
    const button = document.createElement('button');
    button.type = 'button'; button.className = 'category-back';
    const arrow = document.createElement('b'); arrow.textContent = '←';
    const copy = document.createElement('span');
    const label = document.createElement('small'); label.textContent = words.backCategories;
    const name = document.createElement('strong'); name.textContent = category?.name || words.all;
    copy.append(label, name); button.append(arrow, copy);
    button.onclick = () => selectCategory('all');
    categoryFilter.append(button); categoryFilter.hidden = false;
  }

  function createCategoryCard(category, count) {
    const card = document.createElement('button');
    card.type = 'button'; card.className = 'category-card';
    card.style.setProperty('--category-color', category.color || 'var(--brand)');
    const icon = document.createElement('span'); icon.className = 'category-card-icon';
    icon.textContent = category.id === 'other' ? '•••' : (iconSymbols[category.icon] || iconSymbols.tag);
    const copy = document.createElement('span'); copy.className = 'category-card-copy';
    const name = document.createElement('strong'); name.textContent = category.name;
    const amount = document.createElement('small'); amount.textContent = `${count} ${count === 1 ? words.item : words.items}`;
    copy.append(name, amount);
    const arrow = document.createElement('b'); arrow.textContent = '→';
    card.append(icon, copy, arrow); card.onclick = () => selectCategory(category.id);
    return card;
  }

  function createOrderVisual(product) {
    const visual = document.createElement('span');
    visual.className = 'order-line-visual';
    if (product.imageUrl) {
      const image = document.createElement('img'); image.src = product.imageUrl; image.alt = '';
      visual.append(image);
    } else visual.textContent = iconSymbols[product.icon] || iconSymbols.tag;
    return visual;
  }

  function renderOrderPanel() {
    const products = orderProducts();
    orderLines.innerHTML = '';
    document.getElementById('orderCount').textContent = itemCountText();
    document.getElementById('orderTotal').textContent = euro.format(total()/100);
    if (!products.length) {
      const empty = document.createElement('p'); empty.className = 'order-empty'; empty.textContent = words.emptyOrder;
      orderLines.append(empty); return;
    }
    for (const product of products) {
      const quantity = cart[product.id];
      const line = document.createElement('article'); line.className = 'order-line';
      const copy = document.createElement('div'); copy.className = 'order-line-copy';
      const name = document.createElement('strong'); name.textContent = product.name;
      const price = document.createElement('small'); price.textContent = euro.format(signedPrice(product) * quantity / 100);
      copy.append(name, price);
      const controls = document.createElement('div'); controls.className = 'order-line-controls';
      const minus = document.createElement('button'); minus.type = 'button'; minus.textContent = '−'; minus.onclick = () => setQuantity(product.id, quantity - 1);
      const count = document.createElement('b'); count.textContent = `${quantity}×`;
      const plus = document.createElement('button'); plus.type = 'button'; plus.textContent = '+'; plus.onclick = () => setQuantity(product.id, quantity + 1);
      controls.append(minus, count, plus);
      line.append(createOrderVisual(product), copy, controls);
      orderLines.append(line);
    }
  }

  function renderPaymentReceipt() {
    const products = orderProducts();
    paymentLines.innerHTML = '';
    document.getElementById('paymentItemCount').textContent = itemCountText();
    for (const product of products) {
      const quantity = cart[product.id];
      const line = document.createElement('div'); line.className = 'payment-line';
      const name = document.createElement('span'); name.textContent = `${quantity}× ${product.name}`;
      const price = document.createElement('strong'); price.textContent = euro.format(signedPrice(product) * quantity / 100);
      line.append(name, price); paymentLines.append(line);
    }
  }

  function createProductCard(product) {
    const count = cart[product.id] || 0;
    const element = document.createElement('article');
    element.className = `product-card ${product.kind === 'DepositReturn' ? 'return' : ''} ${count > 0 ? 'has-count' : ''}`;
    element.style.setProperty('--product-color', product.color);
    const visual = product.imageUrl
      ? `<img class="product-image" src="${product.imageUrl}" alt="" draggable="false">`
      : (iconSymbols[product.icon] || iconSymbols.tag);
    element.innerHTML = `<button class="product-add" type="button" aria-label="Add"><span class="product-icon">${visual}</span><span class="product-copy"><strong></strong><small>${euro.format(signedPrice(product)/100)}</small><em class="product-count">${count}×</em></span><b aria-hidden="true">+</b></button><button class="product-minus" type="button" aria-label="Remove" ${count === 0 ? 'disabled' : ''}>−</button>`;
    element.querySelector('strong').textContent = product.name;
    element.querySelector('.product-add').onclick = () => setQuantity(product.id, count + 1);
    element.querySelector('.product-minus').onclick = event => {
      event.stopPropagation();
      setQuantity(product.id, count - 1);
    };
    return element;
  }

  function renderSections(info, mode) {
    categoryFilter.hidden = true;
    categoryFilter.innerHTML = '';
    grid.className = 'product-sections';
    const groups = info.categories.map(category => ({
      ...category, products:currentList().products.filter(product => matchesCategory(product, category.id))
    }));
    if (info.hasOther) groups.push({ id:'other', name:words.other, icon:null, color:'var(--muted)', products:currentList().products.filter(product => !product.categoryId) });
    for (const group of groups) {
      const section = document.createElement('section'); section.className = 'product-section';
      const heading = document.createElement('h2'); heading.className = 'category-section-title';
      heading.style.setProperty('--category-color', group.color || 'var(--brand)');
      const icon = document.createElement('span'); icon.textContent = group.id === 'other' ? '•••' : (iconSymbols[group.icon] || iconSymbols.tag);
      const name = document.createElement('strong'); name.textContent = group.name;
      const count = document.createElement('small'); count.textContent = String(group.products.length);
      heading.append(icon, name, count);
      const products = document.createElement('div'); products.className = `product-grid ${mode}`;
      group.products.forEach(product => products.append(createProductCard(product)));
      section.append(heading, products); grid.append(section);
    }
  }

  function render() {
    const mode = localStorage.tabsaleView || 'tiles';
    grid.innerHTML = '';
    const info = categoryInfo();
    normalizeCategory(info);
    const categoryMode = currentList().categoryDisplayMode || 'Filter';

    if (categoryMode === 'Sections' && info.categories.length) {
      activeCategory = 'all';
      renderSections(info, mode);
    } else if (categoryMode === 'Drilldown' && info.categories.length && activeCategory === 'all') {
      categoryFilter.hidden = true;
      categoryFilter.innerHTML = '';
      grid.className = 'category-selection';
      info.categories.forEach(category => grid.append(createCategoryCard(category, currentList().products.filter(product => matchesCategory(product, category.id)).length)));
      if (info.hasOther) grid.append(createCategoryCard({ id:'other', name:words.other, icon:null, color:'var(--muted)' }, currentList().products.filter(product => !product.categoryId).length));
    } else {
      grid.className = `product-grid ${mode}`;
      if (categoryMode === 'Drilldown' && info.categories.length) renderCategoryBack(info);
      else renderCategoryFilter(info);
      const products = currentList().products.filter(product => categoryMode === 'Filter' ? matchesCategory(product, activeCategory) : (categoryMode === 'Drilldown' ? matchesCategory(product, activeCategory) : true));
      products.forEach(product => grid.append(createProductCard(product)));
    }

    const value = total();
    document.getElementById('topTotal').textContent = euro.format(value/100);
    document.getElementById('bottomTotal').textContent = euro.format(value/100);
    payBar.disabled = orderProducts().length === 0;
    renderOrderPanel();
  }

  document.querySelectorAll('[data-view]').forEach(button => button.onclick = () => {
    localStorage.tabsaleView = button.dataset.view;
    document.querySelectorAll('[data-view]').forEach(item => item.classList.toggle('active', item === button));
    render();
  });
  restaurantToggle?.addEventListener('click', () => {
    restaurantPreference = app.classList.contains('restaurant-layout') ? 'off' : 'on';
    localStorage.setItem('tabsaleRestaurantMode', restaurantPreference);
    applyRestaurantMode();
  });
  restaurantMedia.addEventListener('change', () => { if (restaurantPreference === null) applyRestaurantMode(); });
  document.querySelector(`[data-view="${localStorage.tabsaleView || 'tiles'}"]`)?.click();
  listOpen?.addEventListener('click', () => listDialog?.showModal());
  document.querySelectorAll('[data-list-id]').forEach(button => button.addEventListener('click', () => {
    listId = Number(button.dataset.listId);
    listName.textContent = currentList().name;
    cashShiftLink.href = `/Sale/Shift?saleListId=${listId}`;
    document.querySelectorAll('[data-list-id]').forEach(item => {
      const active = item === button;
      item.classList.toggle('active', active);
      item.setAttribute('aria-pressed', String(active));
      item.querySelector('b').textContent = active ? '✓' : '→';
    });
    loadCart(); loadCategory(); render(); listDialog.close();
  }));
  payBar.onclick = () => { given = 0; inputDigits = ''; updatePayment(); dialog.showModal(); };
  document.querySelectorAll('[data-cash]').forEach(button => button.onclick = () => {
    given = Math.min(99999999, given + Number(button.dataset.cash));
    inputDigits = String(given);
    updatePayment();
  });
  document.getElementById('clearGiven').onclick = () => { given = 0; inputDigits = ''; updatePayment(); };
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
    renderPaymentReceipt();
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

  applyRestaurantMode(); loadCart(); loadCategory(); render(); sync(); addEventListener('online', sync);
})();
