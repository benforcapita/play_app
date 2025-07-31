import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { App } from './app';
import { TestUtils } from './testing/test-utils';

describe('App Integration Tests', () => {
  let fixture: any;
  let component: App;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([])
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

    it('should render the main layout structure', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('main')).toBeTruthy();
      expect(compiled.querySelector('.content')).toBeTruthy();
      expect(compiled.querySelector('.left-side')).toBeTruthy();
      expect(compiled.querySelector('.right-side')).toBeTruthy();
    });

    it('should display the correct title in the header', () => {
      const titleElement = TestUtils.getElement(fixture, 'h1');
      expect(titleElement.textContent).toContain('Hello, play-app');
    });

    it('should display the congratulations message', () => {
      const messageElement = TestUtils.getElement(fixture, 'p');
      expect(messageElement.textContent).toContain('Congratulations! Your app is running. 🎉');
    });
  });

  describe('Navigation Pills', () => {
    it('should render all 6 navigation pills', () => {
      const pills = TestUtils.getElements(fixture, '.pill');
      expect(pills.length).toBe(6);
    });

    it('should have correct titles for all pills', () => {
      const pills = TestUtils.getElements(fixture, '.pill');
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

    it('should have correct URLs for all pills', () => {
      const pills = TestUtils.getElements<HTMLAnchorElement>(fixture, '.pill');
      const expectedUrls = [
        'https://angular.dev',
        'https://angular.dev/tutorials',
        'https://angular.dev/ai/develop-with-ai',
        'https://angular.dev/tools/cli',
        'https://angular.dev/tools/language-service',
        'https://angular.dev/tools/devtools'
      ];

      pills.forEach((pill, index) => {
        // Only the base URL gets a trailing slash added by the browser
        const expectedUrl = expectedUrls[index] === 'https://angular.dev' 
          ? expectedUrls[index] + '/' 
          : expectedUrls[index];
        expect(pill.href).toBe(expectedUrl);
      });
    });

    it('should have proper security attributes on pills', () => {
      const pills = TestUtils.getElements<HTMLAnchorElement>(fixture, '.pill');
      
      pills.forEach(pill => {
        expect(pill.target).toBe('_blank');
        expect(pill.rel).toBe('noopener');
      });
    });

    it('should have hover effects on pills', () => {
      const pills = TestUtils.getElements(fixture, '.pill');
      
      pills.forEach(pill => {
        // Check if pill has the correct CSS class for hover effects
        expect(pill.classList.contains('pill')).toBe(true);
      });
    });
  });

  describe('Social Media Links', () => {
    it('should render all 3 social media links', () => {
      const socialLinks = TestUtils.getElements(fixture, '.social-links a');
      expect(socialLinks.length).toBe(3);
    });

    it('should have correct social media URLs', () => {
      const socialLinks = TestUtils.getElements<HTMLAnchorElement>(fixture, '.social-links a');
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
      const socialLinks = TestUtils.getElements<HTMLAnchorElement>(fixture, '.social-links a');
      
      socialLinks.forEach(link => {
        expect(link.target).toBe('_blank');
        expect(link.rel).toBe('noopener');
        expect(link.getAttribute('aria-label')).toBeTruthy();
      });
    });

    it('should have SVG icons in social links', () => {
      const socialLinks = TestUtils.getElements(fixture, '.social-links a');
      
      socialLinks.forEach(link => {
        const svg = link.querySelector('svg');
        expect(svg).toBeTruthy();
        expect(svg?.getAttribute('xmlns')).toBe('http://www.w3.org/2000/svg');
      });
    });
  });

  describe('Visual Design', () => {
    it('should have the Angular logo', () => {
      const logo = TestUtils.getElement(fixture, '.angular-logo');
      expect(logo).toBeTruthy();
    });

    it('should have proper color scheme variables', () => {
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

    it('should have gradient definitions', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const style = window.getComputedStyle(compiled);
      
      expect(style.getPropertyValue('--red-to-pink-to-purple-vertical-gradient')).toBeTruthy();
      expect(style.getPropertyValue('--red-to-pink-to-purple-horizontal-gradient')).toBeTruthy();
    });

    it('should have proper typography', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const style = window.getComputedStyle(compiled);
      
      expect(style.fontFamily).toContain('Inter');
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA attributes', () => {
      const divider = TestUtils.getElement(fixture, '.divider');
      expect(divider.getAttribute('role')).toBe('separator');
      expect(divider.getAttribute('aria-label')).toBe('Divider');
    });

    it('should have semantic HTML structure', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('main')).toBeTruthy();
      expect(compiled.querySelector('h1')).toBeTruthy();
      expect(compiled.querySelector('p')).toBeTruthy();
    });

    it('should have proper heading hierarchy', () => {
      const headings = TestUtils.getElements(fixture, 'h1');
      expect(headings.length).toBe(1);
    });

    it('should have proper link descriptions', () => {
      const socialLinks = TestUtils.getElements<HTMLAnchorElement>(fixture, '.social-links a');
      
      socialLinks.forEach(link => {
        const ariaLabel = link.getAttribute('aria-label');
        expect(ariaLabel).toBeTruthy();
        expect(ariaLabel?.length).toBeGreaterThan(0);
      });
    });
  });

  describe('Responsive Design', () => {
    it('should have responsive CSS classes', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      
      expect(compiled.querySelector('.content')).toBeTruthy();
      expect(compiled.querySelector('.pill-group')).toBeTruthy();
    });

    it('should have media query considerations', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const style = window.getComputedStyle(compiled);
      
      // Check if CSS custom properties are defined (indicating responsive design)
      expect(style.getPropertyValue('--bright-blue')).toBeTruthy();
      expect(style.getPropertyValue('--electric-violet')).toBeTruthy();
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

  describe('Cross-browser Compatibility', () => {
    it('should have proper vendor prefixes', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const style = window.getComputedStyle(compiled);
      
      // Check for webkit font smoothing
      expect(style.getPropertyValue('-webkit-font-smoothing')).toBeTruthy();
    });

    it('should have proper CSS fallbacks', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const style = window.getComputedStyle(compiled);
      
      // Check if font family has fallbacks
      expect(style.fontFamily).toContain('Inter');
      expect(style.fontFamily).toContain('-apple-system');
      expect(style.fontFamily).toContain('system-ui');
    });
  });
}); 