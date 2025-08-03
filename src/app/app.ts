import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgIf, JsonPipe } from '@angular/common';
import { ApiService } from './core/services/api.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgIf, JsonPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('play-app');

  // Display state for API calls
  result = signal<any | null>(null);
  healthText = signal<string | null>(null);
  error = signal<string | null>(null);

  constructor(private api: ApiService) {}

  checkPing() {
    this.error.set(null);
    this.healthText.set(null);
    this.result.set(null);

    this.api.getPing().subscribe({
      next: (r: any) => this.result.set(r),
      error: (err: any) => this.error.set(err?.message ?? 'Request failed')
    });
  }

  checkHealth() {
    this.error.set(null);
    this.result.set(null);
    this.healthText.set(null);

    this.api.getHealth().subscribe({
      next: (text: any) => this.healthText.set(typeof text === 'string' ? text : JSON.stringify(text)),
      error: (err: any) => this.error.set(err?.message ?? 'Request failed')
    });
  }
}
