# Centralized UI Theme - Implementation Guide

**Date**: November 13, 2025
**Status**: ✅ Active and Implemented

---

## 🎯 Overview

The **UITheme** class is the **single source of truth** for all visual properties in WebTemplate.Setup. Every color, font, spacing, and size value comes from this centralized configuration.

---

## 📍 Location

`WebTemplate.Setup/UI/UITheme.cs`

---

## 🎨 What's Centralized

### 1. **Colors** (`UITheme.Colors`)

All application colors are defined here:

```csharp
// Background colors
UITheme.Colors.WindowBackground
UITheme.Colors.PanelBackground
UITheme.Colors.GroupBoxBackground

// Text colors
UITheme.Colors.PrimaryText
UITheme.Colors.SecondaryText
UITheme.Colors.DisabledText

// Status colors
UITheme.Colors.Success    // Dark Green
UITheme.Colors.Error      // Dark Red
UITheme.Colors.Warning    // Orange
UITheme.Colors.Info       // Blue

// Control colors
UITheme.Colors.TextBoxBackground
UITheme.Colors.ButtonFace
```

**Example Usage:**
```csharp
lblStatus.ForeColor = UITheme.Colors.Success;  // Green text for success
lblError.ForeColor = UITheme.Colors.Error;     // Red text for errors
```

---

### 2. **Fonts** (`UITheme.Fonts`)

All font specifications come from the theme:

```csharp
UITheme.Fonts.Default           // Segoe UI, 9pt, Regular
UITheme.Fonts.DefaultBold       // Segoe UI, 9pt, Bold
UITheme.Fonts.Header            // Segoe UI, 10pt, Bold
UITheme.Fonts.SubHeader         // Segoe UI, 9.75pt, Bold
UITheme.Fonts.Small             // Segoe UI, 8.25pt, Regular
UITheme.Fonts.StatusBar         // Segoe UI, 8.25pt, Regular
UITheme.Fonts.Monospace         // Consolas, 9pt (for code/secrets)
```

**Example Usage:**
```csharp
lblHeader.Font = UITheme.Fonts.Header;
groupBox.Font = UITheme.Fonts.DefaultBold;
txtSecretKey.Font = UITheme.Fonts.Monospace;  // For base64 strings
```

---

### 3. **Spacing** (`UITheme.Spacing`)

Consistent spacing and padding values:

```csharp
// Margins
UITheme.Spacing.ControlMargin    // 3, 3, 3, 3
UITheme.Spacing.FormMargin       // 10, 10, 10, 10

// Padding
UITheme.Spacing.GroupBoxPadding  // 10, 10, 10, 10
UITheme.Spacing.TabPadding       // 10, 10, 10, 10

// Gaps (constants)
UITheme.Spacing.SmallGap         // 3px
UITheme.Spacing.MediumGap        // 6px
UITheme.Spacing.LargeGap         // 10px
UITheme.Spacing.SectionGap       // 15px
```

**Example Usage:**
```csharp
this.Padding = UITheme.Spacing.TabPadding;
groupBox.Padding = UITheme.Spacing.GroupBoxPadding;
```

---

### 4. **Sizes** (`UITheme.Sizes`)

Standard control sizes:

```csharp
// Buttons
UITheme.Sizes.ButtonSmall        // 75 x 23
UITheme.Sizes.ButtonMedium       // 100 x 30
UITheme.Sizes.ButtonLarge        // 120 x 35

// Input controls
UITheme.Sizes.TextBoxWidth       // 250px
UITheme.Sizes.TextBoxWidthLarge  // 400px

// Labels
UITheme.Sizes.LabelWidthShort    // 80px
UITheme.Sizes.LabelWidthMedium   // 120px
UITheme.Sizes.LabelWidthLong     // 180px
```

**Example Usage:**
```csharp
btnSave.Size = UITheme.Sizes.ButtonMedium;
txtProjectName.Width = UITheme.Sizes.TextBoxWidth;
```

---

### 5. **Form Settings** (`UITheme.FormSettings`)

Form dimensions and positioning:

```csharp
UITheme.FormSettings.MainFormMinimumSize   // 1000 x 700
UITheme.FormSettings.MainFormDefaultSize   // 1200 x 800
UITheme.FormSettings.DialogSmall           // 400 x 250
UITheme.FormSettings.DefaultStartPosition  // CenterScreen
```

**Example Usage:**
```csharp
this.MinimumSize = UITheme.FormSettings.MainFormMinimumSize;
this.Size = UITheme.FormSettings.MainFormDefaultSize;
this.StartPosition = UITheme.FormSettings.DefaultStartPosition;
```

