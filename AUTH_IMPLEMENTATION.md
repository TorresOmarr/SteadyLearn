# 🔐 AUTH_IMPLEMENTATION.md - Authentication System Details

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Overview

Authentication is the first feature implemented (Sprint 1). It provides:
- User registration with email verification
- Secure login with JWT tokens
- Token refresh flow (Access + Refresh tokens)
- Password reset capability
- Admin seeding for development

---

## 🔑 JWT Strategy

### Access Token
```
Type: Bearer token (JWT)
Lifetime: 15 minutes
Storage: Authorization header
Contains: userId, email, role
Purpose: Stateless authentication for API requests
Invalidation: Cannot be revoked (short lived)
```

### Refresh Token
```
Type: Opaque token (should be stored as hash)
Lifetime: 7 days (configurable)
Storage: HttpOnly cookie (immune to XSS)
Contains: userId, token version (rotation tracking)
Purpose: Obtain new access token without re-login
Invalidation: Requires logout or token expiry
Rotation: Issued fresh on each use (old token invalidated)
```

### Token Rotation Strategy
```
User logs in
  ↓
Server issues accessToken (15 min) + refreshToken (7 day)
  ↓
refreshToken stored in HttpOnly cookie
  ↓
accessToken expires after 15 min
  ↓
Frontend uses refreshToken to get new accessToken
  ↓
Server rotates refreshToken (invalidates old, issues new)
  ↓
New refreshToken stored in HttpOnly cookie
```

---

## 🛡️ Password Security

### Password Requirements
```
Minimum length: 8 characters
Must contain:
  ✓ 1 uppercase letter (A-Z)
  ✓ 1 lowercase letter (a-z)
  ✓ 1 digit (0-9)
  ✓ 1 special character (!@#$%^&*)

Examples:
  ✓ MyPassword123!
  ✓ SecureP@ss2025
  ❌ password123 (no uppercase, no special char)
  ❌ SHORT1! (too short)
```

### Password Hashing
```
Algorithm: bcrypt (via BCrypt.Net-Next NuGet)
Cost factor: 12 (default, adjustable)
Storage: Salted hash, never plaintext
Comparison: Always use bcrypt.Verify()

Never:
  - Store plaintext passwords
  - Use MD5 or SHA1
  - Implement custom hashing
```

---

## 📧 Email Verification

### Flow
```
1. User registers with email + password
2. Server generates 24-hour verification token
3. Development: Token logged to console (no email sent)
4. Production: Token sent via email
5. User clicks link with token
6. Server validates token and marks email as verified
7. Token destroyed after use
```

### Token Generation
```
Format: Base64URL(GUID + timestamp)
Expiry: 24 hours from creation
Storage: EmailVerificationToken field on User
Validation: Check token exists + hasn't expired + matches user
Cleanup: Delete after verification or expiry (optional)
```

### Development Email Service (Fake)
```csharp
// Development uses IEmailService implementation that:
// - Logs verification link to console
// - Returns success immediately
// - Never sends actual emails

Interface IEmailService
  ├── SendVerificationEmailAsync(email, token)
  ├── SendPasswordResetEmailAsync(email, token)
  └── [future] SendNotificationEmailAsync(...)

Development: FakeEmailService (console logging)
Production: SmtpEmailService (real email)
```

---

## 🔄 Authentication Flow Diagrams

### Registration
```
POST /api/auth/register
{
  email: "user@example.com",
  password: "SecureP@ss123!",
  firstName: "John",
  lastName: "Doe"
}
  ↓
Validate input (FluentValidation)
  ↓
Check email not taken
  ↓
Hash password with bcrypt
  ↓
Generate verification token (24h expiry)
  ↓
Create User record (IsEmailVerified = false)
  ↓
Send verification email (fake in dev)
  ↓
Return success + "Check email for verification link"
```

### Login
```
POST /api/auth/login
{
  email: "user@example.com",
  password: "SecureP@ss123!"
}
  ↓
Find user by email (case-insensitive)
  ↓
Verify password hash
  ↓
If password wrong: return 401 Unauthorized
  ↓
If email not verified: return 403 Forbidden
  ↓
Generate JWT access token (15 min)
  ↓
Generate refresh token (7 days)
  ↓
Hash and store refresh token in User.RefreshTokenHash
  ↓
Return access token in body
  ↓
Set refresh token in HttpOnly cookie
  ↓
Return 200 OK + user details
```

### Refresh Token
```
POST /api/auth/refresh
Cookie: refreshToken=<token>
  ↓
Extract refreshToken from cookie
  ↓
Find user by token hash
  ↓
Verify token hasn't expired
  ↓
Invalidate old token (set RefreshTokenExpiresAt to past)
  ↓
Generate new access token (15 min)
  ↓
Generate new refresh token (7 days)
  ↓
Hash and store new refresh token
  ↓
Clear old refresh cookie
  ↓
Set new refresh cookie
  ↓
Return new access token
```

### Email Verification
```
GET /api/auth/verify-email?token=<token>
  ↓
Find user with matching verification token
  ↓
Check token hasn't expired (24h rule)
  ↓
If expired: return error + option to resend
  ↓
Set User.IsEmailVerified = true
  ↓
Clear User.EmailVerificationToken
  ↓
Return success + redirect to login
```

### Password Reset
```
POST /api/auth/forgot-password
{
  email: "user@example.com"
}
  ↓
Find user by email
  ↓
If not found: return success anyway (security)
  ↓
If found: Generate reset token (24h expiry)
  ↓
Send reset email (dev: log to console)
  ↓
Return success message

---

POST /api/auth/reset-password
{
  token: "reset-token",
  newPassword: "NewP@ss123!"
}
  ↓
Find user with matching reset token
  ↓
Validate new password
  ↓
Verify token hasn't expired
  ↓
Hash new password
  ↓
Update User.PasswordHash
  ↓
Clear User.PasswordResetToken
  ↓
Clear any active refresh tokens (force re-login)
  ↓
Return success
```

