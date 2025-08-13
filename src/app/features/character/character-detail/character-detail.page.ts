import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CharactersService } from '../../../core/services/characters.services';
import { Character } from '../../../core/models/character.models';

@Component({
  selector: 'app-character-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
  <div class="wrap">
    <header>
      <a routerLink="/characters" class="ghost">← Back</a>
      <h1>Character Details</h1>
    </header>

    <div class="card" *ngIf="character(); else loadingOrError">
      <div class="title">{{ character()!.name }}</div>
      <div class="meta">Class: <b>{{ character()!.class }}</b></div>
      <div class="meta">Species: <b>{{ character()!.species }}</b></div>
      <div class="meta" *ngIf="character()!.level">Level: <b>{{ character()!.level }}</b></div>
    </div>

    <ng-template #loadingOrError>
      <div class="placeholder" *ngIf="!error(); else err">Loading…</div>
      <ng-template #err><p class="error">{{ error() }}</p></ng-template>
    </ng-template>
  </div>
  `,
  styles: [`
    .wrap { max-width: 720px; margin: 0 auto; padding: 24px; color: #e5e7eb; }
    header { display:flex; align-items:center; gap:12px; margin-bottom: 16px; }
    h1 { margin:0; font-size: 22px; }
    .ghost { padding:8px 12px; border-radius:10px; border:1px solid #2a3552; background:transparent; color:#cbd5e1; text-decoration:none; }
    .card { background:#0b1220; border:1px solid #1f2a44; padding:16px; border-radius:12px; }
    .title { font-size:18px; font-weight:600; margin-bottom:8px; }
    .meta { color:#94a3b8; margin: 4px 0; }
    .error { margin-top: 12px; color: #fca5a5; font-size: 13px; }
    .placeholder { background:#0b1220; border:1px dashed #2a3552; border-radius:12px; padding:16px; color:#94a3b8; }
  `]
})
export class CharacterDetailPage {
  character = signal<Character | null>(null);
  error = signal<string | null>(null);

  constructor(private route: ActivatedRoute, private svc: CharactersService) {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;
    if (!id || Number.isNaN(id)) {
      this.error.set('Invalid character id');
      return;
    }

    this.svc.get(id).subscribe({
      next: (c) => this.character.set(c),
      error: (err) => this.error.set(err?.message || 'Failed to load character')
    });
  }
}
