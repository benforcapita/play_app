# Testing Documentation

This Angular project includes comprehensive testing setup with multiple testing strategies to ensure code quality and reliability.

## Testing Strategy

### 1. Unit Tests (Jasmine + Karma)
- **Location**: `src/app/*.spec.ts`
- **Purpose**: Test individual components, services, and functions in isolation
- **Coverage**: 80% minimum coverage required

### 2. Integration Tests
- **Location**: `src/app/app.integration.spec.ts`
- **Purpose**: Test component interactions and app-wide functionality
- **Scope**: Tests the entire app as a cohesive unit

### 3. E2E Tests (Playwright)
- **Location**: `e2e/`
- **Purpose**: Test the application from a user's perspective
- **Browsers**: Chrome, Firefox, Safari, Mobile Chrome, Mobile Safari

### 4. Router Tests
- **Location**: `src/app/app.routes.spec.ts`
- **Purpose**: Test routing configuration and navigation

## Running Tests

### Unit Tests
```bash
# Run all unit tests
npm run test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage report
npm run test:coverage
```

### E2E Tests
```bash
# Install Playwright browsers (first time only)
npx playwright install

# Run all E2E tests
npm run test:e2e

# Run E2E tests with UI
npm run test:e2e:ui

# Run E2E tests in headed mode (visible browser)
npm run test:e2e:headed
```

### All Tests
```bash
# Run both unit and E2E tests
npm run test:all
```

## Test Structure

### Unit Tests (`app.spec.ts`)
Tests individual component functionality:
- Component creation
- Template rendering
- Signal values
- User interactions
- Accessibility features
- Visual design elements

### Integration Tests (`app.integration.spec.ts`)
Tests the app as a whole:
- Component initialization
- Navigation pills functionality
- Social media links
- Visual design and layout
- Accessibility compliance
- Responsive design
- Router integration
- Performance metrics
- Cross-browser compatibility

### E2E Tests (`e2e/app.spec.ts`)
Tests from user perspective:
- Page load and initial render
- Navigation pill interactions
- Social media link functionality
- Visual design verification
- Accessibility testing
- Performance and loading
- Cross-browser compatibility
- Mobile responsiveness
- Error handling
- SEO and meta tags

### Router Tests (`app.routes.spec.ts`)
Tests routing functionality:
- Route configuration
- Navigation behavior
- Route guards (when implemented)
- Route parameters (when implemented)
- Child routes (when implemented)
- Lazy loading (when implemented)

## Test Utilities

### TestUtils Class (`src/app/testing/test-utils.ts`)
Provides common testing functions:
- Element selection by CSS selector
- Element selection by test ID
- Click event simulation
- Input value setting
- Class and attribute checking
- Text content retrieval
- Visibility checking
- Async operation waiting
- Mock event creation
- Style property checking

### Custom Matchers
Custom Jasmine matchers for better test readability:
- `toHaveClass(className)`
- `toHaveAttribute(attr, value?)`
- `toBeVisible()`
- `toBeHidden()`

## Coverage Requirements

The project enforces 80% minimum coverage for:
- Statements
- Branches
- Functions
- Lines

## Test Best Practices

### 1. Test Organization
- Group related tests using `describe` blocks
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

### 2. Component Testing
- Test component creation
- Test template rendering
- Test user interactions
- Test accessibility features
- Test responsive behavior

### 3. Service Testing
- Test service creation
- Test API calls with HttpTestingController
- Test error handling
- Test data transformation
- Test caching mechanisms

### 4. E2E Testing
- Test critical user journeys
- Test cross-browser compatibility
- Test mobile responsiveness
- Test performance metrics
- Test accessibility compliance

### 5. Router Testing
- Test route configuration
- Test navigation behavior
- Test route guards
- Test route parameters
- Test lazy loading

## Adding New Tests

### For New Components
1. Create `component.spec.ts` file
2. Import necessary testing modules
3. Test component creation
4. Test template rendering
5. Test user interactions
6. Test accessibility features

### For New Services
1. Create `service.spec.ts` file
2. Use `HttpClientTestingModule` for API calls
3. Test service methods
4. Test error handling
5. Test data transformation

### For New Routes
1. Add route tests to `app.routes.spec.ts`
2. Test route configuration
3. Test navigation behavior
4. Test route guards if applicable

### For E2E Tests
1. Add tests to `e2e/app.spec.ts`
2. Test user interactions
3. Test cross-browser compatibility
4. Test mobile responsiveness

## Debugging Tests

### Unit Tests
```bash
# Run specific test file
npm run test -- --include="**/app.spec.ts"

# Run tests with verbose output
npm run test -- --verbose
```

### E2E Tests
```bash
# Run specific test
npm run test:e2e -- --grep "should load the application"

# Run tests with debug mode
npm run test:e2e -- --debug
```

## Continuous Integration

The test setup is configured for CI/CD:
- Unit tests run on every commit
- E2E tests run on pull requests
- Coverage reports are generated
- Test results are reported to CI system

## Performance Testing

Tests include performance checks:
- Component render time < 100ms
- Page load time < 3 seconds
- No layout shifts during rendering
- Minimal console errors

## Accessibility Testing

Tests verify accessibility compliance:
- ARIA attributes
- Semantic HTML structure
- Keyboard navigation
- Screen reader compatibility
- Color contrast ratios

## Browser Compatibility

E2E tests run on:
- Chrome (Desktop)
- Firefox (Desktop)
- Safari (Desktop)
- Chrome (Mobile)
- Safari (Mobile)

## Troubleshooting

### Common Issues

1. **Tests failing due to timing**
   - Use `fixture.whenStable()` for async operations
   - Use `TestUtils.waitForAsync()` utility

2. **E2E tests failing on CI**
   - Check browser installation
   - Verify viewport settings
   - Check network connectivity

3. **Coverage not meeting thresholds**
   - Add tests for uncovered code paths
   - Mock external dependencies
   - Test error scenarios

### Debug Commands
```bash
# Debug unit tests
npm run test -- --browsers=Chrome --single-run=false

# Debug E2E tests
npm run test:e2e -- --headed --debug
```

## Future Enhancements

- Add visual regression testing
- Add performance benchmarking
- Add accessibility audit testing
- Add security testing
- Add load testing for API endpoints 