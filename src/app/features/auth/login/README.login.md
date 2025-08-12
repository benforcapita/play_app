### Feature Pages Implemented

- Login: Minimal elegant form that calls `AuthService.login` and navigates to `'/characters'` on success. Errors are shown inline.
- Character List: Lists characters from `CharactersService.list()`, links to details, and includes Logout and Extract shortcuts.
- Character Detail: Loads by `id` from route params using `CharactersService.get(id)` and renders key fields.
- Extract: UI-only page to select a file and preview file name. No API calls implemented by design.

### Auth and Routing
- All non-login routes use `canActivateAuth` to require a valid token.
- `authInterceptor` automatically attaches the Bearer token to API calls except `api/auth/*`.
- `app.config.ts` prefixes relative HTTP URLs with `environment.apiUiBaseUrl` via `apiBaseUrlInterceptor`.

### Tests
- Unit tests added for login, list, detail, and extract pages.
- Extract tests validate only UI behavior (no heavy extraction flows).

### How to Use
- Navigate to `/login`, sign in, then use the header controls to refresh, extract, or logout.