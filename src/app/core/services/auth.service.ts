import { Injectable, signal, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

type loginResponse = {
    access_token: string;
    refresh_token?: string;
    expires_in?: number;
    expires_at?: number;
    token_type?: string;
    user?: any;
}

interface TokenData {
    token: string;
    expiresAt: number;
}

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private _token = signal<string | null>(null);
    token = this._token.asReadonly();
    private readonly TOKEN_KEY = 'auth_token';
    private readonly TOKEN_EXPIRY_KEY = 'auth_token_expiry';
    private readonly platformId = inject(PLATFORM_ID);

    constructor(private http: HttpClient) {
        // Only attempt storage access in the browser
        if (isPlatformBrowser(this.platformId)) {
            this.initializeTokenFromStorage();
        }
    }

    private initializeTokenFromStorage(): void {
        const tokenData = this.getTokenFromStorage();
        if (tokenData) {
            this._token.set(tokenData.token);
        }
    }

    private getTokenFromStorage(): TokenData | null {
        if (!isPlatformBrowser(this.platformId)) {
            return null;
        }
        try {
            const token = localStorage.getItem(this.TOKEN_KEY);
            const expiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
            
            if (!token || !expiry) {
                return null;
            }

            let expiresAt = parseInt(expiry, 10);
            if (isNaN(expiresAt)) {
                this.clearTokenFromStorage();
                return null;
            }

            // Auto-migrate seconds -> milliseconds if needed
            if (expiresAt < 1e12) {
                expiresAt = expiresAt * 1000;
                this.saveTokenToStorage(token, expiresAt);
            }

            return { token, expiresAt };
        } catch (error) {
            console.error('Error reading token from localStorage:', error);
            return null;
        }
    }

    private saveTokenToStorage(token: string, expiresAt: number): void {
        if (!isPlatformBrowser(this.platformId)) {
            return;
        }
        try {
            localStorage.setItem(this.TOKEN_KEY, token);
            localStorage.setItem(this.TOKEN_EXPIRY_KEY, expiresAt.toString());
        } catch (error) {
            console.error('Error saving token to localStorage:', error);
        }
    }

    private clearTokenFromStorage(): void {
        if (!isPlatformBrowser(this.platformId)) {
            return;
        }
        try {
            localStorage.removeItem(this.TOKEN_KEY);
            localStorage.removeItem(this.TOKEN_EXPIRY_KEY);
        } catch (error) {
            console.error('Error clearing token from localStorage:', error);
        }
    }

    private isTokenExpired(tokenData: TokenData): boolean {
        return Date.now() >= tokenData.expiresAt;
    }

    login(email: string, password: string) {
        return this.http.post<loginResponse>('api/auth/login', { email, password })
            .pipe(
                tap(res => {
                    this._token.set(res.access_token);
                    // Normalize expiry to milliseconds
                    let expiresAtMs: number;
                    if (typeof res.expires_at === 'number' && !isNaN(res.expires_at)) {
                        expiresAtMs = res.expires_at < 1e12 ? res.expires_at * 1000 : res.expires_at;
                    } else {
                        const seconds = (res.expires_in ?? 3600);
                        expiresAtMs = Date.now() + seconds * 1000;
                    }
                    this.saveTokenToStorage(res.access_token, expiresAtMs);
                })
            );
    }

    logout() {
        this._token.set(null);
        this.clearTokenFromStorage();
    }

    isAuthenticated(): boolean {
        const token = this.getToken();
        return token !== null;
    }

    getToken(): string | null {
        if (!isPlatformBrowser(this.platformId)) {
            // Avoid SSR touching browser storage. Treat as unauthenticated on server.
            return null;
        }
        const tokenData = this.getTokenFromStorage();
        
        if (!tokenData) {
            this._token.set(null);
            return null;
        }

        if (this.isTokenExpired(tokenData)) {
            // Token has expired, clear it and throw error
            this.logout();
            throw new Error('Token has expired. Please log in again.');
        }

        // Update the signal with the valid token
        this._token.set(tokenData.token);
        return tokenData.token;
    }
}
