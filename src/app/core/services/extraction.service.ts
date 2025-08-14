import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, timer, switchMap, catchError, of, tap } from 'rxjs';
import { ExtractionJobResponse, ExtractionJobStatus, ExtractionResult } from '../models/character.models';

export interface ExtractionJob {
  jobToken: string;
  fileName: string;
  status: 'pending' | 'running' | 'completed' | 'failed';
  createdAt: Date;
  startedAt?: Date;
  completedAt?: Date;
  isSuccessful?: boolean;
  errorMessage?: string;
  sectionResults: any[];
  character?: any;
  progress?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ExtractionService {
  private http = inject(HttpClient);
  
  // Global state for all extraction jobs
  private _jobs = signal<ExtractionJob[]>([]);
  private _isPolling = signal(false);
  
  // Computed values
  jobs = this._jobs.asReadonly();
  isPolling = this._isPolling.asReadonly();
  
  // Get active jobs (not completed or failed)
  activeJobs = computed(() => 
    this._jobs().filter(job => job.status === 'pending' || job.status === 'running')
  );
  
  // Get completed jobs
  completedJobs = computed(() => 
    this._jobs().filter(job => job.status === 'completed' || job.status === 'failed')
  );

  constructor() {
    // Load jobs from localStorage on initialization
    this.loadJobsFromStorage();
  }

  // Start a new extraction job
  startExtraction(file: File): Observable<ExtractionJobResponse> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ExtractionJobResponse>('api/extract/characters', formData).pipe(
      tap(response => {
        // Add new job to state
        const newJob: ExtractionJob = {
          jobToken: response.jobToken,
          fileName: file.name,
          status: 'pending',
          createdAt: new Date(),
          sectionResults: []
        };
        
        this.addJob(newJob);
        this.startPolling();
      })
    );
  }

  // Add a job to the global state
  private addJob(job: ExtractionJob) {
    const currentJobs = this._jobs();
    this._jobs.set([...currentJobs, job]);
    this.saveJobsToStorage();
  }

  // Update job status
  private updateJob(jobToken: string, updates: Partial<ExtractionJob>) {
    const currentJobs = this._jobs();
    const updatedJobs = currentJobs.map(job => 
      job.jobToken === jobToken ? { ...job, ...updates } : job
    );
    this._jobs.set(updatedJobs);
    this.saveJobsToStorage();
  }

  // Remove a job
  removeJob(jobToken: string) {
    const currentJobs = this._jobs();
    const filteredJobs = currentJobs.filter(job => job.jobToken !== jobToken);
    this._jobs.set(filteredJobs);
    this.saveJobsToStorage();
  }

  // Get job by token
  getJob(jobToken: string): ExtractionJob | undefined {
    return this._jobs().find(job => job.jobToken === jobToken);
  }

  // Check status of a specific job
  checkJobStatus(jobToken: string): Observable<ExtractionJobStatus> {
    return this.http.get<ExtractionJobStatus>(`api/extract/jobs/${jobToken}/status`).pipe(
      tap(status => {
        this.updateJob(jobToken, {
          status: status.status,
          startedAt: status.startedAt ? new Date(status.startedAt) : undefined,
          completedAt: status.completedAt ? new Date(status.completedAt) : undefined,
          isSuccessful: status.isSuccessful,
          errorMessage: status.errorMessage,
          sectionResults: status.sectionResults,
          character: status.character
        });
      }),
      catchError(error => {
        this.updateJob(jobToken, {
          status: 'failed',
          errorMessage: error.message || 'Failed to check status'
        });
        return throwError(() => error);
      })
    );
  }

  // Get result of a completed job
  getJobResult(jobToken: string): Observable<ExtractionResult> {
    return this.http.get<ExtractionResult>(`api/extract/jobs/${jobToken}/result`).pipe(
      tap(result => {
        this.updateJob(jobToken, {
          character: result.character,
          status: 'completed',
          isSuccessful: result.jobSummary.isSuccessful
        });
      })
    );
  }

  // Start polling for active jobs
  private startPolling() {
    if (this._isPolling()) return;
    
    this._isPolling.set(true);
    
    // Poll every 2 seconds for active jobs
    timer(0, 2000).pipe(
      switchMap(() => {
        const activeJobs = this.activeJobs();
        if (activeJobs.length === 0) {
          this._isPolling.set(false);
          return of(null);
        }
        
        // Check status for all active jobs
        const statusChecks = activeJobs.map(job => 
          this.checkJobStatus(job.jobToken).pipe(
            catchError(() => of(null))
          )
        );
        
        return statusChecks.length > 0 ? statusChecks : of(null);
      })
    ).subscribe();
  }

  // Save jobs to localStorage
  private saveJobsToStorage() {
    if (typeof window !== 'undefined' && window.localStorage) {
      const jobsData = this._jobs().map(job => ({
        ...job,
        createdAt: job.createdAt.toISOString(),
        startedAt: job.startedAt?.toISOString(),
        completedAt: job.completedAt?.toISOString()
      }));
      localStorage.setItem('extraction_jobs', JSON.stringify(jobsData));
    }
  }

  // Load jobs from localStorage
  private loadJobsFromStorage() {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const jobsData = localStorage.getItem('extraction_jobs');
        if (jobsData) {
          const jobs = JSON.parse(jobsData).map((job: any) => ({
            ...job,
            createdAt: new Date(job.createdAt),
            startedAt: job.startedAt ? new Date(job.startedAt) : undefined,
            completedAt: job.completedAt ? new Date(job.completedAt) : undefined
          }));
          this._jobs.set(jobs);
          
          // Restart polling if there are active jobs
          if (this.activeJobs().length > 0) {
            this.startPolling();
          }
        }
      } catch (error) {
      }
    }
  }

  // Clear all jobs
  clearAllJobs() {
    this._jobs.set([]);
    this.saveJobsToStorage();
  }

  // Clear completed jobs only
  clearCompletedJobs() {
    const activeJobs = this.activeJobs();
    this._jobs.set(activeJobs);
    this.saveJobsToStorage();
  }
}

function throwError(errorFactory: () => any): Observable<never> {
  return new Observable(subscriber => {
    subscriber.error(errorFactory());
  });
}
