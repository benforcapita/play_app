import { Routes } from '@angular/router';
import { canActivateAuth} from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.page').then(m => m.LoginPage)
  },
  {
    path: 'characters',
    canActivate: [canActivateAuth],
    loadComponent: () => import('./features/character/character-list/character-list.page').then(m => m.CharacterListPage)
  },
  {
    path: 'characters/:id',
    canActivate: [canActivateAuth],
    loadComponent: () => import('./features/character/character-detail/character-detail.page').then(m => m.CharacterDetailPage)
  },
  {
    path: 'extract',
    canActivate: [canActivateAuth],
    loadComponent: () => import('./features/extract/extract.page').then(m => m.ExtractPage)
  },
  { path: '', redirectTo: 'characters', pathMatch: 'full' },
  { path: '**', redirectTo: 'characters' }
];
