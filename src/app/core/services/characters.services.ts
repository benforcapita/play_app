import { inject , Injectable} from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Character,SheetSectionKey,CharacterSheet,ExtractionJobResponse,ExtractionJobStatus,ExtractionResult } from "../models/character.models";


@Injectable({
    providedIn: 'root'
})
export class CharactersService {
    private http = inject(HttpClient);

    list() {
        return this.http.get<Character[]>('api/characters');
    }
    
    get(id:number) {
        return this.http.get<Character>(`api/characters/${id}`);
    }
    
    create(character:Character) {
        return this.http.post<Character>('api/characters',character);
    }
    
    update(id:number,character:Character) {
        return this.http.put<Character>(`api/characters/${id}`,character);
    }
    
    delete(id:number) {
        return this.http.delete(`api/characters/${id}`);
    }
    
    getSheet(id:number,section:SheetSectionKey) {
        return this.http.get<CharacterSheet>(`api/characters/${id}/sheet/${section}`);
    }

    // Extraction methods
    startExtraction(file: File) {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<ExtractionJobResponse>('api/extract/characters', formData);
    }
    
    getExtractionStatus(jobToken: string) {
        return this.http.get<ExtractionJobStatus>(`api/extract/jobs/${jobToken}/status`);
    }
    
    getExtractionResult(jobToken: string) {
        return this.http.get<ExtractionResult>(`api/extract/jobs/${jobToken}/result`);
    }
}