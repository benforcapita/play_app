import { TestBed } from '@angular/core/testing';
import { ExtractPage } from './extract.page';

function makeFile(name: string): File {
  return new File(['content'], name, { type: 'image/png' });
}

describe('ExtractPage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ExtractPage]
    });
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(ExtractPage);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should update fileName on input change', () => {
    const fixture = TestBed.createComponent(ExtractPage);
    const comp = fixture.componentInstance;

    const input = document.createElement('input');
    const file = makeFile('sheet.png');
    const dt = new DataTransfer();
    dt.items.add(file);
    input.files = dt.files;

    comp.onFileChange({ target: input } as any);

    expect(comp.fileName()).toBe('sheet.png');
  });
});
