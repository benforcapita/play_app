import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { App } from './app';
import { ApiService } from './core/services/api.service';
import { CharactersService } from './core/services/characters.services';
import { of, throwError } from 'rxjs';

describe('App', () => {
  let mockApiService: jasmine.SpyObj<ApiService>;
  let mockCharactersService: jasmine.SpyObj<CharactersService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['getPing', 'getHealth']);
    const charactersSpy = jasmine.createSpyObj('CharactersService', [
      'list', 'get', 'create', 'update', 'delete', 'getSheet',
      'startExtraction', 'getExtractionStatus', 'getExtractionResult'
    ]);
    
    await TestBed.configureTestingModule({
      imports: [App, RouterTestingModule],
      providers: [
        provideZonelessChangeDetection(),
        { provide: ApiService, useValue: apiSpy },
        { provide: CharactersService, useValue: charactersSpy }
      ]
    }).compileComponents();

    mockApiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    mockCharactersService = TestBed.inject(CharactersService) as jasmine.SpyObj<CharactersService>;
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

  // API Service Test Buttons
  describe('API Service Test Buttons', () => {
    it('should render API test container', () => {
      const fixture = TestBed.createComponent(App);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('.api-test-container')).toBeTruthy();
      expect(compiled.querySelector('.api-test-container h2')?.textContent).toContain('API Service Test');
    });

    it('should render both API test buttons', () => {
      const fixture = TestBed.createComponent(App);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      
      const pingButton = compiled.querySelector('.ping-button') as HTMLButtonElement;
      const healthButton = compiled.querySelector('.health-button') as HTMLButtonElement;
      
      expect(pingButton).toBeTruthy();
      expect(healthButton).toBeTruthy();
      expect(pingButton.textContent?.trim()).toBe('Test Ping API');
      expect(healthButton.textContent?.trim()).toBe('Test Health API');
    });

    it('should have proper button attributes', () => {
      const fixture = TestBed.createComponent(App);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      
      // Test only the main API test buttons (ping and health) which should never be disabled
      const pingButton = compiled.querySelector('.ping-button') as HTMLButtonElement;
      const healthButton = compiled.querySelector('.health-button') as HTMLButtonElement;
      
      [pingButton, healthButton].forEach(button => {
        expect(button.type).toBe('button');
        expect(button.disabled).toBe(false);
        expect(button.classList.contains('api-button')).toBe(true);
      });
    });

    it('should call checkPing when ping button is clicked', () => {
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      const pingButton = fixture.nativeElement.querySelector('.ping-button') as HTMLButtonElement;
      
      spyOn(app, 'checkPing');
      pingButton.click();
      
      expect(app.checkPing).toHaveBeenCalled();
    });

    it('should call checkHealth when health button is clicked', () => {
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      const healthButton = fixture.nativeElement.querySelector('.health-button') as HTMLButtonElement;
      
      spyOn(app, 'checkHealth');
      healthButton.click();
      
      expect(app.checkHealth).toHaveBeenCalled();
    });

    it('should display ping result when API call succeeds', () => {
      const mockPingResult = { message: 'pong', timestamp: '2023-01-01T00:00:00Z' };
      mockApiService.getPing.and.returnValue(of(mockPingResult));
      
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      app.checkPing();
      fixture.detectChanges();
      
      const resultDisplay = fixture.nativeElement.querySelector('.result-display');
      expect(resultDisplay).toBeTruthy();
      expect(resultDisplay.textContent).toContain('Ping Result:');
      expect(resultDisplay.textContent).toContain('pong');
    });

    it('should display health result when API call succeeds', () => {
      const mockHealthResult = 'Service is healthy';
      mockApiService.getHealth.and.returnValue(of(mockHealthResult));
      
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      app.checkHealth();
      fixture.detectChanges();
      
      const resultDisplay = fixture.nativeElement.querySelector('.result-display');
      expect(resultDisplay).toBeTruthy();
      expect(resultDisplay.textContent).toContain('Health Result:');
      expect(resultDisplay.textContent).toContain('Service is healthy');
    });

    it('should display error message when API call fails', () => {
      const mockError = new Error('Network error');
      mockApiService.getPing.and.returnValue(throwError(() => mockError));
      
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      app.checkPing();
      fixture.detectChanges();
      
      const errorMessage = fixture.nativeElement.querySelector('.error-message');
      expect(errorMessage).toBeTruthy();
      expect(errorMessage.textContent).toContain('Error:');
      expect(errorMessage.textContent).toContain('Network error');
    });

    it('should clear previous results when making new API calls', () => {
      const mockPingResult = { message: 'pong' };
      const mockHealthResult = 'Service is healthy';
      
      mockApiService.getPing.and.returnValue(of(mockPingResult));
      mockApiService.getHealth.and.returnValue(of(mockHealthResult));
      
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      // First call ping
      app.checkPing();
      fixture.detectChanges();
      
      let resultDisplays = fixture.nativeElement.querySelectorAll('.result-display');
      expect(resultDisplays.length).toBe(1);
      expect(resultDisplays[0].textContent).toContain('Ping Result:');
      
      // Then call health
      app.checkHealth();
      fixture.detectChanges();
      
      resultDisplays = fixture.nativeElement.querySelectorAll('.result-display');
      expect(resultDisplays.length).toBe(1);
      expect(resultDisplays[0].textContent).toContain('Health Result:');
    });

    it('should have proper CSS classes for API test elements', () => {
      const fixture = TestBed.createComponent(App);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('.api-test-container')).toBeTruthy();
      expect(compiled.querySelector('.button-container')).toBeTruthy();
      expect(compiled.querySelector('.results-container')).toBeTruthy();
      expect(compiled.querySelector('.ping-button')).toBeTruthy();
      expect(compiled.querySelector('.health-button')).toBeTruthy();
    });

    it('should have proper signal values for API state management', () => {
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      
      expect(app.result()).toBeNull();
      expect(app.healthText()).toBeNull();
      expect(app.error()).toBeNull();
    });

    it('should update signals when API calls are made', () => {
      const mockPingResult = { message: 'pong' };
      mockApiService.getPing.and.returnValue(of(mockPingResult));
      
      const fixture = TestBed.createComponent(App);
      const app = fixture.componentInstance;
      fixture.detectChanges();
      
      app.checkPing();
      fixture.detectChanges();
      
      expect(app.result()).toEqual(mockPingResult);
      expect(app.error()).toBeNull();
    });
  });
});
