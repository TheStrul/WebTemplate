# WebTemplate - Bulletproof Backend Testing Strategy

## Testing Philosophy
**Goal**: Create a bulletproof backend through comprehensive, systematic testing
**Approach**: Multiple layers of tests covering every angle
**Coverage Target**: 99%+ code coverage with meaningful tests

## Test Pyramid Structure

```
       ╱────────╲
      ╱   E2E    ╲      (Skip stress tests for now)
     ╱────────────╲
    ╱ Integration  ╲    30% - Full API flow tests
   ╱────────────────╲
  ╱   Unit Tests     ╲  70% - Isolated component tests
 ╱────────────────────╲
```

## Test Projects

### 1. **WebTemplate.UnitTests**
- **Purpose**: Test individual components in isolation
- **Scope**: Services, Repositories, Validators, Utilities
- **Tools**: xUnit, Moq, FluentAssertions, AutoFixture
- **Patterns**: AAA (Arrange-Act-Assert), Test Data Builders

### 2. **WebTemplate.IntegrationTests**
- **Purpose**: Test API endpoints with real dependencies
- **Scope**: Controllers, full request/response cycle
- **Tools**: WebApplicationFactory, In-Memory DB, TestContainers
- **Patterns**: API Client patterns, Test fixtures

### 3. **WebTemplate.ApiTests** (Current - to be enhanced)
- **Purpose**: Sanity checks and smoke tests
- **Scope**: Happy path scenarios, basic validation
- **Tools**: WebApplicationFactory, xUnit

## Test Types by Functionality

### Authentication Module Tests

#### A. **Unit Tests** (AuthService)
1. **Login Logic**
   - ✓ Valid credentials → success
   - ✓ Invalid credentials → fail
   - ✓ Non-existent user → fail
   - ✓ Inactive user → fail
   - ✓ Unconfirmed email (when required) → fail
   - ✓ Case-insensitive email lookup
   - ✓ Password hash verification
   - ✓ Token generation called correctly

2. **Registration Logic**
   - ✓ Valid data → user created
   - ✓ Duplicate email → fail
   - ✓ Password validation rules
   - ✓ Email normalization
   - ✓ Default values assigned
   - ✓ Role assignment

3. **Token Management**
   - ✓ Token generation produces valid JWT
   - ✓ Token expiration calculation correct
   - ✓ Refresh token rotation
   - ✓ Token cleanup (expired tokens)
   - ✓ Max tokens per user enforcement
   - ✓ Concurrent token operations

#### B. **Integration Tests** (Auth Endpoints)
1. **POST /api/auth/register**
   - ✓ 201 Created with valid data
   - ✓ 400 Bad Request with invalid email
   - ✓ 400 Bad Request with weak password
   - ✓ 409 Conflict with duplicate email
   - ✓ 422 Unprocessable with mismatched passwords
   - ✓ Response includes user profile
   - ✓ Response includes access token
   - ✓ User persisted to database

2. **POST /api/auth/login**
   - ✓ 200 OK with valid credentials
   - ✓ 401 Unauthorized with wrong password
   - ✓ 401 Unauthorized with non-existent user
   - ✓ 403 Forbidden with inactive user
   - ✓ Rate limiting enforced
   - ✓ Device tracking works
   - ✓ Last login timestamp updated

3. **POST /api/auth/refresh-token**
   - ✓ 200 OK with valid refresh token
   - ✓ 401 with expired token
   - ✓ 401 with revoked token
   - ✓ 401 with tampered token
   - ✓ Token rotation occurs
   - ✓ Old token invalidated

4. **POST /api/auth/logout**
   - ✓ 200 OK removes tokens
   - ✓ Works with valid token
   - ✓ Works with expired token (graceful)
   - ✓ Single device logout
   - ✓ All devices logout

#### C. **Edge Case Tests**
1. **Concurrency**
   - ✓ Simultaneous logins from same user
   - ✓ Parallel token refreshes
   - ✓ Race condition in token cleanup
   - ✓ Double logout handling

2. **Boundary Conditions**
   - ✓ Email at max length (320 chars)
   - ✓ Password at min/max length
   - ✓ Special characters in inputs
   - ✓ Unicode/emoji in names
   - ✓ Token at expiration edge (±1 second)

3. **Error Scenarios**
   - ✓ Database connection failure
   - ✓ Database deadlock recovery
   - ✓ Email service failure
   - ✓ Token signing failure
   - ✓ Null/empty input handling

#### D. **Security Tests**
1. **Input Validation**
   - ✓ SQL injection attempts blocked
   - ✓ XSS payloads sanitized
   - ✓ LDAP injection blocked
   - ✓ Command injection blocked
   - ✓ Path traversal blocked

2. **Authentication**
   - ✓ JWT signature verification
   - ✓ JWT expiration enforced
   - ✓ Token replay prevention
   - ✓ Brute force protection
   - ✓ Password complexity enforced

3. **Authorization**
   - ✓ Role-based access control
   - ✓ Resource ownership verification
   - ✓ Privilege escalation prevention
   - ✓ CORS policy enforcement

#### E. **Data Integrity Tests**
1. **Database Constraints**
   - ✓ Unique email enforced
   - ✓ Required fields validated
   - ✓ Foreign key integrity
   - ✓ Cascade delete behavior

2. **Data Consistency**
   - ✓ Transaction rollback on error
   - ✓ Audit trail created
   - ✓ Timestamps accurate
   - ✓ Soft delete vs hard delete

## Test Naming Convention

```
[MethodName]_[Scenario]_[ExpectedResult]

Examples:
- Login_WithValidCredentials_ReturnsAccessToken
- RefreshToken_WithExpiredToken_Returns401Unauthorized
- Register_WithDuplicateEmail_Returns409Conflict
```

## Test Data Management

### Builders Pattern
```csharp
public class UserBuilder
{
    private string _email = "test@example.com";
    private string _password = "Test123!";

    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithPassword(string password) { _password = password; return this; }
    public LoginDto Build() => new() { Email = _email, Password = _password };
}
```

### AutoFixture for Random Data
```csharp
var fixture = new Fixture();
var user = fixture.Build<ApplicationUser>()
    .With(u => u.Email, "test@example.com")
    .Without(u => u.RefreshTokens)
    .Create();
```

## Code Coverage Requirements

| Component | Min Coverage | Target Coverage |
|-----------|--------------|-----------------|
| Services | 99% | 99% |
| Repositories | 99% | 99% |
| Controllers | 99% | 99% |
| Validators | 99% | 99% |
| DTOs/Entities | 0% | - (data classes) |
| Overall | 99% | 99% |

## Tools & Libraries

### Core Testing
- **xUnit** - Test framework
- **FluentAssertions** - Readable assertions
- **Moq** - Mocking framework
- **AutoFixture** - Test data generation

### Integration Testing
- **WebApplicationFactory** - In-memory API testing
- **EF Core InMemory** - Database testing
- **TestContainers** - Real database testing (future)

### Code Coverage
- **Coverlet** - Coverage collection
- **ReportGenerator** - Coverage reports
- **SonarQube** (optional) - Code quality

## CI/CD Integration

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport

# Run specific category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

## Test Execution Order

1. **Unit Tests** (fastest, run always)
2. **Integration Tests** (slower, run on commit)
3. **API Tests** (smoke tests, run on deploy)

## Next Steps

1. ⏳ Create WebTemplate.UnitTests project
2. ⏳ Setup proper test infrastructure
3. ⏳ Implement Auth module comprehensive tests
4. ⏳ Implement User module tests
5. ⏳ Implement Token module tests
6. ⏳ Setup code coverage reporting
7. ⏳ Create test documentation
