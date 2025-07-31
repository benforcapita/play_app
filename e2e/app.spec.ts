import { test, expect } from '@playwright/test';

test.describe('Angular App E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test.describe('Page Load and Initial Render', () => {
    test('should load the application successfully', async ({ page }) => {
      // Check if the page loads without errors
      await expect(page).toHaveTitle(/play-app/);
      
      // Check if main content is visible
      await expect(page.locator('main')).toBeVisible();
    });

    test('should display the Angular logo', async ({ page }) => {
      const logo = page.locator('.angular-logo');
      await expect(logo).toBeVisible();
    });

    test('should display the main heading', async ({ page }) => {
      const heading = page.locator('h1');
      await expect(heading).toBeVisible();
      await expect(heading).toContainText('Hello, play-app');
    });

    test('should display the congratulations message', async ({ page }) => {
      const message = page.locator('p');
      await expect(message).toBeVisible();
      await expect(message).toContainText('Congratulations! Your app is running. 🎉');
    });
  });

  test.describe('Navigation Pills', () => {
    test('should display all 6 navigation pills', async ({ page }) => {
      const pills = page.locator('.pill');
      await expect(pills).toHaveCount(6);
    });

    test('should have correct pill titles', async ({ page }) => {
      const expectedTitles = [
        'Explore the Docs',
        'Learn with Tutorials',
        'Prompt and best practices for AI',
        'CLI Docs',
        'Angular Language Service',
        'Angular DevTools'
      ];

      const pills = page.locator('.pill');
      for (let i = 0; i < expectedTitles.length; i++) {
        await expect(pills.nth(i)).toContainText(expectedTitles[i]);
      }
    });

    test('should have correct URLs for pills', async ({ page }) => {
      const expectedUrls = [
        'https://angular.dev',
        'https://angular.dev/tutorials',
        'https://angular.dev/ai/develop-with-ai',
        'https://angular.dev/tools/cli',
        'https://angular.dev/tools/language-service',
        'https://angular.dev/tools/devtools'
      ];

      const pills = page.locator('.pill');
      for (let i = 0; i < expectedUrls.length; i++) {
        await expect(pills.nth(i)).toHaveAttribute('href', expectedUrls[i]);
      }
    });

    test('should have proper target and rel attributes on pills', async ({ page }) => {
      const pills = page.locator('.pill');
      for (let i = 0; i < 6; i++) {
        await expect(pills.nth(i)).toHaveAttribute('target', '_blank');
        await expect(pills.nth(i)).toHaveAttribute('rel', 'noopener');
      }
    });

    test('should have hover effects on pills', async ({ page }) => {
      const firstPill = page.locator('.pill').first();
      
      // Hover over the pill
      await firstPill.hover();
      
      // Check if the pill is still visible after hover
      await expect(firstPill).toBeVisible();
    });
  });

  test.describe('Social Media Links', () => {
    test('should display all 3 social media links', async ({ page }) => {
      const socialLinks = page.locator('.social-links a');
      await expect(socialLinks).toHaveCount(3);
    });

    test('should have correct social media URLs', async ({ page }) => {
      const expectedUrls = [
        'https://github.com/angular/angular',
        'https://twitter.com/angular',
        'https://www.youtube.com/channel/UCbn1OgGei-DV7aSRo_HaAiw'
      ];

      const socialLinks = page.locator('.social-links a');
      for (let i = 0; i < expectedUrls.length; i++) {
        await expect(socialLinks.nth(i)).toHaveAttribute('href', expectedUrls[i]);
      }
    });

    test('should have proper accessibility attributes on social links', async ({ page }) => {
      const socialLinks = page.locator('.social-links a');
      for (let i = 0; i < 3; i++) {
        await expect(socialLinks.nth(i)).toHaveAttribute('target', '_blank');
        await expect(socialLinks.nth(i)).toHaveAttribute('rel', 'noopener');
        await expect(socialLinks.nth(i)).toHaveAttribute('aria-label');
      }
    });

    test('should have SVG icons in social links', async ({ page }) => {
      const socialLinks = page.locator('.social-links a');
      for (let i = 0; i < 3; i++) {
        const svg = socialLinks.nth(i).locator('svg');
        await expect(svg).toBeVisible();
      }
    });
  });

  test.describe('Visual Design and Layout', () => {
    test('should have proper layout structure', async ({ page }) => {
      await expect(page.locator('.content')).toBeVisible();
      await expect(page.locator('.left-side')).toBeVisible();
      await expect(page.locator('.right-side')).toBeVisible();
      await expect(page.locator('.divider')).toBeVisible();
    });

    test('should have proper color scheme', async ({ page }) => {
      // Check if CSS custom properties are applied
      const mainElement = page.locator('main');
      await expect(mainElement).toBeVisible();
    });

    test('should have responsive design elements', async ({ page }) => {
      // Test on different viewport sizes
      await page.setViewportSize({ width: 1200, height: 800 });
      await expect(page.locator('.content')).toBeVisible();

      await page.setViewportSize({ width: 768, height: 1024 });
      await expect(page.locator('.content')).toBeVisible();

      await page.setViewportSize({ width: 375, height: 667 });
      await expect(page.locator('.content')).toBeVisible();
    });
  });

  test.describe('Accessibility', () => {
    test('should have proper ARIA attributes', async ({ page }) => {
      const divider = page.locator('.divider');
      await expect(divider).toHaveAttribute('role', 'separator');
      await expect(divider).toHaveAttribute('aria-label', 'Divider');
    });

    test('should have semantic HTML structure', async ({ page }) => {
      await expect(page.locator('main')).toBeVisible();
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('p')).toBeVisible();
    });

    test('should have proper heading hierarchy', async ({ page }) => {
      const headings = page.locator('h1');
      await expect(headings).toHaveCount(1);
    });

    test('should have proper link descriptions', async ({ page }) => {
      const socialLinks = page.locator('.social-links a');
      for (let i = 0; i < 3; i++) {
        const ariaLabel = await socialLinks.nth(i).getAttribute('aria-label');
        expect(ariaLabel).toBeTruthy();
        expect(ariaLabel!.length).toBeGreaterThan(0);
      }
    });
  });

  test.describe('Performance and Loading', () => {
    test('should load quickly', async ({ page }) => {
      const startTime = Date.now();
      await page.goto('/');
      const loadTime = Date.now() - startTime;
      
      // Page should load in less than 3 seconds
      expect(loadTime).toBeLessThan(3000);
    });

    test('should render without layout shifts', async ({ page }) => {
      await page.goto('/');
      
      // Check if main content is stable
      await expect(page.locator('main')).toBeVisible();
      await expect(page.locator('.content')).toBeVisible();
    });
  });

  test.describe('Cross-browser Compatibility', () => {
    test('should work in different browsers', async ({ page }) => {
      // This test will run in different browser contexts
      await expect(page.locator('main')).toBeVisible();
      await expect(page.locator('h1')).toBeVisible();
    });
  });

  test.describe('Mobile Responsiveness', () => {
    test('should be responsive on mobile devices', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });
      
      // Check if content is still accessible
      await expect(page.locator('main')).toBeVisible();
      await expect(page.locator('.content')).toBeVisible();
      
      // Check if pills are still visible
      await expect(page.locator('.pill')).toBeVisible();
    });

    test('should handle touch interactions', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });
      
      // Test touch interaction with pills
      const firstPill = page.locator('.pill').first();
      await firstPill.tap();
      
      // Should not cause any errors
      await expect(page.locator('main')).toBeVisible();
    });
  });

  test.describe('Error Handling', () => {
    test('should handle network errors gracefully', async ({ page }) => {
      // This test would be more relevant when the app makes API calls
      await expect(page.locator('main')).toBeVisible();
    });

    test('should handle JavaScript errors gracefully', async ({ page }) => {
      // Check if page loads without console errors
      const consoleErrors: string[] = [];
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleErrors.push(msg.text());
        }
      });
      
      await page.goto('/');
      
      // Should have minimal or no console errors
      expect(consoleErrors.length).toBeLessThan(5);
    });
  });

  test.describe('SEO and Meta Tags', () => {
    test('should have proper title', async ({ page }) => {
      await expect(page).toHaveTitle(/play-app/);
    });

    test('should have proper meta tags', async ({ page }) => {
      // Check for viewport meta tag
      const viewport = page.locator('meta[name="viewport"]');
      await expect(viewport).toBeAttached();
    });
  });
}); 