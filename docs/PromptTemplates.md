## Prompt Template Management (Admin)

This module lets admins manage LLM prompt templates without code changes and view usage logs.

### Data Model
- `PromptTemplate`: `Id`, `Key`, `Title`, `Locale`, `ProviderHint`, `Content`, `OutputFormatHint`, `IsActive`, `Description`, `UpdatedAt`, `UpdatedBy`.
- `PromptCallLog`: links to template and records provider/model, duration (ms), token usage, success/error, summary, `IsTest`.

### Backend APIs (Admin only)
- `GET /api/prompts?locale=&includeInactive=` – list templates.
- `GET /api/prompts/{id}` – get one.
- `POST /api/prompts` – create.
- `PUT /api/prompts/{id}` – update (key/locale can change).
- `DELETE /api/prompts/{id}` – delete.
- `POST /api/prompts/{id}/clone` – duplicate with optional overrides.
- `POST /api/prompts/{key}/test` – render + run a test call with sample data; logs as `IsTest=true`.
- `GET /api/prompts/logs?take=50` – recent usage.

### Template Resolution & Defaults
- Templates are cached in-memory (10 minutes). Cache is invalidated on create/update/delete/clone.
- If DB is empty, built-in seeds are stored automatically (general/story/tips variants).
- When a prompt key isn’t provided, persona content generation falls back by scenario (`general`, `story`, `tips`), then locale, then default seed.
- `ProviderHint` can steer which provider to use when the caller doesn’t specify one.

### Logging
- Persona content generation and test calls log to `PromptCallLog` with template key/ID, provider/model, duration, and summary/error.
- Logs are accessible from the admin UI and `GET /api/prompts/logs`.

### Admin UI (client)
- Navigate to **Admin → Prompt Templates**.
- Create/edit templates (key, locale, provider hint, active toggle, content, output hint, description).
- Clone existing templates, run test calls, and preview prompt/response.
- View recent call logs (template key, model, duration, success/error, summary).
