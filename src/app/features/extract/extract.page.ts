import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-extract-page',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="wrap">
    <header>
      <h1>Extract Character</h1>
      <p class="sub">Upload a sheet image or PDF. We will process it shortly.</p>
    </header>

    <div class="card">
      <label class="drop" for="fileInput">
        <input id="fileInput" type="file" (change)="onFileChange($event)" hidden />
        <div *ngIf="!fileName(); else chosen">Click to choose a file</div>
        <ng-template #chosen>
          <div>Selected: <b>{{ fileName() }}</b></div>
        </ng-template>
      </label>

      <div class="hint">
        Supported: PNG, JPG, WEBP, GIF, PDF
      </div>
    </div>
  </div>
  `,
  styles: [`
    .wrap { max-width: 720px; margin: 0 auto; padding: 24px; color:#e5e7eb; }
    header { margin-bottom: 12px; }
    h1 { margin: 0 0 6px; font-size: 22px; }
    .sub { color:#94a3b8; margin:0; }
    .card { background:#0b1220; border:1px solid #1f2a44; padding:16px; border-radius:12px; }
    .drop { display:grid; place-items:center; padding:20px; border:1px dashed #2a3552; border-radius:12px; cursor:pointer; color:#cbd5e1; }
    .drop:hover { border-color:#3b82f6; }
    .hint { color:#94a3b8; font-size:12px; margin-top:10px; }
  `]
})
export class ExtractPage {
  fileName = signal<string | null>(null);

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.fileName.set(file ? file.name : null);
  }
}