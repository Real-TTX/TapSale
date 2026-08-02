const CACHE='tabsale-v21';
const SHELL=['/css/site.css','/js/site.js','/js/sale.js','/manifest.webmanifest','/favicon.ico','/icons/app-icon.svg','/icons/app-icon-192.png','/icons/app-icon-512.png'];
self.addEventListener('install',event=>event.waitUntil(caches.open(CACHE).then(c=>c.addAll(SHELL)).then(()=>self.skipWaiting())));
self.addEventListener('activate',event=>event.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(k=>k!==CACHE).map(k=>caches.delete(k)))).then(()=>self.clients.claim())));
self.addEventListener('fetch',event=>{
 if(event.request.method!=='GET'||new URL(event.request.url).origin!==location.origin)return;
 if(event.request.mode==='navigate') event.respondWith(fetch(event.request).then(r=>{const copy=r.clone();caches.open(CACHE).then(c=>c.put(event.request,copy));return r;}).catch(()=>caches.match(event.request).then(r=>r||caches.match('/Sale'))));
 else event.respondWith(caches.match(event.request).then(hit=>hit||fetch(event.request).then(r=>{if(r.ok)caches.open(CACHE).then(c=>c.put(event.request,r.clone()));return r;})));
});
