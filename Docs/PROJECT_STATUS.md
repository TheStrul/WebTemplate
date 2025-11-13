# Core WebTemplate - Starter Template

A complete starter template for future web applications with **zero hard-coding policy** - all UI elements are configurable through CSS variables and resource files.

## Project Overview

This is a comprehensive web application starter that includes:
- **Frontend**: React TypeScript with complete CSS variable system
- **Backend**: .NET 9.0 with WebTemplate.API and WebTemplate.Core projects  
- **Authentication**: JWT-based with refresh tokens and role-based access control
- **Database**: Entity Framework Core with Identity integration
- **CSS System**: Complete variable-based theming with light/dark mode support

## Core Principles

### Zero Hard-Coding Policy
- **NO** hard-coded text, sizes, colors, or UI elements in code
- All UI definitions must be configurable via CSS variables or resource files
- Complete modularity and reusability across different applications

### CSS Variable System
All styling is managed through a comprehensive CSS variable system:
- `variables.css` - Core design system definitions
- `themes/light.css` - Light theme variables
- `themes/dark.css` - Dark theme variables  
- `components/base.css` - Base component styles

## Current Implementation Status

### ✅ Completed Features

#### CSS Architecture
- Complete variable system with 100+ design tokens
- Typography scale, color palette, spacing system
- Component-level styling with zero hard-coded values
- Light/dark theme support with smooth transitions
- Responsive design patterns

#### Backend Authentication
- JWT authentication service with refresh tokens
- Role-based access control system
- User management with ApplicationUser extending IdentityUser
- AuthController with login, register, refresh, logout endpoints
- Comprehensive error handling and validation

#### Frontend Services  
- ApiClient with automatic token management
- AuthService for authentication operations
- AuthContext for React state management
- ThemeContext for theme switching

#### UI Components
- **LoginForm**: Complete form with validation, error handling, CSS styling
- **RegisterForm**: Full registration form with terms/privacy acceptance
- **InputGroup**: Reusable input component with validation and help text
- **Button**: Complete button component with variants and loading states
- **AuthPage**: Container component that switches between login/register

### 🔄 In Progress
- Component integration and testing
- Protected route implementation
- Error boundary components

### ⏳ Planned Features
- Localization system
- Additional UI components (modals, tables, etc.)
- Dashboard layout components
- Admin panel components

## Project Structure

```
Backend/
├── WebTemplate.sln
├── WebTemplate.API/          # API layer with controllers
│   ├── Controllers/
│   │   └── AuthController.cs
│   ├── appsettings.json
│   └── Program.cs
└── WebTemplate.Core/         # Core business logic
    ├── Entities/
    ├── DTOs/
    ├── Interfaces/
    └── Services/

Frontend/
└── webtemplate-frontend/
    ├── public/
    └── src/
        ├── components/
        │   ├── Auth/     # Authentication components
        │   └── Base/     # Reusable base components  
        ├── contexts/     # React contexts
        ├── services/     # API services
        ├── styles/       # CSS system
        │   ├── variables.css
        │   ├── themes/
        │   └── components/
        └── types/        # TypeScript definitions
```

## Getting Started

### Prerequisites
- Node.js 18+
- .NET 9.0 SDK
- SQL Server or SQL Server Express LocalDB

### Backend Setup
1. Navigate to `Backend/`
2. Restore packages: `dotnet restore`
3. Update database: `dotnet ef database update -p WebTemplate.API`
4. Run API: `dotnet run --project WebTemplate.API`

### Frontend Setup
1. Navigate to `Frontend/webtemplate-frontend/`
2. Install dependencies: `npm install`
3. Start development server: `npm start`

### Configuration

#### CSS Themes
Themes are automatically applied based on system preference or manual selection.
All theme variables are defined in `src/styles/themes/`.

#### Authentication Settings
Update `appsettings.json` in WebTemplate.API for JWT configuration:
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  }
}
```

## Development Guidelines

### CSS Development
- **NEVER** use hard-coded values in CSS
- Always reference CSS variables from the design system
- Add new variables to `variables.css` following naming conventions
- Use semantic color names (e.g., `--text-primary`, not `--gray-900`)

### Component Development  
- Follow the established patterns in existing components
- Ensure full accessibility with proper ARIA attributes
- Include comprehensive TypeScript interfaces
- Implement proper error handling and loading states

### API Development
- Follow RESTful conventions
- Include comprehensive validation
- Use DTOs for all data transfer
- Implement proper error responses

## Next Steps

1. **Complete RegisterForm Integration**: Finish RegisterForm component integration with AuthPage
2. **Protected Routes**: Implement route protection wrapper component
3. **Dashboard Layout**: Create main application layout components
4. **Localization**: Implement i18n system for multi-language support
5. **Component Library**: Expand base component library (modals, tables, forms)

## Contributing

When contributing to this project:
1. Maintain the zero hard-coding policy
2. Use CSS variables for all styling
3. Follow existing naming conventions
4. Include proper TypeScript types
5. Add comprehensive error handling
6. Ensure responsive design
7. Test with both light and dark themes

## Architecture Notes

This starter template is designed to be the foundation for any type of web application. The modular architecture allows for:
- Easy customization without code changes
- Theme switching and branding updates
- Scalable component reuse
- Microservice-ready backend architecture
- Complete separation of concerns

The CSS variable system ensures that any UI element can be reconfigured without touching the codebase, making this truly reusable across different projects and brands.