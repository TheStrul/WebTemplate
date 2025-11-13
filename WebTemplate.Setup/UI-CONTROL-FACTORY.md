# UIControlFactory - Central Control Definition System

**Date**: November 13, 2025
**Status**: ✅ **FULLY IMPLEMENTED**

---

## 🎯 Overview

**UIControlFactory** is the **SINGLE LOCATION** where **ALL UI control definitions** can be retrieved based on a given theme. Every control property (font, color, size, spacing) comes from this centralized factory.

---

## 📍 Location

- **Factory**: `WebTemplate.Setup/UI/UIControlFactory.cs`
- **Examples**: `WebTemplate.Setup/UI/UIControlFactory.Examples.cs`
- **Theme**: `WebTemplate.Setup/UI/UITheme.cs` (provides the theme values)

---

## 🏭 Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    UIControlFactory                      │
│  - Creates controls with theme applied                   │
│  - ALL definitions in ONE place                          │
│  - Type-based control creation                           │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ├──► UITheme (gets colors, fonts, sizes)
                   │
                   └──► Creates fully-configured controls
                           ↓
                    ┌──────────────────┐
                    │  Ready-to-use    │
                    │  Control with    │
                    │  all properties  │
                    │  from theme      │
                    └──────────────────┘
```

---

## 🎨 Available Control Types

### Text Input Controls

| Type | Purpose | Font | Features |
|------|---------|------|----------|
| `StandardTextBox` | General text input | Segoe UI 9pt | Standard sizing |
| `MonospaceTextBox` | Technical data (secrets, connections) | Consolas 9pt | Wider, monospace |
| `MultilineTextBox` | Long text | Segoe UI 9pt | Scrollable, multiline |
| `PasswordTextBox` | Password input | Segoe UI 9pt | Masked characters |

### Button Controls

| Type | Purpose | Appearance | Use Case |
|------|---------|------------|----------|
| `PrimaryButton` | Main actions | Standard button face | Save, Generate, Create |
| `SecondaryButton` | Secondary actions | Light background | Cancel, Back, Browse |
| `DangerButton` | Destructive actions | Red background, white text | Delete, Remove, Clear |
| `IconButton` | Icon-only buttons | Small, 30x23px | Tool buttons |

### Label Controls

| Type | Purpose | Font | Color |
|------|---------|------|-------|
| `StandardLabel` | General labels | Segoe UI 9pt | Primary text |
| `FieldLabel` | Field labels (e.g., "Name:") | Segoe UI 9pt | Primary text, left-aligned |
| `HeaderLabel` | Section headers | Segoe UI 10pt Bold | Primary text |
| `SubHeaderLabel` | Sub-sections | Segoe UI 9.75pt Bold | Primary text |
| `StatusLabel` | Status messages | Segoe UI 8.25pt | Primary text |
| `ErrorLabel` | Error messages | Segoe UI 9pt | Red text |
| `SuccessLabel` | Success messages | Segoe UI 9pt | Green text |

### Other Controls

| Type | Purpose |
|------|---------|
| `CheckBox` | Boolean options |
| `RadioButton` | Single choice from group |
| `NumericUpDown` | Numeric input |
| `ComboBox` | Dropdown selection |
| `GroupBox` | Group related controls |
| `Panel` | Flexible container |

---

## 🛠️ How to Use

### Basic Usage - Create Individual Controls

```csharp
using WebTemplate.Setup.UI;

// Create a standard TextBox
var txtName = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox,
    "txtName"
);

// Create a monospace TextBox for technical data
var txtConnectionString = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.MonospaceTextBox,
    "txtConnectionString"
);

// Create a password TextBox
var txtPassword = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.PasswordTextBox,
    "txtPassword"
);

// Create buttons with different styles
var btnSave = UIControlFactory.CreateButton(
    UIControlFactory.ControlType.PrimaryButton,
    "btnSave",
    "Save"
);

var btnDelete = UIControlFactory.CreateButton(
    UIControlFactory.ControlType.DangerButton,
    "btnDelete",
    "Delete"
);

// Create labels
var lblHeader = UIControlFactory.CreateLabel(
    UIControlFactory.ControlType.HeaderLabel,
    "lblHeader",
    "Database Configuration"
);

