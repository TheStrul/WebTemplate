# Core WebTemplate - Project Instructions

## Project Vision
This is a starter point WebTemplate designed for any kind of future web application development. It provides a comprehensive foundation including Frontend + Backend + Database with all common components typically needed when developing web applications.

## Core Principles

### 1. Modular Design
- Components designed in standard and flexible patterns
- Ability to be added as standalone microservices to future webApps
- Maximum independence with minimal configuration required
- Each module should be self-contained and loosely coupled

### 2. Complete UI Configurability
- **ZERO hard-coded UI elements in code**
- All text must be externalized to resource files
- All sizes, colors, fonts, spacing defined in CSS variables
- All UI definitions must be configurable through CSS/resource files
- Theming support through CSS custom properties
- Responsive design through CSS media queries

### 3. Technology Stack
- **Frontend**: React TypeScript
- **Backend**: .NET 9.0 (WebTemplate.API + WebTemplate.Core)
- **Database**: To be integrated (likely Entity Framework Core)
- **Authentication**: JWT-based with refresh tokens
- **Authorization**: Role-based access control (RBAC)

## First Component: User Handling Module

### User Management Features
1. **Authentication**
   - Login/Logout functionality
   - JWT token management
   - Refresh token implementation
   - Password reset functionality
   - Email verification

2. **Authorization**
   - Role-based access control
   - Permission-based authorization
   - Admin role management
   - User role assignment

3. **User CRUD Operations**
   - User registration
   - User profile management
   - Admin user management interface
   - User settings management
   - Password management (change/reset)

4. **Admin Functionality**
   - User administration dashboard
   - Role management
   - Permission management
   - User activity monitoring

### Technical Implementation Requirements

#### Backend Structure
```
WebTemplate.Core/
├── Entities/
│   ├── ApplicationUser.cs
│   ├── UserRole.cs
│   ├── Permission.cs
│   └── UserSession.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   ├── RegisterDto.cs
│   │   ├── TokenDto.cs
│   │   └── PasswordResetDto.cs
│   └── User/
│       ├── UserProfileDto.cs
│       ├── UserManagementDto.cs
│       └── UserSettingsDto.cs
├── Interfaces/
│   ├── IAuthService.cs
│   ├── IUserService.cs
│   ├── ITokenService.cs
│   └── IEmailService.cs
├── Services/
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── TokenService.cs
│   └── EmailService.cs
└── Configuration/
    ├── JwtSettings.cs
    ├── EmailSettings.cs
    └── UserSettings.cs
```

#### Frontend Structure
```
src/
├── components/
│   ├── Auth/
│   │   ├── LoginForm/
│   │   ├── RegisterForm/
│   │   ├── PasswordReset/
│   │   └── ProtectedRoute/
│   ├── User/
│   │   ├── UserProfile/
│   │   ├── UserSettings/
│   │   └── UserDashboard/
│   └── Admin/
│       ├── UserManagement/
│       ├── RoleManagement/
│       └── AdminDashboard/
├── hooks/
│   ├── useAuth.ts
│   ├── useUser.ts
│   └── usePermissions.ts
├── services/
│   ├── authService.ts
│   ├── userService.ts
│   └── apiClient.ts
├── contexts/
│   ├── AuthContext.tsx
│   └── ThemeContext.tsx
├── styles/
│   ├── variables.css
│   ├── themes/
│   │   ├── light.css
│   │   └── dark.css
│   └── components/
└── localization/
    ├── en.json
    ├── es.json
    └── fr.json
```

### UI Configuration Standards

#### CSS Variables Structure
```css
:root {
  /* Colors */
  --primary-color: #007bff;
  --secondary-color: #6c757d;
  --success-color: #28a745;
  --danger-color: #dc3545;
  --warning-color: #ffc107;
  --info-color: #17a2b8;
  --light-color: #f8f9fa;
  --dark-color: #343a40;
  
  /* Typography */
  --font-family-primary: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.25rem;
  --font-size-xl: 1.5rem;
  
  /* Spacing */
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 3rem;
  
  /* Borders */
  --border-radius: 0.375rem;
  --border-width: 1px;
  --border-color: #dee2e6;
  
  /* Shadows */
  --shadow-sm: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
  --shadow-md: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
  --shadow-lg: 0 1rem 3rem rgba(0, 0, 0, 0.175);
}
```

#### Localization Requirements
- All text must be externalized to JSON resource files
- Support for multiple languages (en, es, fr as starting point)
- Dynamic language switching
- Date/time localization
- Number formatting localization

#### Component Configuration Example
```typescript
// ❌ WRONG - Hard-coded text and styles
const LoginButton = () => (
  <button style={{backgroundColor: '#007bff', color: 'white', padding: '10px'}}>
    Login
  </button>
);

// ✅ CORRECT - Configurable text and styles
const LoginButton = () => {
  const { t } = useTranslation();
  return (
    <button className="btn btn-primary">
      {t('auth.login')}
    </button>
  );
};
```

### Development Guidelines

1. **No Hard-Coding Policy**
   - No inline styles in components
   - No hard-coded text strings
   - No fixed dimensions or colors in code
   - All configuration through external files

2. **Modular Architecture**
   - Each feature as a separate module
   - Clear interfaces between modules
   - Dependency injection for services
   - Configuration-driven behavior

3. **Security Standards**
   - Input validation on both client and server
   - SQL injection prevention
   - XSS protection
   - CSRF protection
   - Secure password handling
   - JWT token security

4. **Testing Requirements**
   - Unit tests for all services
   - Integration tests for APIs
   - E2E tests for critical user flows
   - Security testing

5. **Documentation Standards**
   - API documentation (Swagger/OpenAPI)
   - Component documentation (Storybook)
   - Setup and deployment guides
   - Configuration reference

### Future Microservice Readiness
- Database per service pattern consideration
- API gateway compatibility
- Service discovery readiness
- Configuration management
- Logging and monitoring hooks
- Health check endpoints

## Implementation Phases

### Phase 1: Foundation
- Basic project structure
- Authentication system
- Basic user management
- CSS variable system
- Localization framework

### Phase 2: User Management
- Complete user CRUD operations
- Admin interface
- Role and permission system
- User settings management

### Phase 3: Advanced Features
- Email notifications
- User activity logging
- Advanced security features
- Performance optimization

### Phase 4: Microservice Preparation
- Service boundaries definition
- API versioning
- Service communication patterns
- Deployment containerization

This instruction file serves as the blueprint for developing a robust, flexible, and maintainable web application foundation that can scale to any future requirements while maintaining consistency and modularity.