---

## 🛠️ How to Use

### In MainForm (Already Implemented)

```csharp
public MainForm(...)
{
    InitializeComponent();

    // Apply theme FIRST, before any other initialization
    ApplyTheme();

    InitializeTabControls();
    // ... rest of initialization
}

private void ApplyTheme()
{
    // Apply theme to entire form and all controls
    UITheme.Apply(this);

    // Set form-specific properties from theme
    this.MinimumSize = UITheme.FormSettings.MainFormMinimumSize;
    this.Size = UITheme.FormSettings.MainFormDefaultSize;

    // Apply to status bar
    statusStrip.Font = UITheme.Fonts.StatusBar;
    statusStrip.Height = UITheme.StatusBarSettings.Height;

    // Apply to toolbar
    toolStrip.ImageScalingSize = UITheme.ToolbarSettings.IconSize;
}
```

---

### In Tab Controls (UserControl)

**Pattern Applied to:**
- ✅ `SecretsControl.cs` - Monospace font for Key Vault URL
- ✅ `AuthControl.cs` - Monospace font for JWT secret key
- ✅ `DatabaseControl.cs` - Monospace font for connection string

**Example Implementation:**

```csharp
public partial class AuthControl : UserControl
{
    public AuthControl()
    {
        InitializeComponent();
        ApplyTheme();        // Apply theme
        WireUpEvents();      // Then wire up events
    }

    private void ApplyTheme()
    {
        // Secret key uses monospace font (it's base64)
        UITheme.ApplyMonospaceFont(txtSecretKey);

        // Set consistent tab padding
        this.Padding = UITheme.Spacing.TabPadding;

        // GroupBoxes use theme padding and bold font
        if (grpSettings != null)
        {
            grpSettings.Padding = UITheme.Spacing.GroupBoxPadding;
            grpSettings.Font = UITheme.Fonts.DefaultBold;
        }
    }

    // ... rest of control logic
}
```

---

## 🎨 Theme Helper Methods

### 1. Apply Theme to Form/Control

```csharp
// Apply to entire form
UITheme.Apply(this);  // Form

// Apply to UserControl
UITheme.Apply(_projectControl);  // UserControl
```

This recursively applies the theme to all child controls.

---

### 2. Apply Monospace Font

For connection strings, secret keys, base64 values, JSON, etc.:

```csharp
UITheme.ApplyMonospaceFont(txtConnectionString);
UITheme.ApplyMonospaceFont(txtSecretKey);
UITheme.ApplyMonospaceFont(txtKeyVaultUrl);
```

**Result**: Uses `Consolas 9pt` for better readability of technical strings.

---

### 3. Make Section Header

For labels that act as section headers:

```csharp
UITheme.MakeSectionHeader(lblDatabaseSettings);
```

**Result**: Uses `Segoe UI 10pt Bold` with primary text color.

---

### 4. Status Messages with Colors

```csharp
var (message, color) = UITheme.CreateStatusMessage(
    "Configuration saved successfully",
    UITheme.StatusType.Success);

lblStatus.Text = message;
lblStatus.ForeColor = color;  // Green
```

**Or use MainForm helper methods:**

```csharp
ShowSuccessStatus("Configuration saved!");     // Green
ShowErrorStatus("Failed to load config");      // Red
ShowWarningStatus("Validation warnings");      // Orange
ShowInfoStatus("Loading configuration...");    // Blue
ShowNormalStatus("Ready");                     // Default
```

---

## 🔄 How It Works

### Automatic Theme Application

When `UITheme.Apply()` is called:

1. **Form/UserControl** gets base theme applied
2. **All child controls** are recursively themed based on their type:
   - `GroupBox` → Bold font + padding
   - `Panel` → Padding
   - `Label` → Default font + primary text color
   - `TextBox` → Default font + proper background
   - `Button` → Default font + button face color
   - `CheckBox/RadioButton` → Default font + primary text color

3. **Special controls** can override with specific theming:
   ```csharp
   UITheme.ApplyMonospaceFont(txtConnectionString);
   ```

---

## 📊 Current Implementation Status

