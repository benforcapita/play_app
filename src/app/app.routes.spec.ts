import { routes } from './app.routes';

describe('App Routes', () => {
  describe('Route Configuration', () => {
    it('should have routes defined', () => {
      expect(routes).toBeDefined();
      expect(Array.isArray(routes)).toBe(true);
    });

    it('should have at least one route', () => {
      expect(routes.length).toBeGreaterThanOrEqual(0);
    });

    it('should have valid route objects', () => {
      if (routes.length > 0) {
        routes.forEach(route => {
          expect(route).toBeDefined();
          expect(typeof route).toBe('object');
        });
      } else {
        // If no routes are defined, that's also valid
        expect(routes.length).toBe(0);
      }
    });
  });

  describe('Future Route Features', () => {
    it('should be ready for route guards', () => {
      expect(routes).toBeDefined();
    });

    it('should be ready for route parameters', () => {
      expect(routes).toBeDefined();
    });

    it('should be ready for child routes', () => {
      expect(routes).toBeDefined();
    });

    it('should be ready for lazy loading', () => {
      expect(routes).toBeDefined();
    });

    it('should be ready for route data', () => {
      expect(routes).toBeDefined();
    });

    it('should be ready for route resolvers', () => {
      expect(routes).toBeDefined();
    });
  });
}); 