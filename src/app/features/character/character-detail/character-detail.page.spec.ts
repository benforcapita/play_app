import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CharacterDetailPage } from './character-detail.page';
import { CharactersService } from '../../../core/services/characters.services';

class ActivatedRouteMock { snapshot = { paramMap: new Map([['id', '1']]) }; }
class CharactersServiceMock {
  get = jasmine.createSpy('get').and.returnValue(of({ id: 1, name: 'Ayla', class: 'Rogue', species: 'Elf', level: 3, sheet: {} as any }));
}

describe('CharacterDetailPage', () => {
  it('should load character by id', () => {
    TestBed.configureTestingModule({
      imports: [CharacterDetailPage],
      providers: [
        { provide: ActivatedRoute, useClass: ActivatedRouteMock },
        { provide: CharactersService, useClass: CharactersServiceMock },
      ]
    });

    const fixture = TestBed.createComponent(CharacterDetailPage);
    const comp = fixture.componentInstance;
    fixture.detectChanges();

    expect(comp.character()).toBeTruthy();
  });

  it('should set error when service fails', () => {
    class FailingSvc extends CharactersServiceMock {
      override get = jasmine.createSpy('get').and.returnValue(throwError(() => new Error('boom')));
    }

    TestBed.configureTestingModule({
      imports: [CharacterDetailPage],
      providers: [
        { provide: ActivatedRoute, useClass: ActivatedRouteMock },
        { provide: CharactersService, useClass: FailingSvc },
      ]
    });

    const fixture = TestBed.createComponent(CharacterDetailPage);
    const comp = fixture.componentInstance;
    fixture.detectChanges();

    expect(comp.error()).toContain('boom');
  });
});