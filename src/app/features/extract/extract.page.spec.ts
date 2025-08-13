import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ExtractPage } from './extract.page';
import { ExtractionService } from '../../core/services/extraction.service';

function makeFile(name: string): File {
  return new File(['content'], name, { type: 'image/png' });
}

describe('ExtractPage', () => {
  let mockExtractionService: jasmine.SpyObj<ExtractionService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const extractionSpy = jasmine.createSpyObj('ExtractionService', ['startExtraction', 'removeJob', 'clearCompletedJobs'], {
      activeJobs: signal([]),
      isPolling: signal(false),
      jobs: signal([]),
      completedJobs: signal([])
    });
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [ExtractPage],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ExtractionService, useValue: extractionSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    mockExtractionService = TestBed.inject(ExtractionService) as jasmine.SpyObj<ExtractionService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(ExtractPage);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should update fileName on input change', () => {
    const fixture = TestBed.createComponent(ExtractPage);
    const comp = fixture.componentInstance;
    
    // Initial state check
    expect(comp.fileName()).toBeNull();
    
    fixture.detectChanges();

    // Simulate a file input change with a plain object carrying a FileList-like structure
    const file = makeFile('sheet.png');
    const event = { target: { files: [file] } } as any;

    // Call the method directly
    comp.onFileChange(event);
    
    // In zoneless mode, signal changes should be immediate
    expect(comp.fileName()).toBe('sheet.png');
    
    // Also trigger change detection to ensure template updates
    fixture.detectChanges();
    
    // Verify the signal value is still correct
    expect(comp.fileName()).toBe('sheet.png');
  });
});
