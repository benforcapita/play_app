import { Component, computed, signal } from '@angular/core';
import { CommonModule, DatePipe, NgIf, NgFor } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CharactersService } from '../../../core/services/characters.services';
import { Character } from '../../../core/models/character.models';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-character-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIf, NgFor, DatePipe],
  template: `
  <div class="wrap">
    <header>
      <h1>Characters</h1>
      <button class="ghost" (click)="refresh()" [disabled]="loading()">{{ loading() ? 'Refreshing…' : 'Refresh' }}</button>
      <button class="ghost" (click)="goExtract()">Extract</button>
      <button class="ghost" (click)="logout()">Logout</button>
    </header>

    <div class="grid" *ngIf="characters().length; else empty">
      <a class="card" *ngFor="let c of characters()" [routerLink]="['/characters', c.id]">
        <div class="title">{{ c.name }}</div>
        <div class="meta">{{ c.class }} • {{ c.species }} <span *ngIf="c.level">• L{{ c.level }}</span></div>
      </a>
    </div>

    <ng-template #empty>
      <div class="empty">
        <p>No characters found.</p>
        <button class="primary" (click)="refresh()">Try again</button>
      </div>
    </ng-template>

    <p class="error" *ngIf="error()">{{ error() }}</p>
  </div>
  `,
  styles: [`
    .wrap { max-width: 900px; margin: 0 auto; padding: 24px; color: #e5e7eb; }
    header { display:flex; align-items:center; gap:8px; margin-bottom: 16px; }
    h1 { flex:1; margin:0; font-size: 22px; }
    .grid { display:grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 12px; }
    .card { display:block; text-decoration:none; background:#0b1220; border:1px solid #1f2a44; padding:14px; border-radius:12px; color:inherit; transition: border-color .2s, transform .06s; }
    .card:hover { border-color:#3b82f6; }
    .card:active { transform: translateY(1px); }
    .title { font-weight:600; margin-bottom:4px; }
    .meta { color:#94a3b8; font-size: 12px; }
    .ghost { padding:8px 12px; border-radius:10px; border:1px solid #2a3552; background:transparent; color:#cbd5e1; cursor:pointer; }
    .primary { padding:10px 14px; border-radius:10px; border:none; background:linear-gradient(135deg,#2563eb,#7c3aed); color:white; cursor:pointer; }
    .empty { display:grid; place-items:center; padding:32px; background:#0b1220; border:1px dashed #2a3552; border-radius:12px; }
    .error { margin-top: 12px; color: #fca5a5; font-size: 13px; }
  `]
})
export class CharacterListPage {
  characters = signal<Character[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private svc: CharactersService, private router: Router, private auth: AuthService) {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.error.set(null);
    this.svc.list().subscribe({
      next: (list) => {
        this.characters.set(list || []);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.message || 'Failed to load');
        this.loading.set(false);
      }
    });
  }

  goExtract() { this.router.navigate(['/extracts']); }
  logout() { this.auth.logout(); this.router.navigate(['/login']); }
}
