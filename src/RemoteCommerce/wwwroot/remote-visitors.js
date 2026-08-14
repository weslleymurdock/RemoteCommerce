(() => {
    let lastUrl = window.location.href;

    async function track() {
        try {
            await fetch('/api/rp/v1/remote-visitors/track', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    path: window.location.pathname + window.location.search,
                    referrer: document.referrer || null,
                    userAgent: navigator.userAgent,
                    networkKey: null
                })
            });
        } catch {
            // Visitor telemetry must never affect storefront navigation.
        }
    }

    track();
    window.setInterval(() => {
        if (window.location.href !== lastUrl) {
            lastUrl = window.location.href;
            track();
        }
    }, 1000);
})();
