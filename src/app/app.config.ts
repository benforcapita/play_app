import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { inject, InjectionToken } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptors, HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../enviroments/enviroment';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideServiceWorker } from '@angular/service-worker';

// Global API base URL token and interceptor so all relative HTTP calls target the same host
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  // If URL is absolute (http/https) or an assets request, leave it unchanged
  const isAbsolute = /^https?:\/\//i.test(req.url);
  if (isAbsolute || req.url.startsWith('assets/')) {
    return next(req);
  }

  const base = inject(API_BASE_URL);
  const url = req.url.startsWith('/') ? `${base}${req.url}` : `${base}/${req.url}`;
  return next(req.clone({ url }));
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000'
    }),
    // Provide a single HttpClient for the whole app and prefix relative URLs with the API base
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, apiBaseUrlInterceptor])
    ),

    // Make the base URL configurable per environment
    { provide: API_BASE_URL, useValue: environment.apiUiBaseUrl }
  ]
};