var lblError = UIControlFactory.CreateLabel(
    UIControlFactory.ControlType.ErrorLabel,
    "lblError",
    "This field is required"
);
```

**Result**: Every control is created with ALL properties from the centralized theme!

---

### Advanced Usage - Create Field Rows

The factory provides convenience methods for common patterns:

```csharp
// Create a label + textbox pair
var (lblProjectName, txtProjectName) = UIControlFactory.CreateFieldRow(
    "Project Name",     // Label text
    "ProjectName"       // Control name
);

// Create with monospace textbox for technical data
var (lblApiKey, txtApiKey) = UIControlFactory.CreateFieldRow(
    "API Key",
    "ApiKey",
    UIControlFactory.ControlType.MonospaceTextBox
);

// Create with password textbox
var (lblPassword, txtPassword) = UIControlFactory.CreateFieldRow(
    "Password",
    "Password",
    UIControlFactory.ControlType.PasswordTextBox
);
```

**Result**: Label is 120px wide, properly aligned, textbox has correct type and sizing!

---

### Complete Example - Build a Form Section

```csharp
private GroupBox BuildDatabaseSection()
{
    // Create container (gets padding, font from theme)
    var grpDatabase = UIControlFactory.CreateGroupBox(
        "grpDatabase",
        "Database Configuration"
    );
    grpDatabase.Size = new Size(500, 300);

    // Create fields using factory
    var (lblServer, txtServer) = UIControlFactory.CreateFieldRow(
        "Server",
        "Server"
    );

    var (lblDatabase, txtDatabase) = UIControlFactory.CreateFieldRow(
        "Database",
        "Database"
    );

    var (lblConnectionString, txtConnectionString) = UIControlFactory.CreateFieldRow(
        "Connection String",
        "ConnectionString",
        UIControlFactory.ControlType.MonospaceTextBox  // Monospace for connection string
    );

    // Create checkbox
    var chkIntegratedSecurity = UIControlFactory.Create(
        UIControlFactory.ControlType.CheckBox,
        "chkIntegratedSecurity",
        "Use Integrated Security"
    ) as CheckBox;

    // Create buttons
    var btnTest = UIControlFactory.CreateButton(
        UIControlFactory.ControlType.PrimaryButton,
        "btnTest",
        "Test Connection"
    );

    var btnBuild = UIControlFactory.CreateButton(
        UIControlFactory.ControlType.SecondaryButton,
        "btnBuild",
        "Build Connection String"
    );

    // Create validation label
    var lblValidation = UIControlFactory.CreateValidationLabel("lblValidation");

    // Position controls
    int y = 20;
    lblServer.Location = new Point(10, y);
    txtServer.Location = new Point(140, y);
    y += 30;

    lblDatabase.Location = new Point(10, y);
    txtDatabase.Location = new Point(140, y);
    y += 30;

    // ... position other controls

    // Add to container
    grpDatabase.Controls.AddRange(new Control[]
    {
        lblServer, txtServer,
        lblDatabase, txtDatabase,
        lblConnectionString, txtConnectionString,
        chkIntegratedSecurity!,
        btnTest, btnBuild,
        lblValidation
    });

    return grpDatabase;
}
```

**Result**: Entire section built from factory - consistent fonts, colors, sizing, spacing!

---

## 🎯 Key Benefits

### 1. **Single Definition Location**

```csharp
// ALL TextBox properties defined in ONE place
private static void ApplyStandardTextBoxTheme(TextBox textBox)
{
    textBox.Font = UITheme.Fonts.Default;              // From theme
    textBox.BackColor = UITheme.Colors.TextBoxBackground;  // From theme
    textBox.ForeColor = UITheme.Colors.PrimaryText;    // From theme
    textBox.Size = new Size(
        UITheme.Sizes.TextBoxWidth,                    // From theme
        UITheme.Sizes.TextBoxHeight                    // From theme
    );
    textBox.Margin = UITheme.Spacing.ControlMargin;    // From theme
}
```

### 2. **Consistent Creation**

```csharp
// Old way (inconsistent, scattered)
var txt1 = new TextBox { Font = new Font("Segoe UI", 9F), Size = new Size(250, 23) };
var txt2 = new TextBox { Font = new Font("Segoe UI", 9F), Size = new Size(300, 23) }; // Oops, different size!

