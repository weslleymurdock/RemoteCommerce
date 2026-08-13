window.remoteCommerceIdentity = {
    async login(email, password) {
        const response = await fetch('/api/rc/v1/identity/login', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (response.status === 409) {
            return { requiresTwoFactor: true };
        }

        return await this.parse(response);
    },
    async login2fa(email, code, rememberMachine = false) {
        return await this.post(
            '/api/rc/v1/identity/login/2fa',
            { email, code, rememberMachine });
    },
    async loginRecovery(email, recoveryCode) {
        return await this.post(
            '/api/rc/v1/identity/login/recovery',
            { email, recoveryCode });
    },
    async refresh(refreshToken) {
        return await this.post(
            '/api/rc/v1/identity/refresh',
            { refreshToken });
    },
    async setup(displayName, email, password) {
        return await this.post(
            '/api/rc/v1/identity/setup',
            { displayName, email, password });
    },
    async register(email, displayName, password) {
        return await this.post(
            '/api/rc/v1/identity/register',
            { email, displayName, password });
    },
    async logout() {
        return await this.post('/api/rc/v1/identity/logout', {});
    },
    async forgotPassword(email) {
        return await this.post(
            '/api/rc/v1/identity/forgot-password',
            { email });
    },
    async resetPassword(email, resetToken, newPassword) {
        return await this.post(
            '/api/rc/v1/identity/reset-password',
            { email, resetToken, newPassword });
    },
    async confirmEmail(userId, token) {
        return await this.get(
            `/api/rc/v1/identity/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`);
    },
    async resendConfirmation(email) {
        return await this.post(
            '/api/rc/v1/identity/resend-confirmation',
            { email });
    },
    async profile() {
        return await this.get('/api/rc/v1/identity/manage/info');
    },
    async updateProfile(displayName, email) {
        return await this.post(
            '/api/rc/v1/identity/manage/info',
            { displayName, email });
    },
    async twoFactor() {
        return await this.get('/api/rc/v1/identity/manage/2fa');
    },
    async setTwoFactor(enable) {
        return await this.post(
            '/api/rc/v1/identity/manage/2fa',
            { enable });
    },
    async disableTwoFactor() {
        return await this.post(
            '/api/rc/v1/identity/manage/2fa/disable',
            {});
    },
    async recoveryCodes() {
        return await this.post(
            '/api/rc/v1/identity/manage/2fa/recovery-codes',
            {});
    },
    async resetAuthenticator() {
        return await this.post(
            '/api/rc/v1/identity/manage/2fa/reset-authenticator',
            {});
    },
    async get(url) {
        const response = await fetch(url, {
            credentials: 'same-origin'
        });

        return await this.parse(response);
    },
    async post(url, body) {
        const response = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        return await this.parse(response);
    },
    async parse(response) {
        if (!response.ok) {
            const payload = await response.json().catch(() => null);
            const error = new Error(
                payload?.detail ?? 'The identity operation failed.');
            error.status = response.status;
            error.payload = payload;
            throw error;
        }

        return await response.json().catch(() => null);
    }
};
