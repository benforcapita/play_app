import { inject, Inject, PLATFORM_ID } from "@angular/core";
import { isPlatformBrowser } from '@angular/common';
import {CanActivateFn, Router} from "@angular/router";
import {AuthService} from "../services/auth.service";

export const canActivateAuth: CanActivateFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const platformId = inject(PLATFORM_ID);

    try{
        // During SSR, skip auth guard so the server can render the shell
        if (!isPlatformBrowser(platformId)) {
            return true;
        }
        const token = auth.getToken();
        if(token) return true;
        
        // No token found, redirect to login
        router.navigate(['/login']);
        return false;
    }catch(error){
        // Error occurred, redirect to login
        router.navigate(['/login']);
        return false;
    }
}
