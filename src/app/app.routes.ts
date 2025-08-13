import { Routes } from '@angular/router';
import { canActivateAuth} from './core/guards/auth.guard';
import {LoginPage} from './features/auth/login/login.page'
import {CharacterListPage} from './features/character/character-list/character-list.page'
import {CharacterDetailPage} from './features/character/character-detail/character-detail.page'
import {ExtractPage} from './features/extract/extract.page'

export const routes: Routes = [

    { path: 'login', component: LoginPage },
    { path: 'characters', component: CharacterListPage, canActivate: [canActivateAuth] },
    { path: 'characters/:id', component: CharacterDetailPage, canActivate: [canActivateAuth] },
    { path: 'extract', component: ExtractPage, canActivate: [canActivateAuth] },
    { path: '', redirectTo: 'characters', pathMatch: 'full' },
    { path: '**', redirectTo: 'characters' }

];
