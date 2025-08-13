import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideZonelessChangeDetection, signal } from '@angular/core';
import { App } from './app';
import { ApiService } from './core/services/api.service';
import { CharactersService } from './core/services/characters.services';
import { ExtractionService } from './core/services/extraction.service';
import { of, throwError } from 'rxjs';

describe('App', () => {
  let mockApiService: jasmine.SpyObj<ApiService>;
  let mockCharactersService: jasmine.SpyObj<CharactersService>;
  let mockExtractionService: jasmine.SpyObj<ExtractionService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['getPing', 'getHealth']);
    const charactersSpy = jasmine.createSpyObj('CharactersService', [
      'list', 'get', 'create', 'update', 'delete', 'getSheet',
      'startExtraction', 'getExtractionStatus', 'getExtractionResult'
    ]);
    const extractionSpy = jasmine.createSpyObj('ExtractionService', [
      'startExtraction', 'checkJobStatus', 'getJobResult', 'removeJob', 'clearAllJobs'
    ], {
      activeJobs: signal([]),
      isPolling: signal(false)
    });
    
    await TestBed.configureTestingModule({
      imports: [App, RouterTestingModule],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ApiService, useValue: apiSpy },
        { provide: CharactersService, useValue: charactersSpy },
        { provide: ExtractionService, useValue: extractionSpy }
      ]
    }).compileComponents();

    mockApiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    mockCharactersService = TestBed.inject(CharactersService) as jasmine.SpyObj<CharactersService>;
    mockExtractionService = TestBed.inject(ExtractionService) as jasmine.SpyObj<ExtractionService>;
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should have router outlet', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const routerOutlet = fixture.debugElement.query(By.directive(RouterOutlet));
    expect(routerOutlet).toBeTruthy();
  });

  it('should have top bar', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    expect(compiled.querySelector('.topbar')).toBeTruthy();
  });

  it('should have job indicator component', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    expect(compiled.querySelector('app-job-indicator')).toBeTruthy();
  });
});
