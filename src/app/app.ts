import { Component, signal, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgIf, NgFor, JsonPipe, TitleCasePipe, DatePipe } from '@angular/common';
import { ApiService } from './core/services/api.service';
import { CharactersService } from './core/services/characters.services';
import { Character, CharacterSheet, SheetSectionKey, ExtractionJobResponse, ExtractionJobStatus, ExtractionResult } from './core/models/character.models';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgIf, NgFor, JsonPipe, TitleCasePipe, DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnDestroy {
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

  constructor(private api: ApiService, private charactersService: CharactersService) {}

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
      console.log('File selected for extraction:', {
        name: file.name,
        size: file.size,
        type: file.type,
        lastModified: file.lastModified
      });
      this.startExtraction(file);
    } else {
      console.warn('No file selected');
    }
  }

  startExtraction(file: File) {
    console.log('Starting extraction process for file:', file.name);
    this.clearExtractionResults();
    this.isExtracting.set(true);

    // Validate file type
    const supportedTypes = ['image/png', 'image/jpeg', 'image/webp', 'image/gif', 'application/pdf'];
    if (!supportedTypes.includes(file.type)) {
      console.warn('Unsupported file type:', file.type);
      this.error.set('Unsupported file type. Please use PNG, JPEG, WebP, GIF, or PDF files.');
      this.isExtracting.set(false);
      return;
    }

    console.log('File validation passed, calling API...');
    this.charactersService.startExtraction(file).subscribe({
      next: (response: ExtractionJobResponse) => {
        console.log('Extraction started successfully:', response);
        this.extractionJobToken.set(response.jobToken);
        this.result.set({ message: response.message, jobToken: response.jobToken });
        this.pollExtractionStatus();
      },
      error: (err: any) => {
        console.error('Extraction failed in app component:', err);
        this.error.set(err?.message ?? 'Failed to start extraction');
        this.isExtracting.set(false);
      }
    });
  }

  checkExtractionStatus() {
    const token = this.extractionJobToken();
    if (!token) {
      console.warn('No extraction job token available');
      this.error.set('No extraction job token available');
      return;
    }

    console.log('Checking extraction status for token:', token);
    this.charactersService.getExtractionStatus(token).subscribe({
      next: (status: ExtractionJobStatus) => {
        console.log('Extraction status received:', status);
        this.extractionStatus.set(status);
        
        if (status.status === 'completed') {
          console.log('Extraction completed, success:', status.isSuccessful);
          this.isExtracting.set(false);
          this.stopPolling();
          
          if (status.isSuccessful) {
            this.getExtractionResult();
          } else {
            console.warn('Extraction completed with errors:', status.errorMessage);
            this.error.set(status.errorMessage || 'Extraction completed with errors');
          }
        } else if (status.status === 'failed') {
          console.error('Extraction failed:', status.errorMessage);
          this.isExtracting.set(false);
          this.stopPolling();
          this.error.set(status.errorMessage || 'Extraction failed');
        } else {
          console.log('Extraction still in progress, status:', status.status);
        }
        // For 'pending' or 'running' status, continue polling
      },
      error: (err: any) => {
        console.error('Status check failed:', err);
        this.error.set(err?.message ?? 'Failed to check extraction status');
        this.isExtracting.set(false);
        this.stopPolling();
      }
    });
  }

  pollExtractionStatus() {
    console.log('Starting polling for extraction status');
    // Check status immediately, then every 2 seconds
    this.checkExtractionStatus();
    
    const interval = setInterval(() => {
      const status = this.extractionStatus();
      if (!status || status.status === 'completed' || status.status === 'failed') {
        console.log('Stopping polling - extraction finished or failed');
        this.stopPolling();
        return;
      }
      console.log('Polling: checking status again...');
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
}