// New way (consistent, from factory)
var txt1 = UIControlFactory.CreateTextBox(UIControlFactory.ControlType.StandardTextBox, "txt1");
var txt2 = UIControlFactory.CreateTextBox(UIControlFactory.ControlType.StandardTextBox, "txt2");
// Both identical - all properties from centralized definition!
```

### 3. **Theme-Based Changes**

Want to make all buttons larger? Change ONE line in the theme:

```csharp
// In UITheme.cs
public static readonly Size ButtonMedium = new(120, 35);  // Was (100, 30)
```

**Every button created by the factory automatically uses the new size!**

### 4. **Type-Specific Styling**

```csharp
// Automatically gets correct font for the type
var txtNormal = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox, "txt1"
);  // → Segoe UI

var txtSecret = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.MonospaceTextBox, "txt2"
);  // → Consolas (monospace)
```

### 5. **Easy to Extend**

Want a new control type? Add it in ONE place:

```csharp
// 1. Add to enum
public enum ControlType
{
    // ... existing
    LargeTextBox,  // NEW!
}

// 2. Add factory case
case ControlType.LargeTextBox:
    return new TextBox { Multiline = true, Height = 150 };

// 3. Add theme application
private static void ApplyLargeTextBoxTheme(TextBox textBox)
{
    textBox.Font = UITheme.Fonts.Default;
    textBox.Size = new Size(UITheme.Sizes.TextBoxWidthLarge, 150);
    // ... other properties
}
```

**Now everyone can create LargeTextBox with the same definition!**

---

## 📊 Comparison: Before vs After

### Before (Scattered Definitions)

```csharp
// In Form1.Designer.cs
txtName.Font = new Font("Segoe UI", 9F);
txtName.Size = new Size(250, 23);
txtName.BackColor = SystemColors.Window;

// In Form2.Designer.cs
txtEmail.Font = new Font("Segoe UI", 9F);
txtEmail.Size = new Size(300, 23);  // Different size!
txtEmail.BackColor = Color.White;   // Different color!

// In UserControl1.Designer.cs
txtAddress.Font = new Font("Arial", 9F);  // Different font!
txtAddress.Size = new Size(250, 25);      // Different height!
```

❌ **Problems:**
- Inconsistent properties across forms
- Hard to change globally
- No single source of truth
- Copy-paste errors

### After (Centralized Factory)

```csharp
// Everywhere in the application
var txtName = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox, "txtName"
);

var txtEmail = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox, "txtEmail"
);

var txtAddress = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox, "txtAddress"
);
```

✅ **Benefits:**
- ALL TextBoxes identical
- Change once → affects all
- Single source of truth
- No errors possible

---

## 🔄 How It Works Internally

### Step 1: Request a Control

```csharp
var txtSecret = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.MonospaceTextBox,
    "txtSecret"
);
```

### Step 2: Factory Creates Instance

```csharp
private static Control CreateControlInstance(ControlType type)
{
    return type switch
    {
        ControlType.MonospaceTextBox => new TextBox(),  // Creates TextBox
        // ...
    };
}
```

### Step 3: Apply Theme Definition

```csharp
private static void ApplyMonospaceTextBoxTheme(TextBox textBox)
{
    textBox.Font = UITheme.Fonts.Monospace;  // ← From UITheme.cs
    textBox.BackColor = UITheme.Colors.TextBoxBackground;  // ← From UITheme.cs
    textBox.ForeColor = UITheme.Colors.PrimaryText;  // ← From UITheme.cs
    textBox.Size = new Size(
        UITheme.Sizes.TextBoxWidthLarge,  // ← From UITheme.cs
        UITheme.Sizes.TextBoxHeight       // ← From UITheme.cs
    );
    textBox.Margin = UITheme.Spacing.ControlMargin;  // ← From UITheme.cs
}
```

### Step 4: Return Configured Control

```csharp
return textBox;  // Fully configured with ALL properties from theme!
```

---

## 📚 Convenience Methods

The factory provides helper methods for common patterns:

### CreateFieldRow - Label + Input Pair

```csharp
var (label, textBox) = UIControlFactory.CreateFieldRow(
    "Server Name",  // Label text
    "ServerName"    // Control name
);
```

**Returns**: Label (120px wide, left-aligned) + TextBox (250px wide, themed)

### CreateBrowseButton - File/Folder Selection

```csharp
var btnBrowse = UIControlFactory.CreateBrowseButton("btnBrowse");
```

**Returns**: Button sized 80x23 with "Browse..." text

### CreateValidationLabel - Error Display

```csharp
var lblError = UIControlFactory.CreateValidationLabel("lblError");
```

**Returns**: Red error label, hidden by default

---

## 🎨 Control Type Reference

### TextBox Types

```csharp
// Standard input
UIControlFactory.ControlType.StandardTextBox
→ Font: Segoe UI 9pt
→ Size: 250x23
→ Color: Window background

