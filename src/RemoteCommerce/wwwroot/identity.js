window.remoteCommerceIdentity = {
    async login(email, password) {
        return await this.post('/api/identity/login', { email, password });
    },
    async setup(displayName, email, password) {
        return await this.post('/api/identity/setup', { displayName, email, password });
    },
    async logout() {
        return await this.post('/api/identity/logout', {});
    },
    async post(url, body) {
        const response = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (!response.ok) {
            const payload = await response.json().catch(() => null);
            throw new Error(payload?.detail ?? 'The identity operation failed.');
        }
        return await response.json().catch(() => null);
    }
};
