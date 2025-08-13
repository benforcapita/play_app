import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection, signal } from '@angular/core';
import { App } from './app';
import { ApiService } from './core/services/api.service';
import { CharactersService } from './core/services/characters.services';
import { ExtractionService } from './core/services/extraction.service';

describe('App Integration Tests', () => {
  let fixture: any;
  let component: App;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        ApiService,
        CharactersService,
        {
          provide: ExtractionService,
          useValue: {
            activeJobs: signal([]),
            isPolling: signal(false),
            startExtraction: jasmine.createSpy('startExtraction'),
            checkJobStatus: jasmine.createSpy('checkJobStatus'),
            getJobResult: jasmine.createSpy('getJobResult'),
            removeJob: jasmine.createSpy('removeJob'),
            clearAllJobs: jasmine.createSpy('clearAllJobs')
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create the app component', () => {
      expect(component).toBeTruthy();
    });

    it('should have topbar', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.topbar')).toBeTruthy();
    });
  });

  describe('Router Integration', () => {
    it('should have router outlet', () => {
      const routerOutlet = fixture.debugElement.query(By.directive(RouterOutlet));
      expect(routerOutlet).toBeTruthy();
    });

    it('should render router outlet in template', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('router-outlet')).toBeTruthy();
    });
  });

  describe('Performance', () => {
    it('should render without errors', () => {
      expect(() => {
        fixture.detectChanges();
      }).not.toThrow();
    });

    it('should have reasonable render time', () => {
      const startTime = performance.now();
      fixture.detectChanges();
      const endTime = performance.now();
      
      // Should render in less than 100ms
      expect(endTime - startTime).toBeLessThan(100);
    });
  });
}); 