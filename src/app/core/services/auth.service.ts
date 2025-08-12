import { Injectable, signal } from '@angular/core';
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

    constructor(private http: HttpClient) {
        // Initialize token from localStorage on service creation
        this.initializeTokenFromStorage();
    }

    private initializeTokenFromStorage(): void {
        const tokenData = this.getTokenFromStorage();
        if (tokenData) {
            this._token.set(tokenData.token);
        }
    }

    private getTokenFromStorage(): TokenData | null {
        try {
            const token = localStorage.getItem(this.TOKEN_KEY);
            const expiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
            
            if (!token || !expiry) {
                return null;
            }

            const expiresAt = parseInt(expiry, 10);
            if (isNaN(expiresAt)) {
                this.clearTokenFromStorage();
                return null;
            }

            return { token, expiresAt };
        } catch (error) {
            console.error('Error reading token from localStorage:', error);
            return null;
        }
    }

    private saveTokenToStorage(token: string, expiresAt: number): void {
        try {
            localStorage.setItem(this.TOKEN_KEY, token);
            localStorage.setItem(this.TOKEN_EXPIRY_KEY, expiresAt.toString());
        } catch (error) {
            console.error('Error saving token to localStorage:', error);
        }
    }

    private clearTokenFromStorage(): void {
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
                    // Use expires_at if provided, otherwise calculate from expires_in
                    const expiresAt = res.expires_at || (Date.now() + (res.expires_in || 3600) * 1000);
                    this.saveTokenToStorage(res.access_token, expiresAt);
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
