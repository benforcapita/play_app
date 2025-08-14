import { Component, signal, computed, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ExtractionService, ExtractionJob } from '../../core/services/extraction.service';

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

      <!-- Upload Section -->
      <div class="card">
        <label class="drop" for="fileInput" [class.disabled]="isUploading()">
          <input 
            id="fileInput" 
            type="file" 
            (change)="onFileChange($event)" 
            [disabled]="isUploading()"
            hidden 
          />
          <div *ngIf="!fileName(); else chosen">
            <div class="upload-icon">📄</div>
            <div>Click to choose a file</div>
          </div>
          <ng-template #chosen>
            <div class="selected-file">
              <div class="file-icon">📄</div>
              <div class="file-info">
                <div class="file-name">{{ fileName() }}</div>
                <div class="file-size">{{ fileSize() }}</div>
              </div>
            </div>
          </ng-template>
        </label>

        <div class="hint">
          Supported: PNG, JPG, WEBP, GIF, PDF
        </div>

        <!-- Upload Button -->
        <div class="upload-actions" *ngIf="fileName() && !isUploading()">
          <button class="btn btn-primary" (click)="uploadFile()">
            Start Extraction
          </button>
          <button class="btn btn-ghost" (click)="clearFile()">
            Clear
          </button>
        </div>

        <!-- Upload Progress -->
        <div class="upload-progress" *ngIf="isUploading()">
          <div class="progress-bar">
            <div class="progress-fill" [style.width.%]="uploadProgress()"></div>
          </div>
          <div class="progress-text">Uploading... {{ uploadProgress() }}%</div>
        </div>
      </div>

      <!-- Active Jobs Section -->
      <div class="jobs-section" *ngIf="activeJobs().length > 0">
        <h2>Active Extractions</h2>
        <div class="job-list">
          <div class="job-card" *ngFor="let job of activeJobs()">
            <div class="job-header">
              <div class="job-info">
                <div class="job-name">{{ job.fileName }}</div>
                <div class="job-status" [class]="job.status">
                  {{ getStatusText(job.status) }}
                </div>
              </div>
              <div class="job-time">{{ getTimeAgo(job.createdAt) }}</div>
            </div>
            
            <div class="job-progress" *ngIf="job.status === 'running'">
              <div class="progress-bar">
                <div class="progress-fill running" [style.width.%]="getJobProgress(job)"></div>
              </div>
              <div class="progress-text">Processing... {{ getJobProgress(job) }}%</div>
            </div>

            <div class="job-actions">
              <button class="btn btn-ghost btn-sm" (click)="removeJob(job.jobToken)">
                Cancel
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Completed Jobs Section -->
      <div class="jobs-section" *ngIf="completedJobs().length > 0">
        <div class="section-header">
          <h2>Completed Extractions</h2>
          <button class="btn btn-ghost btn-sm" (click)="clearCompletedJobs()">
            Clear All
          </button>
        </div>
        <div class="job-list">
          <div class="job-card completed" *ngFor="let job of completedJobs()">
            <div class="job-header">
              <div class="job-info">
                <div class="job-name">{{ job.fileName }}</div>
                <div class="job-status" [class]="job.status">
                  {{ getStatusText(job.status) }}
                </div>
              </div>
              <div class="job-time">{{ getTimeAgo(job.completedAt || job.createdAt) }}</div>
            </div>
            
            <div class="job-result" *ngIf="job.status === 'completed' && job.character">
              <div class="character-preview">
                <div class="character-name">{{ job.character.name }}</div>
                <div class="character-details">
                  Level {{ job.character.level }} {{ job.character.class }} {{ job.character.species }}
                </div>
              </div>
              <div class="job-actions">
                <button class="btn btn-primary btn-sm" (click)="viewCharacter(job.character.id)">
                  View Character
                </button>
                <button class="btn btn-ghost btn-sm" (click)="removeJob(job.jobToken)">
                  Remove
                </button>
              </div>
            </div>

            <div class="job-error" *ngIf="job.status === 'failed'">
              <div class="error-message">{{ job.errorMessage || 'Extraction failed' }}</div>
              <div class="job-actions">
                <button class="btn btn-ghost btn-sm" (click)="removeJob(job.jobToken)">
                  Remove
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Add New Job Button -->
      <div class="add-job-section" *ngIf="completedJobs().length > 0 || activeJobs().length > 0">
        <button class="add-job-btn" (click)="addNewJob()">
          <div class="plus-icon">+</div>
          <div>Extract Another Character</div>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .wrap { 
      max-width: 720px; 
      margin: 0 auto; 
      padding: 24px; 
      color: #e5e7eb; 
    }
    
    header { 
      margin-bottom: 24px; 
    }
    
    h1 { 
      margin: 0 0 6px; 
      font-size: 22px; 
    }
    
    .sub { 
      color: #94a3b8; 
      margin: 0; 
    }
    
    .card { 
      background: #0b1220; 
      border: 1px solid #1f2a44; 
      padding: 24px; 
      border-radius: 12px; 
      margin-bottom: 24px;
    }
    
    .drop { 
      display: grid; 
      place-items: center; 
      padding: 32px; 
      border: 1px dashed #2a3552; 
      border-radius: 12px; 
      cursor: pointer; 
      color: #cbd5e1; 
      transition: all 0.2s ease;
    }
    
    .drop:hover:not(.disabled) { 
      border-color: #3b82f6; 
      background: rgba(59, 130, 246, 0.05);
    }
    
    .drop.disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    
    .upload-icon {
      font-size: 32px;
      margin-bottom: 8px;
    }
    
    .selected-file {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    
    .file-icon {
      font-size: 24px;
    }
    
    .file-info {
      text-align: left;
    }
    
    .file-name {
      font-weight: 500;
      margin-bottom: 4px;
    }
    
    .file-size {
      font-size: 12px;
      color: #94a3b8;
    }
    
    .hint { 
      color: #94a3b8; 
      font-size: 12px; 
      margin-top: 12px; 
      text-align: center;
    }
    
    .upload-actions {
      display: flex;
      gap: 12px;
      margin-top: 16px;
      justify-content: center;
    }
    
    .upload-progress {
      margin-top: 16px;
    }
    
    .progress-bar {
      width: 100%;
      height: 8px;
      background: #1f2a44;
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 8px;
    }
    
    .progress-fill {
      height: 100%;
      background: #3b82f6;
      transition: width 0.3s ease;
    }
    
    .progress-fill.running {
      background: linear-gradient(90deg, #3b82f6, #8b5cf6);
      animation: pulse 2s infinite;
    }
    
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.7; }
    }
    
    .progress-text {
      font-size: 12px;
      color: #94a3b8;
      text-align: center;
    }
    
    .jobs-section {
      margin-bottom: 24px;
    }
    
    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }
    
    h2 {
      margin: 0;
      font-size: 18px;
      color: #e5e7eb;
    }
    
    .job-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    
    .job-card {
      background: #0b1220;
      border: 1px solid #1f2a44;
      border-radius: 12px;
      padding: 16px;
      transition: all 0.2s ease;
    }
    
    .job-card:hover {
      border-color: #2a3552;
    }
    
    .job-card.completed {
      border-color: #059669;
      background: rgba(5, 150, 105, 0.05);
    }
    
    .job-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 12px;
    }
    
    .job-info {
      flex: 1;
    }
    
    .job-name {
      font-weight: 500;
      margin-bottom: 4px;
    }
    
    .job-status {
      font-size: 12px;
      padding: 2px 8px;
      border-radius: 12px;
      display: inline-block;
    }
    
    .job-status.pending {
      background: #f59e0b;
      color: #000;
    }
    
    .job-status.running {
      background: #3b82f6;
      color: #fff;
    }
    
    .job-status.completed {
      background: #059669;
      color: #fff;
    }
    
    .job-status.failed {
      background: #dc2626;
      color: #fff;
    }
    
    .job-time {
      font-size: 12px;
      color: #94a3b8;
    }
    
    .job-progress {
      margin-bottom: 12px;
    }
    
    .job-result {
      margin-top: 12px;
    }
    
    .character-preview {
      background: rgba(59, 130, 246, 0.1);
      border: 1px solid rgba(59, 130, 246, 0.2);
      border-radius: 8px;
      padding: 12px;
      margin-bottom: 12px;
    }
    
    .character-name {
      font-weight: 500;
      margin-bottom: 4px;
    }
    
    .character-details {
      font-size: 12px;
      color: #94a3b8;
    }
    
    .job-error {
      margin-top: 12px;
    }
    
    .error-message {
      color: #f87171;
      font-size: 14px;
      margin-bottom: 12px;
    }
    
    .job-actions {
      display: flex;
      gap: 8px;
      justify-content: flex-end;
    }
    
    .add-job-section {
      text-align: center;
      margin-top: 24px;
    }
    
    .add-job-btn {
      display: flex;
      align-items: center;
      gap: 8px;
      background: transparent;
      border: 1px dashed #2a3552;
      color: #94a3b8;
      padding: 16px 24px;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.2s ease;
      width: 100%;
      justify-content: center;
    }
    
    .add-job-btn:hover {
      border-color: #3b82f6;
      color: #3b82f6;
      background: rgba(59, 130, 246, 0.05);
    }
    
    .plus-icon {
      font-size: 20px;
      font-weight: bold;
    }
    
    .btn {
      padding: 8px 16px;
      border-radius: 8px;
      border: none;
      cursor: pointer;
      font-size: 14px;
      transition: all 0.2s ease;
    }
    
    .btn-primary {
      background: #3b82f6;
      color: white;
    }
    
    .btn-primary:hover {
      background: #2563eb;
    }
    
    .btn-ghost {
      background: transparent;
      color: #94a3b8;
      border: 1px solid #2a3552;
    }
    
    .btn-ghost:hover {
      background: #1f2a44;
      color: #e5e7eb;
    }
    
    .btn-sm {
      padding: 6px 12px;
      font-size: 12px;
    }
  `]
})
export class ExtractPage implements OnDestroy {
  private extractionService = inject(ExtractionService);
  private router = inject(Router);
  
  // File upload state
  fileName = signal<string | null>(null);
  fileSize = signal<string>('');
  selectedFile = signal<File | null>(null);
  isUploading = signal(false);
  uploadProgress = signal(0);
  
  // Job state from service
  activeJobs = this.extractionService.activeJobs;
  completedJobs = this.extractionService.completedJobs;
  isPolling = this.extractionService.isPolling;

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    
    if (file) {
      this.selectedFile.set(file);
      this.fileName.set(file.name);
      this.fileSize.set(this.formatFileSize(file.size));
    } else {
      this.clearFile();
    }
  }

  uploadFile() {
    const file = this.selectedFile();
    if (!file) return;

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    // Simulate upload progress
    const progressInterval = setInterval(() => {
      const current = this.uploadProgress();
      if (current < 90) {
        this.uploadProgress.set(current + 10);
      }
    }, 100);

    this.extractionService.startExtraction(file).subscribe({
      next: (response) => {
        clearInterval(progressInterval);
        this.uploadProgress.set(100);
        
        setTimeout(() => {
          this.clearFile();
          this.isUploading.set(false);
          this.uploadProgress.set(0);
        }, 500);
      },
      error: () => {
        clearInterval(progressInterval);
        this.isUploading.set(false);
        this.uploadProgress.set(0);
      }
    });
  }

  clearFile() {
    this.selectedFile.set(null);
    this.fileName.set(null);
    this.fileSize.set('');
    
    // Reset file input
    const input = document.getElementById('fileInput') as HTMLInputElement;
    if (input) {
      input.value = '';
    }
  }

  addNewJob() {
    this.clearFile();
    // Focus on file input
    const input = document.getElementById('fileInput') as HTMLInputElement;
    if (input) {
      input.click();
    }
  }

  removeJob(jobToken: string) {
    this.extractionService.removeJob(jobToken);
  }

  clearCompletedJobs() {
    this.extractionService.clearCompletedJobs();
  }

  viewCharacter(characterId: number) {
    this.router.navigate(['/characters', characterId]);
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'pending': return 'Queued';
      case 'running': return 'Processing';
      case 'completed': return 'Completed';
      case 'failed': return 'Failed';
      default: return status;
    }
  }

  getTimeAgo(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    return `${diffDays}d ago`;
  }

  getJobProgress(job: ExtractionJob): number {
    if (job.status === 'pending') return 0;
    if (job.status === 'running') return 50; // You could calculate this based on section results
    if (job.status === 'completed') return 100;
    if (job.status === 'failed') return 0;
    return 0;
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  ngOnDestroy() {
    // Cleanup if needed
  }
}
