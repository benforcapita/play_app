import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App, RouterTestingModule],
      providers: [provideZonelessChangeDetection()]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Hello, play-app');
  });

  it('should have the correct title signal value', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('play-app');
  });

  it('should render the Angular logo', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const logo = compiled.querySelector('.angular-logo');
    expect(logo).toBeTruthy();
  });

  it('should render the congratulations message', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const message = compiled.querySelector('p');
    expect(message?.textContent).toContain('Congratulations! Your app is running. 🎉');
  });

  it('should render all navigation pills', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const pills = compiled.querySelectorAll('.pill');
    expect(pills.length).toBe(6);
  });

  it('should have correct pill titles', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const pills = compiled.querySelectorAll('.pill');
    
    const expectedTitles = [
      'Explore the Docs',
      'Learn with Tutorials',
      'Prompt and best practices for AI',
      'CLI Docs',
      'Angular Language Service',
      'Angular DevTools'
    ];

    pills.forEach((pill, index) => {
      expect(pill.textContent?.trim()).toContain(expectedTitles[index]);
    });
  });

  it('should have correct pill links', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const pills = compiled.querySelectorAll('.pill') as NodeListOf<HTMLAnchorElement>;
    
    const expectedLinks = [
      'https://angular.dev',
      'https://angular.dev/tutorials',
      'https://angular.dev/ai/develop-with-ai',
      'https://angular.dev/tools/cli',
      'https://angular.dev/tools/language-service',
      'https://angular.dev/tools/devtools'
    ];

          pills.forEach((pill, index) => {
        // Only the base URL gets a trailing slash added by the browser
        const expectedUrl = expectedLinks[index] === 'https://angular.dev' 
          ? expectedLinks[index] + '/' 
          : expectedLinks[index];
        expect(pill.href).toBe(expectedUrl);
      });
  });

  it('should have proper target and rel attributes on pills', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const pills = compiled.querySelectorAll('.pill') as NodeListOf<HTMLAnchorElement>;
    
    pills.forEach(pill => {
      expect(pill.target).toBe('_blank');
      expect(pill.rel).toBe('noopener');
    });
  });

  it('should render social media links', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const socialLinks = compiled.querySelectorAll('.social-links a');
    expect(socialLinks.length).toBe(3);
  });

  it('should have correct social media URLs', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const socialLinks = compiled.querySelectorAll('.social-links a') as NodeListOf<HTMLAnchorElement>;
    
    const expectedUrls = [
      'https://github.com/angular/angular',
      'https://twitter.com/angular',
      'https://www.youtube.com/channel/UCbn1OgGei-DV7aSRo_HaAiw'
    ];

    socialLinks.forEach((link, index) => {
      expect(link.href).toBe(expectedUrls[index]);
    });
  });

  it('should have proper accessibility attributes on social links', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const socialLinks = compiled.querySelectorAll('.social-links a') as NodeListOf<HTMLAnchorElement>;
    
    socialLinks.forEach(link => {
      expect(link.target).toBe('_blank');
      expect(link.rel).toBe('noopener');
      expect(link.getAttribute('aria-label')).toBeTruthy();
    });
  });

  it('should have router outlet', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const routerOutlet = fixture.debugElement.query(By.directive(RouterOutlet));
    expect(routerOutlet).toBeTruthy();
  });

  it('should have proper CSS classes for styling', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    expect(compiled.querySelector('.main')).toBeTruthy();
    expect(compiled.querySelector('.content')).toBeTruthy();
    expect(compiled.querySelector('.left-side')).toBeTruthy();
    expect(compiled.querySelector('.right-side')).toBeTruthy();
    expect(compiled.querySelector('.divider')).toBeTruthy();
    expect(compiled.querySelector('.pill-group')).toBeTruthy();
    expect(compiled.querySelector('.social-links')).toBeTruthy();
  });

  it('should have proper semantic HTML structure', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    expect(compiled.querySelector('main')).toBeTruthy();
    expect(compiled.querySelector('h1')).toBeTruthy();
    expect(compiled.querySelector('p')).toBeTruthy();
    expect(compiled.querySelector('a')).toBeTruthy();
  });

  it('should have proper ARIA attributes', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    const divider = compiled.querySelector('.divider');
    expect(divider?.getAttribute('role')).toBe('separator');
    expect(divider?.getAttribute('aria-label')).toBe('Divider');
  });

  it('should have SVG icons with proper attributes', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    const svgs = compiled.querySelectorAll('svg');
    expect(svgs.length).toBeGreaterThan(0);
    
    svgs.forEach(svg => {
      expect(svg.getAttribute('xmlns')).toBe('http://www.w3.org/2000/svg');
    });
  });

  it('should have responsive design elements', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    // Check for CSS custom properties that indicate responsive design
    const style = window.getComputedStyle(compiled);
    expect(style.getPropertyValue('--bright-blue')).toBeTruthy();
    expect(style.getPropertyValue('--electric-violet')).toBeTruthy();
  });

  it('should have proper color scheme variables', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    
    const style = window.getComputedStyle(compiled);
    const colorVars = [
      '--bright-blue',
      '--electric-violet',
      '--french-violet',
      '--vivid-pink',
      '--hot-red',
      '--orange-red',
      '--gray-900',
      '--gray-700',
      '--gray-400'
    ];
    
    colorVars.forEach(colorVar => {
      expect(style.getPropertyValue(colorVar)).toBeTruthy();
    });
  });
});