### Logout
```
POST /api/auth/logout
Authorization: Bearer <accessToken>
  ↓
Verify access token is valid (not expired)
  ↓
Find user from token claim
  ↓
Clear User.RefreshTokenHash
  ↓
Clear User.RefreshTokenExpiresAt
  ↓
Blacklist access token (optional, for revocation)
  ↓
Clear refreshToken cookie
  ↓
Return success
```

---

## 👤 Admin User Setup

### Seeding in Development
```
When docker-compose starts:
  1. PostgreSQL starts
  2. dotnet ef database update runs (migrations)
  3. Seeding script creates initial admin user
  4. Admin credentials from appsettings.Development.json
```

### Configuration
```json
// appsettings.Development.json
{
  "Admin": {
    "Email": "admin@example.com",
    "Password": "AdminInitialPassword123!",
    "FirstName": "Admin",
    "LastName": "User"
  },
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Seeding Code Pattern
```csharp
// Data/Seeding/AdminSeeder.cs
public static async Task SeedAdminAsync(this ApplicationDbContext context, IConfiguration config)
{
    var adminEmail = config["Admin:Email"];
    
    // Check if admin already exists
    if (await context.Users.AnyAsync(x => x.Email == adminEmail))
        return;
    
    // Hash password
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(config["Admin:Password"]);
    
    // Create admin user
    var admin = new User
    {
        Id = Guid.NewGuid(),
        Email = adminEmail,
        FirstName = config["Admin:FirstName"],
        LastName = config["Admin:LastName"],
        PasswordHash = passwordHash,
        Role = UserRole.Admin,
        IsEmailVerified = true,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = "SYSTEM"
    };
    
    context.Users.Add(admin);
    await context.SaveChangesAsync();
}
```

### Calling from Startup
```csharp
// Program.cs
var app = builder.Build();

// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await context.SeedAdminAsync(builder.Configuration);
}

app.Run();
```

---

## 🔌 Authorization Policies

### Role-Based Authorization
```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(UserRole.Admin.ToString()));
    
    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole(UserRole.Student.ToString()));
    
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});
```

### Using in Endpoints
```csharp
// Require Admin
app.MapPost("/api/courses", CreateCourse)
    .RequireAuthorization("AdminOnly");

// Require Authentication
app.MapGet("/api/courses/{id}", GetCourse)
    .RequireAuthorization("AuthenticatedUser");

// Public (no auth)
app.MapPost("/api/auth/register", Register)
    .AllowAnonymous();
```

---

## 🛡️ Security Best Practices

### ✅ IMPLEMENTED
```
1. Password hashing with bcrypt
2. Email verification before access
3. Refresh token rotation
4. HttpOnly cookies for sensitive tokens
5. Short-lived access tokens
6. Always validate tokens on backend
7. Clear error messages (don't leak user existence)
8. Rate limiting (TODO: future enhancement)
```

### ❌ NOT YET
```
1. CSRF protection (add CSRF tokens)
2. Account lockout (after N failed attempts)
3. IP whitelist (for admin accounts)
4. 2FA/MFA (two-factor authentication)
5. OAuth/OpenID (IDP integration - Phase 2)
6. API key authentication (for services)
```

---

## 🧪 Testing Auth

### Unit Tests
```
RegisterCommandValidator
  ✓ Valid email + password passes
  ✓ Weak password fails
  ✓ Duplicate email fails
  ✓ Invalid email format fails

LoginCommandValidator
  ✓ Valid credentials pass
  ✓ Missing email fails
  ✓ Missing password fails

RegisterCommandHandler
  ✓ Creates user successfully
  ✓ Hashes password correctly
  ✓ Generates verification token
  ✓ User starts unverified

LoginCommandHandler
  ✓ Logs in verified user
  ✓ Rejects unverified user
  ✓ Rejects wrong password
  ✓ Issues correct tokens

RefreshTokenHandler
  ✓ Issues new access token
  ✓ Rotates refresh token
  ✓ Rejects expired token
  ✓ Invalidates old token
```

---

## 📝 Error Codes (Auth)

| Code | HTTP | Meaning |
|------|------|---------|
| INVALID_EMAIL_FORMAT | 400 | Email format is invalid |
| EMAIL_ALREADY_EXISTS | 400 | Email already registered |
| PASSWORD_TOO_WEAK | 400 | Password doesn't meet requirements |
| PASSWORDS_NOT_MATCHING | 400 | Password confirmation doesn't match |
| INVALID_CREDENTIALS | 401 | Email or password incorrect |
| EMAIL_NOT_VERIFIED | 403 | Email verification pending |
| VERIFICATION_TOKEN_EXPIRED | 400 | Verification token expired |
| VERIFICATION_TOKEN_INVALID | 400 | Verification token invalid |
| REFRESH_TOKEN_EXPIRED | 401 | Refresh token expired |
| REFRESH_TOKEN_INVALID | 401 | Refresh token invalid |
| ACCESS_TOKEN_INVALID | 401 | Access token invalid/expired |
| UNAUTHORIZED | 401 | No token provided |
| FORBIDDEN | 403 | Token valid but insufficient permissions |

---

## 🔗 Related Documents

- **AGENTS.md** - Overall vision (Sprint 1)
- **API_DESIGN.md** - Auth endpoints
- **DOMAIN_MODEL.md** - User entity
- **TESTING.md** - Auth testing patterns
- **DATABASE.md** - User schema + migrations

---

*Authentication is the foundation that all other features depend on.*
