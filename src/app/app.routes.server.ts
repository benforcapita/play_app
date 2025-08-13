import { RenderMode, ServerRoute } from '@angular/ssr';

// Avoid prerendering dynamic parameter routes; render them on the server instead.
export const serverRoutes: ServerRoute[] = [
  {
    path: 'characters/:id',
    renderMode: RenderMode.Server
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
