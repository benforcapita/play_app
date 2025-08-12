import { inject, Inject } from "@angular/core";
import {CanActivateFn, Router} from "@angular/router";
import {AuthService} from "../services/auth.service";

export const canActivateAuth: CanActivateFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    try{
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
