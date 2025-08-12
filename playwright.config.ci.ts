import { defineConfig, devices } from '@playwright/test';

/**
 * CI-specific Playwright configuration
 * Optimized for GitHub Actions with limited resources
 */
export default defineConfig({
  testDir: './e2e',
  
  /* Run tests in serial for CI stability */
  fullyParallel: false,
  workers: 1,
  
  /* Fail the build on CI if you accidentally left test.only */
  forbidOnly: !!process.env.CI,
  
  /* Retry failed tests once in CI */
  retries: process.env.CI ? 1 : 0,
  
  /* Reporter optimized for CI */
  reporter: [
    ['list'],
    ['junit', { outputFile: 'test-results/results.xml' }],
    ['html', { open: 'never' }]
  ],
  
  /* Global timeout for CI */
  timeout: 30 * 1000,
  
  /* Test options */
  use: {
    /* Base URL for CI environment */
    baseURL: 'http://localhost:4200',
    
    /* Collect trace on first retry */
    trace: 'retain-on-failure',
    
    /* Take screenshot on failure */
    screenshot: 'only-on-failure',
    
    /* Reduce timeouts for faster CI runs */
    actionTimeout: 10 * 1000,
    navigationTimeout: 15 * 1000,
  },

  /* Configure projects for CI - only Chromium to save time */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  /* No web server startup - handled in CI workflow */
  // webServer configuration removed for CI
});