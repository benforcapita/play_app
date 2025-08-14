import { Component, signal, OnDestroy, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { ApiService } from './core/services/api.service';
import { CharactersService } from './core/services/characters.services';
import { Character, CharacterSheet, SheetSectionKey, ExtractionJobResponse, ExtractionJobStatus, ExtractionResult } from './core/models/character.models';
import { JobIndicatorComponent } from './core/components/job-indicator.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgIf, JobIndicatorComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('play-app');

  // Display state for API calls
  result = signal<any | null>(null);
  healthText = signal<string | null>(null);
  error = signal<string | null>(null);

  // Character service state
  characters = signal<Character[] | null>(null);
  character = signal<Character | null>(null);
  characterSheet = signal<CharacterSheet | null>(null);
  characterId = signal<number>(1);
  sheetSection = signal<SheetSectionKey>('characterinfo');

  // Extraction state
  extractionJobToken = signal<string | null>(null);
  extractionStatus = signal<ExtractionJobStatus | null>(null);
  extractionResult = signal<ExtractionResult | null>(null);
  isExtracting = signal<boolean>(false);
  statusCheckInterval = signal<any>(null);

  constructor(private api: ApiService, private charactersService: CharactersService, private router: Router) {}

  checkPing() {
    this.error.set(null);
    this.healthText.set(null);
    this.result.set(null);

    this.api.getPing().subscribe({
      next: (r: any) => this.result.set(r),
      error: (err: any) => this.error.set(err?.message ?? 'Request failed')
    });
  }

  checkHealth() {
    this.error.set(null);
    this.result.set(null);
    this.healthText.set(null);

    this.api.getHealth().subscribe({
      next: (text: any) => this.healthText.set(typeof text === 'string' ? text : JSON.stringify(text)),
      error: (err: any) => this.error.set(err?.message ?? 'Request failed')
    });
  }

  // Character service methods
  clearCharacterResults() {
    this.characters.set(null);
    this.character.set(null);
    this.characterSheet.set(null);
    this.error.set(null);
  }

  listCharacters() {
    this.clearCharacterResults();
    this.charactersService.list().subscribe({
      next: (characters: Character[]) => this.characters.set(characters),
      error: (err: any) => this.error.set(err?.message ?? 'Failed to list characters')
    });
  }

  getCharacter() {
    this.clearCharacterResults();
    const id = this.characterId();
    this.charactersService.get(id).subscribe({
      next: (character: Character) => this.character.set(character),
      error: (err: any) => this.error.set(err?.message ?? `Failed to get character ${id}`)
    });
  }

  updateCharacter() {
    this.clearCharacterResults();
    const id = this.characterId();
    const currentCharacter = this.character();
    
    if (!currentCharacter) {
      this.error.set('No character loaded to update. Get a character first.');
      return;
    }

    this.charactersService.update(id, currentCharacter).subscribe({
      next: (character: Character) => this.character.set(character),
      error: (err: any) => this.error.set(err?.message ?? `Failed to update character ${id}`)
    });
  }

  deleteCharacter() {
    this.clearCharacterResults();
    const id = this.characterId();
    this.charactersService.delete(id).subscribe({
      next: () => {
        this.character.set(null);
        // Show success message in result
        this.result.set({ message: `Character ${id} deleted successfully` });
      },
      error: (err: any) => this.error.set(err?.message ?? `Failed to delete character ${id}`)
    });
  }

  getCharacterSheet() {
    this.clearCharacterResults();
    const id = this.characterId();
    const section = this.sheetSection();
    this.charactersService.getSheet(id, section).subscribe({
      next: (sheet: CharacterSheet) => this.characterSheet.set(sheet),
      error: (err: any) => this.error.set(err?.message ?? `Failed to get character sheet section ${section} for character ${id}`)
    });
  }

  // Extraction methods
  clearExtractionResults() {
    this.extractionJobToken.set(null);
    this.extractionStatus.set(null);
    this.extractionResult.set(null);
    this.isExtracting.set(false);
    this.error.set(null);
    this.stopPolling();
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (file) {
      this.startExtraction(file);
    } else {
      this.error.set('No file selected');
    }
  }

  startExtraction(file: File) {
    this.clearExtractionResults();
    this.isExtracting.set(true);

    // Validate file type
    const supportedTypes = ['image/png', 'image/jpeg', 'image/webp', 'image/gif', 'application/pdf'];
    if (!supportedTypes.includes(file.type)) {
      this.error.set('Unsupported file type. Please use PNG, JPEG, WebP, GIF, or PDF files.');
      this.isExtracting.set(false);
      return;
    }

    this.charactersService.startExtraction(file).subscribe({
      next: (response: ExtractionJobResponse) => {
        this.extractionJobToken.set(response.jobToken);
        this.result.set({ message: response.message, jobToken: response.jobToken });
        this.pollExtractionStatus();
      },
      error: (err: any) => {
        this.error.set(err?.message ?? 'Failed to start extraction');
        this.isExtracting.set(false);
      }
    });
  }

  checkExtractionStatus() {
    const token = this.extractionJobToken();
    if (!token) {
      this.error.set('No extraction job token available');
      return;
    }

    this.charactersService.getExtractionStatus(token).subscribe({
      next: (status: ExtractionJobStatus) => {
        this.extractionStatus.set(status);
        
        if (status.status === 'completed') {
          this.isExtracting.set(false);
          this.stopPolling();
          
          if (status.isSuccessful) {
            this.getExtractionResult();
          } else {
            this.error.set(status.errorMessage || 'Extraction completed with errors');
          }
        } else if (status.status === 'failed') {
          this.isExtracting.set(false);
          this.stopPolling();
          this.error.set(status.errorMessage || 'Extraction failed');
        }
        // For 'pending' or 'running' status, continue polling
      },
      error: (err: any) => {
        this.error.set(err?.message ?? 'Failed to check extraction status');
        this.isExtracting.set(false);
        this.stopPolling();
      }
    });
  }

  pollExtractionStatus() {
    // Check status immediately, then every 2 seconds
    this.checkExtractionStatus();
    
    const interval = setInterval(() => {
      const status = this.extractionStatus();
      if (!status || status.status === 'completed' || status.status === 'failed') {
        this.stopPolling();
        return;
      }
      this.checkExtractionStatus();
    }, 2000);
    
    this.statusCheckInterval.set(interval);
  }

  stopPolling() {
    const interval = this.statusCheckInterval();
    if (interval) {
      clearInterval(interval);
      this.statusCheckInterval.set(null);
    }
  }

  getExtractionResult() {
    const token = this.extractionJobToken();
    if (!token) {
      this.error.set('No extraction job token available');
      return;
    }

    this.charactersService.getExtractionResult(token).subscribe({
      next: (result: ExtractionResult) => {
        this.extractionResult.set(result);
        this.character.set(result.character); // Also set the extracted character
        this.result.set({ 
          message: 'Character extraction completed successfully!',
          successRate: `${(result.jobSummary.successRate * 100).toFixed(1)}%`,
          sectionsProcessed: `${result.jobSummary.successfulSections}/${result.jobSummary.totalSections}`
        });
      },
      error: (err: any) => {
        this.error.set(err?.message ?? 'Failed to get extraction result');
      }
    });
  }

  // Helper methods for UI
  updateCharacterId(event: Event) {
    const target = event.target as HTMLInputElement;
    const id = parseInt(target.value);
    if (!isNaN(id)) {
      this.characterId.set(id);
    }
  }

  updateSheetSection(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.sheetSection.set(target.value as SheetSectionKey);
  }

  // Cleanup on component destroy
  ngOnDestroy() {
    this.stopPolling();
  }

  goBack() {
    // Try history back; fallback to characters
    if (typeof window !== 'undefined' && window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/characters']);
    }
  }

  // Back button visibility (hide on character list page)
  showBack = signal(true);

  ngOnInit() {
    this.updateBackVisibility(this.router.url);
    this.router.events.subscribe((e) => {
      if (e instanceof NavigationEnd) {
        this.updateBackVisibility(e.urlAfterRedirects || e.url);
      }
    });
  }

  private updateBackVisibility(url: string) {
    const path = url.split('?')[0];
    const noBackPaths = ['/characters', '/login'];
    this.showBack.set(!noBackPaths.includes(path));
  }
}
