import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ExtractionService } from '../services/extraction.service';

@Component({
  selector: 'app-job-indicator',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div 
      class="job-indicator" 
      *ngIf="hasActiveJobs()"
      (click)="goToExtract()"
      [class.pulsing]="isPolling()"
      title="Active extraction jobs"
    >
      <div class="indicator-icon">🔄</div>
      <div class="indicator-count" *ngIf="activeJobCount() > 1">
        {{ activeJobCount() }}
      </div>
    </div>
  `,
  styles: [`
    .job-indicator {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      background: #3b82f6;
      border-radius: 50%;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    
    .job-indicator:hover {
      background: #2563eb;
      transform: scale(1.05);
    }
    
    .job-indicator.pulsing {
      animation: pulse 2s infinite;
    }
    
    @keyframes pulse {
      0%, 100% { 
        box-shadow: 0 0 0 0 rgba(59, 130, 246, 0.7);
      }
      50% { 
        box-shadow: 0 0 0 8px rgba(59, 130, 246, 0);
      }
    }
    
    .indicator-icon {
      font-size: 16px;
      color: white;
    }
    
    .indicator-count {
      position: absolute;
      top: -4px;
      right: -4px;
      background: #dc2626;
      color: white;
      font-size: 10px;
      font-weight: bold;
      min-width: 16px;
      height: 16px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0 4px;
    }
  `]
})
export class JobIndicatorComponent {
  private extractionService = inject(ExtractionService);
  private router = inject(Router);
  
  activeJobs = this.extractionService.activeJobs;
  isPolling = this.extractionService.isPolling;
  
  hasActiveJobs = computed(() => this.activeJobs().length > 0);
  activeJobCount = computed(() => this.activeJobs().length);
  
  goToExtract() {
    this.router.navigate(['/extract']);
  }
}