| Component | Theme Applied | Notes |
|-----------|---------------|-------|
| MainForm | ✅ Yes | Full theme + form sizing |
| Status Bar | ✅ Yes | Font, height, padding |
| Toolbar | ✅ Yes | Icon sizing |
| Tab Controls (All) | ✅ Yes | Auto-applied from MainForm |
| SecretsControl | ✅ Yes | + Monospace for Key Vault URL |
| AuthControl | ✅ Yes | + Monospace for secret key |
| DatabaseControl | ✅ Yes | + Monospace for connection string |
| ProjectSettingsControl | ✅ Yes | Auto from MainForm.Apply() |
| FeaturesControl | ✅ Yes | Auto from MainForm.Apply() |
| ServerControl | ✅ Yes | Auto from MainForm.Apply() |

---

## 🎯 Benefits

### 1. **Single Source of Truth**
- Change one value → entire app updates
- No hardcoded colors, fonts, or sizes scattered in code

### 2. **Easy Theming**
Want to change the entire look? Update values in `UITheme.cs`:

```csharp
// Make all text larger
public static readonly Font Default = new("Segoe UI", 10F, ...);  // Was 9F

// Change success color
public static readonly Color Success = Color.Green;  // Was Color.FromArgb(0, 128, 0)

// Increase spacing
public const int LargeGap = 15;  // Was 10
```

### 3. **Migration Ready**
When we move to WPF/Avalonia/Blazor:
- Copy the theme values to new UI framework
- Colors, fonts, spacing translate directly
- No need to hunt through designer files

### 4. **Consistency Enforced**
- All controls automatically get proper styling
- No accidental inconsistencies
- Professional, uniform appearance

---

## 📝 Adding New Theme Properties

### Example: Adding a "Danger" Button Color

1. **Add to UITheme.cs:**
```csharp
public static class Colors
{
    // ... existing colors
    public static readonly Color DangerButton = Color.FromArgb(220, 53, 69);  // Bootstrap danger red
}
```

2. **Use in controls:**
```csharp
btnDelete.BackColor = UITheme.Colors.DangerButton;
btnDelete.ForeColor = Color.White;
```

3. **That's it!** Now you can reuse it everywhere.

---

## 🔮 Future Enhancements

Potential additions to the theme system:

1. **Dark Mode Support**
   ```csharp
   public static class DarkTheme
   {
       public static readonly Color WindowBackground = Color.FromArgb(32, 32, 32);
       // ... dark variants of all colors
   }
   ```

2. **Theme Presets**
   ```csharp
   UITheme.ApplyPreset(ThemePreset.Light);
   UITheme.ApplyPreset(ThemePreset.Dark);
   UITheme.ApplyPreset(ThemePreset.HighContrast);
   ```

3. **User-Configurable Themes**
   - Load theme from settings file
   - Allow users to customize colors
   - Export/import themes

4. **Animation Timings**
   ```csharp
   public static class Animations
   {
       public const int FadeInDuration = 200;      // ms
       public const int SlideInDuration = 300;     // ms
   }
   ```

---

## ✅ Best Practices

### DO ✅

```csharp
// Use theme properties
lblError.ForeColor = UITheme.Colors.Error;
this.Padding = UITheme.Spacing.TabPadding;
btnSave.Size = UITheme.Sizes.ButtonMedium;

// Apply theme in constructor
public MyControl()
{
    InitializeComponent();
    ApplyTheme();  // Always apply theme
}

// Use helper methods
UITheme.ApplyMonospaceFont(txtConnectionString);
ShowSuccessStatus("Operation completed");
```

### DON'T ❌

```csharp
// Hardcode colors
lblError.ForeColor = Color.Red;  // ❌ Use UITheme.Colors.Error

// Hardcode fonts
txtSecretKey.Font = new Font("Consolas", 9F);  // ❌ Use UITheme.ApplyMonospaceFont()

// Hardcode sizes
this.Padding = new Padding(10, 10, 10, 10);  // ❌ Use UITheme.Spacing.TabPadding

// Skip theme application
public MyControl()
{
    InitializeComponent();
    // ❌ Missing: ApplyTheme();
}
```

---

## 🎓 Summary

The centralized UITheme system provides:

- **Colors**: Status, text, background, borders
- **Fonts**: Default, bold, headers, monospace
- **Spacing**: Margins, padding, gaps
- **Sizes**: Buttons, inputs, labels
- **Forms**: Window sizes, positioning

**Key Methods:**
- `UITheme.Apply(form/control)` - Apply to everything
- `UITheme.ApplyMonospaceFont(textBox)` - For technical strings
- `UITheme.CreateStatusMessage(msg, type)` - Colored status messages

**Result:** Consistent, maintainable, migration-ready UI that's beautiful and professional! 🎨✨

---

**Everything is now centralized. Change one place, affect the entire application!**
