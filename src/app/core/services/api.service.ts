import { inject, Inject, Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private http = inject(HttpClient);

    getPing() {
        return this.http.get('/ping');
    }

    getHealth() {
        return this.http.get('/health');
    }
}