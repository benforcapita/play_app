import { Routes } from '@angular/router';
import { canActivateAuth} from './core/guards/auth.guard';
import {LoginPage} from './features/auth/login/login.page'
import {CharacterListPage} from './features/character/character-list/character-list.page'
import {CharacterDetailPage} from './features/character/character-detail/character-detail.page'
import {ExtractPage} from './features/extract/extract.page'
//TODO ADD LOGIN PAGE AND CHARACTER LIST PAGE AND CHARACTER DETAIL PAGE AND EXTRACT PAGE 

export const routes: Routes = [
    {path:'login', component: LoginPage},
    {path:'characters', component: CharacterListPage},
    {path:'characters/:id', component: CharacterDetailPage},
    {path:'extracts', component: ExtractPage},
    {path:'**', redirectTo: 'characters'},
    {path:'', redirectTo: 'characters', pathMatch: 'full'}
];
