# WebTemplate.Setup WinForms UI Guidelines
**Version**: 1.0
**Date**: November 13, 2025
**Status**: Active - This is our UI development bible

---

## 🎯 Purpose

This document establishes strict UI guidelines for WebTemplate.Setup WinForms development. These rules ensure:
- **Consistency** across all UI components
- **Maintainability** for current development
- **Future-proofing** for migration to other UI technologies (WPF, Avalonia, Blazor, etc.)
- **Clean separation** of concerns (UI ↔ Business Logic ↔ Data)

---

## ⚠️ Critical Principles

### 1. **WinForms is Temporary**
- WinForms was chosen for **time constraints only**
- Expect migration to modern UI framework in the future
- Design with **portability in mind**

### 2. **Zero Business Logic in UI**
- UI components are **dumb views only**
- All validation, processing, and decisions belong in Services or Models
- UI only: display data, capture input, raise events

### 3. **Event-Driven Architecture**
- UI communicates via events (e.g., `SettingsChanged`, `ValidationRequested`)
- Services/Models never directly reference UI components
- Maintain strict **one-way data flow**

---

## 📐 Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                        MainForm                              │
│  - Orchestrates all controls                                │
│  - Manages configuration lifecycle (Load/Save/Delete)        │
│  - Handles toolbar actions                                   │
│  - Delegates to Services for business logic                  │
└─────────────────────────────────────────────────────────────┘
                           │
        ┌──────────────────┴──────────────────┐
        │                                     │
┌───────▼────────┐                   ┌────────▼────────┐
│  Tab Controls  │                   │    Services     │
│  (UserControl) │                   │  - Persistence  │
│  - Bind data   │◄──────────────────┤  - Generation   │
│  - Raise events│                   │  - Database     │
│  - No logic    │                   │  - Secrets      │
└────────────────┘                   └─────────────────┘
                                              │
                                     ┌────────▼────────┐
                                     │     Models      │
                                     │  - Configuration│
                                     │  - Validation   │
                                     │  - Data only    │
                                     └─────────────────┘
```

---

## 🎨 UI Component Standards

### Tab Control Pattern (UserControl)

Every tab control **MUST** follow this exact structure:

```csharp
public partial class [Feature]Control : UserControl
{
    // ══════════════════════════════════════════════════════
    // EVENTS - How UI communicates with parent
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Raised when any setting changes (for dirty tracking)
    /// </summary>
    public event EventHandler? SettingsChanged;

    // ══════════════════════════════════════════════════════
    // CONSTRUCTOR - Wire up events only
    // ══════════════════════════════════════════════════════

    public [Feature]Control()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Wire ALL control events to raise SettingsChanged
        txtSomething.TextChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        chkSomething.CheckedChanged += (s, e) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        // ... etc
    }

    // ══════════════════════════════════════════════════════
    // DATA BINDING - REQUIRED METHODS
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Load configuration data into UI controls
    /// </summary>
    public void LoadSettings([Configuration]Model config)
    {
        // ONLY: Model → UI controls
        // NO validation, NO transformation, NO logic
        txtField.Text = config.Field;
        chkEnabled.Checked = config.Enabled;
    }

    /// <summary>
    /// Save UI control values back to configuration model
    /// </summary>
    public void SaveSettings([Configuration]Model config)
    {
        // ONLY: UI controls → Model
        // NO validation, NO transformation, NO logic
        config.Field = txtField.Text;
        config.Enabled = chkEnabled.Checked;
    }

    // ══════════════════════════════════════════════════════
    // UI STATE MANAGEMENT - Simple enable/disable only
    // ══════════════════════════════════════════════════════

    private void UpdateControlStates()
    {
        // ONLY: Enable/disable controls based on checkbox state
        // NO validation, NO business logic
        grpOptions.Enabled = chkEnabled.Checked;
    }
}
```

### Key Rules for Tab Controls:

1. **No Business Logic**
   - ❌ NO validation in tab controls
   - ❌ NO data transformation
   - ❌ NO calculations
   - ✅ ONLY UI state management (enable/disable)

2. **Required Methods**
   - `LoadSettings([Config] config)` - Model to UI
   - `SaveSettings([Config] config)` - UI to Model
   - Both methods take the config **as a parameter**

3. **Event Handling**
   - Raise `SettingsChanged` for **every** user interaction
   - Use lambda expressions for simple event wire-ups
   - Keep event handlers minimal

4. **Naming Conventions**
   - Control class: `[Feature]Control.cs`
   - Event: `SettingsChanged`
   - Methods: `LoadSettings()`, `SaveSettings()`

---

## 🏗️ MainForm Responsibilities

MainForm is the **orchestrator** and handles:

### 1. Tab Control Lifecycle
```csharp
private void InitializeTabControls()
{
    // Create all tab controls
    _projectControl = new ProjectSettingsControl { Dock = DockStyle.Fill };
    _featuresControl = new FeaturesControl { Dock = DockStyle.Fill };
    // ... etc

    // Add to TabPages
    tabProject.Controls.Add(_projectControl);
    tabFeatures.Controls.Add(_featuresControl);
    // ... etc

    // Wire up events
    _projectControl.SettingsChanged += (s, e) => MarkDirty();
    _featuresControl.SettingsChanged += (s, e) => MarkDirty();
    // ... etc
}
```

### 2. Configuration Management
```csharp
private void BindConfigurationToUI()
{
    // Load configuration into all tab controls
    _projectControl?.LoadSettings(_currentConfiguration.Project);
    _featuresControl?.LoadSettings(_currentConfiguration.Features);
    // ... etc
}

