import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of, throwError } from 'rxjs';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { By } from '@angular/platform-browser';
import { CharacterListPage } from './character-list.page';
import { CharactersService } from '../../../core/services/characters.services';
import { AuthService } from '../../../core/services/auth.service';

class CharactersServiceMock {
  list = jasmine.createSpy('list').and.returnValue(of([{ id: 1, name: 'Ayla', class: 'Rogue', species: 'Elf', level: 3, sheet: {} as any }]));
}
class RouterMock {}
class AuthServiceMock { logout = jasmine.createSpy('logout'); }
class ActivatedRouteMock { 
  snapshot = { paramMap: new Map() }; 
  queryParams = of({});
}

describe('CharacterListPage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CharacterListPage, RouterTestingModule],
      providers: [
        provideZonelessChangeDetection(),
        { provide: CharactersService, useClass: CharactersServiceMock },
        // Use RouterTestingModule instead of custom Router mock
        { provide: AuthService, useClass: AuthServiceMock },
        { provide: ActivatedRoute, useClass: ActivatedRouteMock },
      ]
    });
  });

  it('should load characters on init', () => {
    const fixture = TestBed.createComponent(CharacterListPage);
    const comp = fixture.componentInstance;
    fixture.detectChanges();
    expect(comp.characters().length).toBe(1);
  });

  it('should show error on load failure', () => {
    const svc = TestBed.inject(CharactersService) as unknown as CharactersServiceMock;
    (svc.list as any).and.returnValue(throwError(() => new Error('fail')));

    const fixture = TestBed.createComponent(CharacterListPage);
    fixture.detectChanges();

    const comp = fixture.componentInstance;
    expect(comp.error()).toContain('fail');
  });

  it('logout should navigate to login', () => {
    const fixture = TestBed.createComponent(CharacterListPage);
    const comp = fixture.componentInstance;
    const auth = TestBed.inject(AuthService) as unknown as AuthServiceMock;
    const router = TestBed.inject(Router);
    const navSpy = spyOn(router, 'navigate');

    comp.logout();
    expect(auth.logout).toHaveBeenCalled();
    expect(navSpy).toHaveBeenCalledWith(['/login']);
  });
});
