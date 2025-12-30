import { Injectable, signal } from '@angular/core';

export interface ClientPrincipal {
    identityProvider: string;
    userId: string;
    userDetails: string;
    userRoles: string[];
}

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    public user = signal<ClientPrincipal | null>(null);
    public apiAuthResult = signal<any | null>(null);

    constructor() {
        this.checkSession();
    }

    async checkSession() {
        try {
            const response = await fetch('/.auth/me');
            const payload = await response.json();
            const { clientPrincipal } = payload;
            this.user.set(clientPrincipal);
        } catch (error) {
            console.error('Failed to fetch auth session', error);
            this.user.set(null);
        }
    }

    async verifyApiAuth() {
        try {
            const response = await fetch('/api/GetAuthInfo');
            const result = await response.json();
            this.apiAuthResult.set(result);
        } catch (error) {
            console.error('Failed to call API auth endpoint', error);
            this.apiAuthResult.set({ error: 'Failed to call API' });
        }
    }

    login() {
        // In SWA, login is a simple redirect to the provider's endpoint
        window.location.href = '/.auth/login/github';
    }

    logout() {
        window.location.href = '/.auth/logout';
    }
}
