import { inject , Injectable} from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Character,SheetSectionKey,CharacterSheet,ExtractionJobResponse,ExtractionJobStatus,ExtractionResult } from "../models/character.models";
import { catchError, tap } from "rxjs/operators";
import { throwError } from "rxjs";

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
        console.log('Starting extraction for file:', {
            name: file.name,
            size: file.size,
            type: file.type,
            lastModified: file.lastModified
        });

        const formData = new FormData();
        formData.append('file', file);
        
        console.log('FormData created with file:', {
            formDataEntries: Array.from(formData.entries()).map(([key, value]) => ({
                key,
                valueType: typeof value,
                valueName: value instanceof File ? value.name : 'not a file'
            }))
        });

        return this.http.post<ExtractionJobResponse>('api/extract/characters', formData).pipe(
            tap(response => {
                console.log('Extraction started successfully:', response);
            }),
            catchError((error: HttpErrorResponse) => {
                console.error('Extraction request failed:', {
                    status: error.status,
                    statusText: error.statusText,
                    message: error.message,
                    error: error.error,
                    url: error.url,
                    name: error.name,
                    ok: error.ok,
                    type: error.type,
                    headers: error.headers?.keys()
                });
                
                // Log additional details for debugging
                if (error.status === 0) {
                    console.error('Network error - possible causes:', {
                        cors: 'CORS policy blocking request',
                        network: 'Network connectivity issue',
                        server: 'Server not responding',
                        ssl: 'SSL/TLS certificate issue',
                        timeout: 'Request timeout'
                    });
                }
                
                return throwError(() => error);
            })
        );
    }
    
    getExtractionStatus(jobToken: string) {
        console.log('Checking extraction status for job token:', jobToken);
        
        return this.http.get<ExtractionJobStatus>(`api/extract/jobs/${jobToken}/status`).pipe(
            tap(response => {
                console.log('Extraction status response:', response);
            }),
            catchError((error: HttpErrorResponse) => {
                console.error('Status check failed:', {
                    jobToken,
                    status: error.status,
                    statusText: error.statusText,
                    message: error.message,
                    error: error.error
                });
                return throwError(() => error);
            })
        );
    }
    
    getExtractionResult(jobToken: string) {
        console.log('Getting extraction result for job token:', jobToken);
        
        return this.http.get<ExtractionResult>(`api/extract/jobs/${jobToken}/result`).pipe(
            tap(response => {
                console.log('Extraction result response:', response);
            }),
            catchError((error: HttpErrorResponse) => {
                console.error('Result retrieval failed:', {
                    jobToken,
                    status: error.status,
                    statusText: error.statusText,
                    message: error.message,
                    error: error.error
                });
                return throwError(() => error);
            })
        );
    }
}