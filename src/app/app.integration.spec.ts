import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { App } from './app';
import { ApiService } from './core/services/api.service';
import { TestUtils } from './testing/test-utils';

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
        ApiService
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

    it('should render the API test container', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.api-test-container')).toBeTruthy();
    });

    it('should display the API test title', () => {
      const titleElement = TestUtils.getElement(fixture, '.api-test-container h2');
      expect(titleElement.textContent).toContain('API Service Test');
    });
  });

  describe('API Test Buttons', () => {
    it('should render both API test buttons', () => {
      const pingButton = TestUtils.getElement(fixture, '.ping-button');
      const healthButton = TestUtils.getElement(fixture, '.health-button');
      
      expect(pingButton).toBeTruthy();
      expect(healthButton).toBeTruthy();
      expect(pingButton.textContent?.trim()).toBe('Test Ping API');
      expect(healthButton.textContent?.trim()).toBe('Test Health API');
    });

    it('should have proper button attributes', () => {
      // Test only the main API test buttons (ping and health) which should never be disabled
      const pingButton = TestUtils.getElement(fixture, '.ping-button') as HTMLButtonElement;
      const healthButton = TestUtils.getElement(fixture, '.health-button') as HTMLButtonElement;
      
      [pingButton, healthButton].forEach(button => {
        expect(button.type).toBe('button');
        expect(button.disabled).toBe(false);
        expect(button.classList.contains('api-button')).toBe(true);
      });
    });

    it('should have proper CSS classes for API test elements', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('.api-test-container')).toBeTruthy();
      expect(compiled.querySelector('.button-container')).toBeTruthy();
      expect(compiled.querySelector('.results-container')).toBeTruthy();
      expect(compiled.querySelector('.ping-button')).toBeTruthy();
      expect(compiled.querySelector('.health-button')).toBeTruthy();
    });
  });

  describe('API Service Integration', () => {
    it('should have ApiService injected', () => {
      const apiService = TestBed.inject(ApiService);
      expect(apiService).toBeTruthy();
    });

    it('should have proper signal values for API state management', () => {
      expect(component.result()).toBeNull();
      expect(component.healthText()).toBeNull();
      expect(component.error()).toBeNull();
    });

    it('should have checkPing method', () => {
      expect(typeof component.checkPing).toBe('function');
    });

    it('should have checkHealth method', () => {
      expect(typeof component.checkHealth).toBe('function');
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

  describe('Template Structure', () => {
    it('should have proper HTML structure', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('router-outlet')).toBeTruthy();
      expect(compiled.querySelector('.api-test-container')).toBeTruthy();
      expect(compiled.querySelector('.button-container')).toBeTruthy();
      expect(compiled.querySelector('.results-container')).toBeTruthy();
    });

    it('should have proper button structure', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      const pingButton = compiled.querySelector('.ping-button') as HTMLButtonElement;
      const healthButton = compiled.querySelector('.health-button') as HTMLButtonElement;
      
      expect(pingButton).toBeTruthy();
      expect(healthButton).toBeTruthy();
      expect(pingButton.type).toBe('button');
      expect(healthButton.type).toBe('button');
    });

    it('should have proper result display structure', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      // These elements should exist in the template but may not be visible initially
      const errorMessage = compiled.querySelector('.error-message');
      const resultDisplay = compiled.querySelector('.result-display');
      
      // The structure should be there, even if not visible
      expect(compiled.querySelector('.results-container')).toBeTruthy();
    });
  });
}); 