// Technical data (secrets, connections, code)
UIControlFactory.ControlType.MonospaceTextBox
→ Font: Consolas 9pt (monospace)
→ Size: 400x23 (wider)
→ Color: Window background

// Long text
UIControlFactory.ControlType.MultilineTextBox
→ Font: Segoe UI 9pt
→ Size: 400x100
→ Features: Multiline, scrollbars

// Passwords
UIControlFactory.ControlType.PasswordTextBox
→ Font: Segoe UI 9pt
→ Size: 250x23
→ Features: Masked characters
```

### Button Types

```csharp
// Primary actions (Save, Create, Generate)
UIControlFactory.ControlType.PrimaryButton
→ Size: 100x30
→ Style: Standard button face

// Secondary actions (Cancel, Back, Browse)
UIControlFactory.ControlType.SecondaryButton
→ Size: 100x30
→ Style: Light background

// Destructive actions (Delete, Remove)
UIControlFactory.ControlType.DangerButton
→ Size: 100x30
→ Style: Red background, white text, bold font

// Icon buttons
UIControlFactory.ControlType.IconButton
→ Size: 30x23
→ Style: Compact, for toolbar icons
```

### Label Types

```csharp
// Standard label
UIControlFactory.ControlType.StandardLabel
→ Font: Segoe UI 9pt
→ Color: Primary text

// Field label (next to inputs)
UIControlFactory.ControlType.FieldLabel
→ Font: Segoe UI 9pt
→ Alignment: Middle left
→ Width: 120px

// Section header
UIControlFactory.ControlType.HeaderLabel
→ Font: Segoe UI 10pt Bold
→ Margin: Extra bottom spacing

// Error message
UIControlFactory.ControlType.ErrorLabel
→ Font: Segoe UI 9pt
→ Color: Red

// Success message
UIControlFactory.ControlType.SuccessLabel
→ Font: Segoe UI 9pt
→ Color: Green
```

---

## ✅ Best Practices

### DO ✅

```csharp
// Use factory for all control creation
var txt = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.MonospaceTextBox,
    "txtConnectionString"
);

// Use correct type for purpose
var txtSecret = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.PasswordTextBox,  // ✅ Correct for passwords
    "txtPassword"
);

var txtCode = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.MonospaceTextBox,  // ✅ Correct for technical data
    "txtApiKey"
);

// Use convenience methods
var (lbl, txt) = UIControlFactory.CreateFieldRow("Name", "Name");
```

### DON'T ❌

```csharp
// Don't create controls manually
var txt = new TextBox  // ❌ No theme applied
{
    Font = new Font("Segoe UI", 9F),
    Size = new Size(250, 23)
};

// Don't use wrong type
var txtSecret = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox,  // ❌ Should be PasswordTextBox
    "txtPassword"
);

// Don't modify factory-created controls (defeats the purpose)
var txt = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.StandardTextBox,
    "txt"
);
txt.Font = new Font("Arial", 10F);  // ❌ Don't override theme properties
```

---

## 🚀 Real-World Usage

See complete examples in: `WebTemplate.Setup/UI/UIControlFactory.Examples.cs`

---

## 🎓 Summary

**UIControlFactory provides:**

1. **Central location** for ALL control definitions
2. **Theme-based** control creation
3. **Type-specific** styling (Standard, Monospace, Password, etc.)
4. **Consistent** properties across entire application
5. **Easy to change** - modify theme, all controls update

**Key Principle:**

> **"Every UI control gets its complete definition from a single centralized location based on the requested type"**

**Result:**

```csharp
// Request this:
var txt = UIControlFactory.CreateTextBox(
    UIControlFactory.ControlType.MonospaceTextBox,
    "txtConnectionString"
);

// Get this:
// ✅ Font: Consolas 9pt (from UITheme)
// ✅ Size: 400x23 (from UITheme)
// ✅ Colors: Themed (from UITheme)
// ✅ Margin: 3,3,3,3 (from UITheme)
// ✅ ALL properties from ONE definition!
```

---

**Perfect consistency, zero duplication, single source of truth!** 🎨✨
