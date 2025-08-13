import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="page">
    <div class="card">
      <h1>Welcome back</h1>
      <p class="sub">Sign in to continue</p>

      <form (ngSubmit)="onSubmit()" #form="ngForm" novalidate>
        <label>Email</label>
        <input
          name="email"
          type="email"
          required
          [(ngModel)]="email"
          placeholder="you@example.com"
          [disabled]="isLoading()"
        />

        <label>Password</label>
        <input
          name="password"
          type="password"
          required
          minlength="6"
          [(ngModel)]="password"
          placeholder="••••••••"
          [disabled]="isLoading()"
        />

        <button class="primary" type="submit" [disabled]="isLoading() || !form.form.valid">
          {{ isLoading() ? 'Signing in…' : 'Sign in' }}
        </button>

        <p class="error" *ngIf="error()">{{ error() }}</p>
      </form>
    </div>
  </div>
  `,
  styles: [`
    .page { display: grid; place-items: center; min-height: 100dvh; background: linear-gradient(135deg,#0f172a,#111827); padding: 16px; }
    .card { width: 100%; max-width: 380px; background: #0b1220; border: 1px solid #1f2a44; border-radius: 16px; padding: 24px; color: #e5e7eb; box-shadow: 0 10px 30px rgba(0,0,0,0.35); }
    h1 { margin: 0 0 4px; font-size: 24px; font-weight: 700; letter-spacing: 0.2px; }
    .sub { margin: 0 0 20px; color: #94a3b8; font-size: 13px; }
    label { display:block; font-size: 12px; color:#a7b0c3; margin: 12px 0 6px; }
    input { width: 100%; padding: 12px 14px; border-radius: 10px; background: #0a1220; border: 1px solid #21314f; color: #e5e7eb; outline: none; transition: border-color .2s, box-shadow .2s; }
    input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,.15); }
    .primary { margin-top: 18px; width: 100%; padding: 12px 14px; border-radius: 10px; border: none; background: linear-gradient(135deg,#2563eb,#7c3aed); color: white; font-weight: 600; cursor: pointer; transition: transform .06s ease; }
    .primary:disabled { opacity: .6; cursor: not-allowed; }
    .primary:active { transform: translateY(1px); }
    .error { margin-top: 12px; color: #fca5a5; font-size: 13px; }
  `]
})
export class LoginPage {
  email = '';
  password = '';
  isLoading = signal(false);
  error = signal<string | null>(null);

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
    if (!this.email || !this.password) return;
    this.error.set(null);
    this.isLoading.set(true);
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/characters']),
      error: (err: any) => {
        this.error.set(err?.error?.message || err?.message || 'Login failed');
        this.isLoading.set(false);
      }
    });
  }
}
