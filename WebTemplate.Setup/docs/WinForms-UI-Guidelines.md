# WinForms UI Guidelines (WebTemplate.Setup)

Authoritative guidance for building and maintaining the WinForms UI until we transition to a future UI stack. This is a living document—update it as we evolve. All rules apply unless explicitly justified in code review.

## Core Principles
- Separation of concerns: Forms/controls are thin views. No business logic in UI—delegate to services/presenters.
- Fail fast: Validate inputs and state explicitly. Do not use fallback/default shortcuts (no `??`, `||`, `.GetValueOrDefault()`).
- Async-first: Keep the UI responsive. Use async/await for I/O and long-running work.
- Consistency: Uniform naming, layout, and theming across all screens.
- Accessibility & Quality: Keyboard navigation, tab order, high contrast, high DPI support.

## Architecture
- Pattern: Passive View (MVP)
  - View: `Form`/`UserControl` raises events and exposes setters/getters for data.
  - Presenter: Consumes services, performs orchestration and validation, updates the view.
  - Services: File I/O, persistence, generation, validation logic.
- Dependencies:
  - Configure DI in `Program.cs` (`Microsoft.Extensions.DependencyInjection`). Resolve presenters/services and inject into forms.
  - Views should not create services directly—presenters do, or DI injects them.

## Async & UI Thread
- Use `async`/`await` for operations touching disk/process/network.
- Never block the UI thread (`Task.Wait`, `.Result`, long loops). Use `await Task.Run` only for CPU-bound work.
- UI updates must occur on the UI thread. Prefer `IProgress<T>` for reporting; update labels/logs/progress via `Invoke` if needed.
- Provide cancellation for long-running operations via `CancellationToken`.
- Disable relevant controls during operations; re-enable in `finally`.

## Validation & State
- Validate user inputs before actions. Aggregate errors and show a single dialog with all error messages.
- No fallbacks—if required data is missing, stop and show an actionable error.
- Centralize validation in services/presenters; views only surface messages.
- Dirty tracking: Use a uniform approach (e.g., `_isDirty` pattern) and update window title with `*`. Prompt before discarding changes.

## Layout & DPI
- Use `TableLayoutPanel` / `FlowLayoutPanel` with `Dock/Anchor`—avoid absolute positioning.
- Set `AutoScaleMode = Dpi` and test at 100%/150%/200% scaling.
- Minimum form sizes to prevent truncation; support resizing where appropriate.
- Keep margins/spacing consistent (8px base spacing; 16px group spacing).

## Theming & Styling
- Centralize theme (colors, fonts, paddings) in a `UITheme` helper. Apply in form constructors/on load.
- Prefer system-aware colors for high contrast. Ensure accessible contrast ratios.
- Use a consistent font (e.g., `Segoe UI, 9–10pt`); avoid per-control font overrides unless required.

## Accessibility
- Set meaningful `Text`, `AccessibleName`, and `AccessibleDescription` where appropriate.
- Logical tab order. Ensure all interactive controls are reachable via keyboard.
- Provide visual focus cues; avoid color-only signaling.

## Localization
- No hardcoded user-facing strings in code. Place strings in `.resx` resources under `WebTemplate.Setup`.
- Use `ResourceManager`/designer-generated properties for access. Default language: English.
- Ensure dialogs/messages compose using resources (titles, body text, button text when custom controls are used).

## Naming Conventions
- Controls: `btnSave`, `txtProjectName`, `cmbConfigurations`, `lblStatus`, `grpOptions`, `chkEnabled`, `lstItems`, `lvItems`, `dgvUsers`.
- Events: `BtnSave_Click`, `CmbConfigurations_SelectedIndexChanged`.
- Forms: `{Feature}Form` (e.g., `MainForm`).
- Controls: `{Feature}Control` (e.g., `ProjectSettingsControl`).
- Presenters: `{Feature}Presenter`.
- Services: `{Noun}Service`.

## Dialogs & Notifications
- Centralize message dialogs in a `NotificationService` (info/warn/error/confirm). Views call the service instead of `MessageBox.Show` directly.
- Use clear, actionable messages. Include next steps when possible.
- Log technical details to an internal log panel/file, not to end-user message boxes.

## Error Handling
- Catch at the boundary (presenter/service) and surface friendly messages. Include context (what failed). Do not swallow exceptions.
- Always clean up UI state using `try/finally`.
- Prefer typed results `(Success, Message, Data?)` or `Result<T>` classes over magic values/exceptions for expected failures.

## File/Folder Selection
- Use `using var dialog = new OpenFileDialog()/FolderBrowserDialog();` and validate the result.
- Pre-populate `InitialDirectory` when appropriate. Validate existence and permissions.

## Logging & Telemetry
- Provide an on-form log area for long operations (generation). Append timestamped lines.
- Keep logs concise; consider a rolling in-memory buffer. Avoid blocking I/O during logging.

## Reusability
- Extract repeated UI patterns into reusable `UserControl`s.
- Encapsulate repeated command patterns (disable buttons, try/await, progress, enable) in helper methods.

## Testing
- Place logic in presenters/services to enable unit tests in `Backend/WebTemplate.UnitTests`.
- Keep views simple; optionally smoke-test via UI automation if needed later.

## Security & Secrets
- Never log secrets or connection strings. Mask where unavoidable.
- When displaying paths/user input, sanitize and show minimal necessary information.

## Performance
- Avoid unnecessary layout passes: suspend/resume layout when adding many controls.
- Virtualize heavy lists (e.g., `ListView` virtual mode) when item counts are large.

## File/Project Structure
- Views: `WebTemplate.Setup/UI/*`
- Presenters: `WebTemplate.Setup/UI/Presenters/*`
- Services: `WebTemplate.Setup/Services/*`
- Models: `WebTemplate.Setup/Models/*`
- Resources: `WebTemplate.Setup/Resources/*`
- Docs: `WebTemplate.Setup/docs/*`

## Review Checklist (PRs)
- Separation: View thin; no business logic in code-behind.
- Async: No UI thread blocking; cancellation supported where long-running.
- Validation: Explicit; no fallbacks; clear error messages.
- Accessibility: Tab order, names, contrast checked.
- Localization: No hardcoded strings.
- Layout: DPI-safe; uses layout panels and docking.
- Theming: Uses centralized theme helpers.
- Tests: Presenter/service logic covered where applicable.

---

Notes
- Backward compatibility is not required; prefer cleaner designs and refactors.
- Prefer .NET 9 / C# 13 features when appropriate, but keep UI code simple and readable.
- Align with repository-wide guidance in `.github/copilot-instructions.md`.
