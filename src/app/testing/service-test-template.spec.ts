import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

/**
 * Template for testing Angular services
 * 
 * Usage:
 * 1. Replace 'ExampleService' with your actual service name
 * 2. Replace 'example.service.ts' with your actual service file path
 * 3. Add your service methods and test them
 * 4. Remove this template file when you have actual services to test
 */

/*
import { ExampleService } from './example.service';

describe('ExampleService', () => {
  let service: ExampleService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ExampleService]
    });
    
    service = TestBed.inject(ExampleService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getData', () => {
    it('should return data from API', (done) => {
      const mockData = { id: 1, name: 'Test' };
      
      service.getData().subscribe(data => {
        expect(data).toEqual(mockData);
        done();
      });

      const req = httpMock.expectOne('/api/data');
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should handle errors gracefully', (done) => {
      service.getData().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('postData', () => {
    it('should post data to API', (done) => {
      const mockData = { name: 'Test' };
      const mockResponse = { id: 1, ...mockData };
      
      service.postData(mockData).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne('/api/data');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockData);
      req.flush(mockResponse);
    });
  });

  describe('updateData', () => {
    it('should update data via API', (done) => {
      const mockData = { id: 1, name: 'Updated Test' };
      
      service.updateData(1, mockData).subscribe(response => {
        expect(response).toEqual(mockData);
        done();
      });

      const req = httpMock.expectOne('/api/data/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(mockData);
      req.flush(mockData);
    });
  });

  describe('deleteData', () => {
    it('should delete data via API', (done) => {
      service.deleteData(1).subscribe(response => {
        expect(response).toBeNull();
        done();
      });

      const req = httpMock.expectOne('/api/data/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('localStorage operations', () => {
    beforeEach(() => {
      localStorage.clear();
    });

    afterEach(() => {
      localStorage.clear();
    });

    it('should save data to localStorage', () => {
      const testData = { key: 'value' };
      service.saveToLocalStorage('test', testData);
      
      const saved = localStorage.getItem('test');
      expect(saved).toBe(JSON.stringify(testData));
    });

    it('should retrieve data from localStorage', () => {
      const testData = { key: 'value' };
      localStorage.setItem('test', JSON.stringify(testData));
      
      const retrieved = service.getFromLocalStorage('test');
      expect(retrieved).toEqual(testData);
    });

    it('should return null for non-existent key', () => {
      const retrieved = service.getFromLocalStorage('non-existent');
      expect(retrieved).toBeNull();
    });
  });

  describe('caching', () => {
    it('should cache API responses', (done) => {
      const mockData = { id: 1, name: 'Cached Data' };
      
      // First call
      service.getCachedData().subscribe(data => {
        expect(data).toEqual(mockData);
        
        // Second call should use cache
        service.getCachedData().subscribe(cachedData => {
          expect(cachedData).toEqual(mockData);
          done();
        });
      });

      const req = httpMock.expectOne('/api/cached-data');
      req.flush(mockData);
    });
  });

  describe('error handling', () => {
    it('should handle network errors', (done) => {
      service.getData().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.name).toBe('NetworkError');
          done();
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.error(new ErrorEvent('Network error'));
    });

    it('should retry failed requests', (done) => {
      const mockData = { id: 1, name: 'Retry Success' };
      let attemptCount = 0;
      
      service.getDataWithRetry().subscribe(data => {
        expect(data).toEqual(mockData);
        expect(attemptCount).toBeGreaterThan(1);
        done();
      });

      const req = httpMock.expectOne('/api/data');
      req.flush('Error', { status: 500, statusText: 'Server Error' });
      
      // Second attempt
      const req2 = httpMock.expectOne('/api/data');
      req2.flush(mockData);
    });
  });

  describe('data transformation', () => {
    it('should transform API response', (done) => {
      const rawData = { id: 1, name: 'raw data' };
      const expectedTransformed = { id: 1, name: 'TRANSFORMED DATA' };
      
      service.getTransformedData().subscribe(data => {
        expect(data).toEqual(expectedTransformed);
        done();
      });

      const req = httpMock.expectOne('/api/data');
      req.flush(rawData);
    });
  });
});
*/

// Placeholder test to keep the file valid
describe('Service Test Template', () => {
  it('should be a placeholder for service tests', () => {
    expect(true).toBe(true);
  });
}); 