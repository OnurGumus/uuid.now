const CACHE_NAME = 'site-cache-v071220251440';

// Install event: do nothing or skip directly
self.addEventListener('install', (event) => {
  // This line ensures immediate activation of the new Service Worker
  self.skipWaiting();
});

// Activate event: clean up old caches if name changes
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(
        keys
          .filter((key) => key !== CACHE_NAME)
          .map((key) => caches.delete(key))
      )
    )
  );
  self.clients.claim(); // Take control of any open clients
});

// Fetch event: dynamic caching
self.addEventListener('fetch', (event) => {
  event.respondWith(
    caches.match(event.request).then((cachedResponse) => {
      if (cachedResponse) {
        return cachedResponse; // Serve from cache
      } else {
        // Fetch from network and cache it
        return fetch(event.request).then((networkResponse) => {
          // Check if valid response before caching
          if (!networkResponse || !networkResponse.ok) {
            return networkResponse;
          }
          return caches.open(CACHE_NAME).then((cache) => {
            cache.put(event.request, networkResponse.clone());
            return networkResponse;
          });
        });
      }
    })
  );
});
