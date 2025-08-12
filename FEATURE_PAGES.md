## Implemented Feature Pages

- **Login Page** (`src/app/features/auth/login/login.page.ts`):
  - Minimal, elegant form using `AuthService.login(email, password)`
  - On success navigates to `'/characters'`; shows inline errors on failure
- **Character List Page** (`src/app/features/character/character-list/character-list.page.ts`):
  - Loads via `CharactersService.list()` and displays cards
  - Links to detail pages; includes buttons for Refresh, Extract, Logout
- **Character Detail Page** (`src/app/features/character/character-detail/character-detail.page.ts`):
  - Reads `id` from route params; loads with `CharactersService.get(id)`
  - Shows key fields in a simple card
- **Extract Page** (`src/app/features/extract/extract.page.ts`):
  - UI-only file selector that shows chosen filename
  - No heavy extraction logic or API calls by design

## Routing and Auth
- Routes wired in `src/app/app.routes.ts`:
  - `login` is public
  - `characters`, `characters/:id`, and `extracts` are protected by `canActivateAuth`
- `auth.interceptor.ts` attaches `Authorization: Bearer <token>` to all API requests except `api/auth/*`
- `app.config.ts` prefixes relative HTTP URLs with `environment.apiUiBaseUrl`

## Tests
- Added unit tests:
  - `src/app/features/auth/login/login.page.spec.ts`
  - `src/app/features/character/character-list/character-list.page.spec.ts`
  - `src/app/features/character/character-detail/character-detail.page.spec.ts`
  - `src/app/features/extract/extract.page.spec.ts` (UI-only)
- Extract tests intentionally avoid heavy operations (UI only)

## Run
- Dev server: `npm start`
- Unit tests (suggested): `npx @angular/cli@20.1.4 test --browsers=jsdom --watch=false`
- E2E: `npm run test:e2e`

## Notes
- No changes to existing architecture; only new standalone components and tests were added.
- UI design aims to be elegant with minimal extra code and styles inline within components.