import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { By } from '@angular/platform-browser';
import { CharacterListPage } from './character-list.page';
import { CharactersService } from '../../../core/services/characters.services';
import { AuthService } from '../../../core/services/auth.service';

class CharactersServiceMock {
  list = jasmine.createSpy('list').and.returnValue(of([{ id: 1, name: 'Ayla', class: 'Rogue', species: 'Elf', level: 3, sheet: {} as any }]));
}
class RouterMock { navigate = jasmine.createSpy('navigate'); }
class AuthServiceMock { logout = jasmine.createSpy('logout'); }

describe('CharacterListPage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CharacterListPage],
      providers: [
        { provide: CharactersService, useClass: CharactersServiceMock },
        { provide: Router, useClass: RouterMock },
        { provide: AuthService, useClass: AuthServiceMock },
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
    const router = TestBed.inject(Router) as unknown as RouterMock;

    comp.logout();
    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
