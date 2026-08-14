(() => {
    let initialized = false;

    async function initialize() {
        if (initialized) return;
        const slots = document.querySelectorAll('[data-remote-adsense-slot]');
        if (!slots.length) return;

        try {
            const response = await fetch('/api/rp/v1/remote-adsense/placements', { credentials: 'include' });
            if (!response.ok) return;
            const placements = await response.json();
            const client = placements.map(x => x.adClient).find(Boolean);
            if (!client) return;

            if (!document.querySelector('script[data-remote-adsense-script]')) {
                const script = document.createElement('script');
                script.async = true;
                script.dataset.remoteAdsenseScript = 'true';
                script.src = `https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${encodeURIComponent(client)}`;
                script.crossOrigin = 'anonymous';
                document.head.appendChild(script);
            }

            for (const element of slots) {
                const slotName = element.getAttribute('data-remote-adsense-slot');
                const placement = placements.find(x => x.slotName === slotName);
                if (!placement) continue;
                element.innerHTML = `<ins class="adsbygoogle" style="display:block" data-ad-client="${placement.adClient}"${placement.adSlot ? ` data-ad-slot="${placement.adSlot}"` : ''} data-ad-format="${placement.format || 'auto'}"></ins>`;
                (window.adsbygoogle = window.adsbygoogle || []).push({});
            }
            initialized = true;
        } catch {
            // Advertising failures must never break storefront rendering.
        }
    }

    initialize();
    window.setInterval(initialize, 1500);
})();
