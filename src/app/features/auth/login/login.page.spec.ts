import { TestBed } from '@angular/core/testing';
import { LoginPage } from './login.page';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { By } from '@angular/platform-browser';

class AuthServiceMock {
  login = jasmine.createSpy('login').and.returnValue(of({ access_token: 't' }));
}
class RouterMock {
  navigate = jasmine.createSpy('navigate');
}

describe('LoginPage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        { provide: AuthService, useClass: AuthServiceMock },
        { provide: Router, useClass: RouterMock }
      ]
    });
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(LoginPage);
    const comp = fixture.componentInstance;
    expect(comp).toBeTruthy();
  });

  it('should call login and navigate on submit', () => {
    const fixture = TestBed.createComponent(LoginPage);
    const comp = fixture.componentInstance;
    const auth = TestBed.inject(AuthService) as unknown as AuthServiceMock;
    const router = TestBed.inject(Router) as unknown as RouterMock;

    comp.email = 'you@example.com';
    comp.password = 'secret123';
    fixture.detectChanges();

    comp.onSubmit();

    expect(auth.login).toHaveBeenCalledWith('you@example.com', 'secret123');
    expect(router.navigate).toHaveBeenCalledWith(['/characters']);
  });

  it('should show error on login failure', () => {
    const fixture = TestBed.createComponent(LoginPage);
    const comp = fixture.componentInstance;
    const auth = TestBed.inject(AuthService) as unknown as AuthServiceMock;

    (auth.login as any).and.returnValue(throwError(() => new Error('bad creds')));

    comp.email = 'you@example.com';
    comp.password = 'wrong';
    fixture.detectChanges();

    comp.onSubmit();
    fixture.detectChanges();

    const errorEl = fixture.debugElement.query(By.css('.error'))?.nativeElement as HTMLElement;
    expect(comp.error()).toContain('bad creds');
    expect(errorEl.textContent || '').toContain('bad creds');
  });
});
