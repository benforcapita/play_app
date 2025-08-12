import { inject } from '@angular/core';
import { HttpInterceptorFn,HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';


export const authInterceptor:HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthService);
    if (req.url.includes('api/auth')) {
        return next(req);
    }

    let token:string | null = null;
    try {
        token = auth.getToken();
    } catch (error) {
        console.error('Error getting token:', error);
        return next(req);
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
            }
            return throwError(() => error);
        })
    );
    
}
