import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpInterceptorFn,HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Router } from '@angular/router';


export const authInterceptor:HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const platformId = inject(PLATFORM_ID);
    if (req.url.includes('api/auth')) {
        return next(req);
    }

    let token:string | null = null;
    if (isPlatformBrowser(platformId)) {
        try {
            token = auth.getToken();
        } catch (error) {
            console.error('Error getting token:', error);
        }
    }

    const authReq = token ? req.clone({
        setHeaders: {
            Authorization: `Bearer ${token}`
        }
    }) : req;

    return next(authReq).pipe(
        catchError((error:HttpErrorResponse) => {
            if (error.status === 401) {
                auth.logout();
                router.navigate(['/login']);
            }
            return throwError(() => error);
        })
    );
    
}