private void BindUIToConfiguration()
{
    // Save UI values back to configuration model
    _projectControl?.SaveSettings(_currentConfiguration.Project);
    _featuresControl?.SaveSettings(_currentConfiguration.Features);
    // ... etc
}
```

### 3. Toolbar Actions
- **New**: Create new configuration, clear UI
- **Save**: Validate via Model, call PersistenceService
- **Load**: Call PersistenceService, bind to UI
- **Delete**: Confirm, call PersistenceService
- **Generate**: Validate, call ProjectGenerationService

### 4. Validation
```csharp
private async Task<bool> ValidateConfiguration()
{
    if (_currentConfiguration == null)
        return false;

    // 1. Bind UI to config
    BindUIToConfiguration();

    // 2. Let MODEL validate (NOT UI!)
    var validationResult = _currentConfiguration.Validate();

    // 3. Display errors if any
    if (!validationResult.IsValid)
    {
        MessageBox.Show(string.Join("\n", validationResult.Errors),
            "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    return validationResult.IsValid;
}
```

---

## 📋 Form Designer Standards

### Control Naming Conventions

| Control Type | Prefix | Example |
|--------------|--------|---------|
| TextBox | `txt` | `txtProjectName` |
| CheckBox | `chk` | `chkEnabled` |
| RadioButton | `rdo` | `rdoUserSecrets` |
| Button | `btn` | `btnBrowse` |
| Label | `lbl` | `lblStatus` |
| ComboBox | `cbo` | `cboStrategy` |
| NumericUpDown | `num` | `numTimeout` |
| GroupBox | `grp` | `grpOptions` |
| Panel | `pnl` | `pnlKeyVault` |
| TabControl | `tab` | `tabMain` |
| TabPage | `tab` | `tabProject` |

### Layout Guidelines

1. **Consistent Spacing**
   - Margin: `3, 3, 3, 3` (standard)
   - Padding: `10, 10, 10, 10` (for GroupBox/Panel)
   - Control spacing: 6-10 pixels vertically

2. **Font Standards**
   - Default: `Segoe UI, 9pt`
   - Headers: `Segoe UI, 10pt, Bold`
   - Status: `Segoe UI, 8.25pt`

3. **Control Sizes**
   - TextBox height: Use default (23px)
   - Button size: 75x23 (small), 100x30 (medium), 120x35 (large)
   - Label width: Auto-size preferred, or match TextBox width

4. **Anchoring**
   - Top-left controls: `Anchor = Top, Left`
   - Full-width controls: `Anchor = Top, Left, Right`
   - Buttons (bottom-right): `Anchor = Bottom, Right`

---

## 🚫 Prohibited Practices

### ❌ NEVER Do This:

```csharp
// ❌ Business logic in UI
public void SaveSettings(ServerConfiguration config)
{
    // DON'T validate here!
    if (string.IsNullOrEmpty(txtUrl.Text))
        throw new Exception("URL required");

    config.BaseUrl = txtUrl.Text;
}

// ❌ Data transformation in UI
public void LoadSettings(AuthConfiguration config)
{
    // DON'T transform data here!
    txtExpiration.Text = (config.ExpirationMinutes / 60).ToString();
}

// ❌ Direct service calls in controls
private void btnSave_Click(object sender, EventArgs e)
{
    // DON'T call services from tab controls!
    var service = new ConfigurationPersistenceService();
    service.SaveConfiguration(...);
}

// ❌ Complex logic in event handlers
private void chkEnabled_CheckedChanged(object sender, EventArgs e)
{
    // DON'T validate or transform here!
    if (chkEnabled.Checked && string.IsNullOrEmpty(txtField.Text))
    {
        MessageBox.Show("Field required!");
        chkEnabled.Checked = false;
    }
}
```

### ✅ DO This Instead:

```csharp
// ✅ Let Model validate
var result = config.Validate();
if (!result.IsValid)
    ShowErrors(result.Errors);

// ✅ Store data as-is, transform in Model
public void LoadSettings(AuthConfiguration config)
{
    // Model provides display-ready value
    numExpiration.Value = config.ExpirationMinutes;
}

// ✅ Raise events, let MainForm handle
private void btnAction_Click(object sender, EventArgs e)
{
    ActionRequested?.Invoke(this, EventArgs.Empty);
}

// ✅ Simple state management only
private void chkEnabled_CheckedChanged(object sender, EventArgs e)
{
    grpOptions.Enabled = chkEnabled.Checked;
    SettingsChanged?.Invoke(this, EventArgs.Empty);
}
```

---

## 🔄 Data Flow Rules

### Configuration Loading (Model → UI)
```
1. User clicks "Load"
2. MainForm calls ConfigurationPersistenceService.LoadConfiguration()
3. Service returns WorkspaceConfiguration (model)
4. MainForm calls control.LoadSettings(config.Section)
5. Control populates UI controls from model properties
```

### Configuration Saving (UI → Model)
```
1. User clicks "Save"
2. MainForm calls control.SaveSettings(config.Section)
3. Control updates model properties from UI controls
4. MainForm calls Model.Validate()
5. If valid, MainForm calls ConfigurationPersistenceService.SaveConfiguration()
```

### Key Principle: **Models are the source of truth, UI is just a view**

---

## 🎨 Visual Consistency

### Color Scheme
- Background: `SystemColors.Control` (default)
- Disabled controls: `SystemColors.GrayText`
- Error indicators: `Color.Red` (text only, no background colors)
- Success indicators: `Color.Green` (text only)

### Icons & Images
- Use sparingly in WinForms (migration consideration)
- Toolbar icons: 16x16 or 24x24
- Store in Resources.resx for easy replacement

### Status Messages
- Success: Display in status bar (green text)
- Info: Display in status bar (default text)
- Warning: MessageBox with Warning icon
- Error: MessageBox with Error icon

---

## 📦 Future Migration Preparation

### Design Decisions for Easy Migration

1. **No WinForms-Specific Logic**
   - Avoid `Control.Invoke()` patterns
   - Avoid complex data binding
   - Avoid nested event subscriptions

2. **MVVM-Ready Structure**
   - Models are already pure data
   - Controls are already dumb views
   - Easy to add ViewModels later

3. **Service Layer is UI-Agnostic**
   - Services return models, not void
   - Services never reference System.Windows.Forms
   - Services use standard exceptions, not MessageBox

4. **Configuration as POCO**
   - All configuration classes are plain C# objects
   - No UI attributes or dependencies
   - JSON serialization works out of the box

### Migration Path
```
Current: WinForms → Services → Models
Future:  [Modern UI] → ViewModels → Services → Models
                          ↑
                    (Reuse 100%)
```

---

## 📝 Code Review Checklist

Before committing UI code, verify:

- [ ] No business logic in UserControl
- [ ] No validation in UserControl
- [ ] No data transformation in UserControl
- [ ] `LoadSettings()` and `SaveSettings()` implemented
- [ ] `SettingsChanged` event raised on all input changes
- [ ] Control naming follows conventions
- [ ] No direct service calls from tab controls
- [ ] No MessageBox in tab controls (only in MainForm)
- [ ] No complex logic in event handlers
- [ ] Consistent spacing and layout
- [ ] Dock property set correctly
- [ ] Anchoring used appropriately

---

## 🛠️ Standard Patterns

### Pattern 1: Enable/Disable Panel Based on Checkbox
```csharp
private void chkFeature_CheckedChanged(object sender, EventArgs e)
{
    grpFeatureOptions.Enabled = chkFeature.Checked;
    SettingsChanged?.Invoke(this, EventArgs.Empty);
}
```

### Pattern 2: Browse for Folder
```csharp
private void btnBrowse_Click(object sender, EventArgs e)
{
    using var dialog = new FolderBrowserDialog
    {
        Description = "Select target folder",
        UseDescriptionForTitle = true,
        ShowNewFolderButton = true,
        SelectedPath = txtPath.Text
    };

    if (dialog.ShowDialog() == DialogResult.OK)
    {
        txtPath.Text = dialog.SelectedPath;
    }
}
```

### Pattern 3: Generate Secure Value
```csharp
private void btnGenerate_Click(object sender, EventArgs e)
{
    txtSecretKey.Text = Convert.ToBase64String(
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
    SettingsChanged?.Invoke(this, EventArgs.Empty);
}
```

### Pattern 4: Multi-Value Input (Comma-Separated)
```csharp
// LoadSettings
txtOrigins.Text = config.AllowedOrigins != null
    ? string.Join(", ", config.AllowedOrigins)
    : string.Empty;

// SaveSettings
config.AllowedOrigins = txtOrigins.Text
    .Split(',')
    .Select(s => s.Trim())
    .Where(s => !string.IsNullOrEmpty(s))
    .ToArray();
```

---

## 📚 References

### Related Documents
- `ARCHITECTURE.md` - Overall solution architecture
- `PROJECT_INSTRUCTIONS.md` - Project-wide guidelines
- `WebTemplate.Setup/Models/*.cs` - Configuration models
- `WebTemplate.Setup/Services/*.cs` - Business logic services

### Key Files
- `MainForm.cs` - Main orchestrator
- `UI/*Control.cs` - Tab control implementations
- `Models/WorkspaceConfiguration.cs` - Master configuration model

---

## ✅ Compliance

**This document is MANDATORY** for all WebTemplate.Setup UI development.

**Violations** of these guidelines must be corrected before merging code.

**Updates** to this document require team review and version increment.

---

**Remember**: WinForms is temporary, but good architecture is forever! 🚀
