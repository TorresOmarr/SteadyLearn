# API Reference

## Overview

The API follows RESTful conventions with:
- Consistent response format
- Standardized error handling
- Multilingual support via `Accept-Language` header

## Response Format

**Success**:
```json
{
  "success": true,
  "data": { /* payload */ }
}
```

**Error**:
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable message"
  }
}
```

**Validation Error**:
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_FAILED",
    "details": [
      { "field": "email", "code": "INVALID_EMAIL_FORMAT" },
      { "field": "password", "code": "PASSWORD_TOO_WEAK" }
    ]
  }
}
```

---

## Authentication Endpoints

### POST /api/auth/register

**Request**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "passwordConfirm": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Success (201)**:
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com"
  }
}
```

### POST /api/auth/login

**Request**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Success (200)**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "role": "Admin"
    }
  }
}
```
*Also sets HttpOnly refresh token cookie*

### POST /api/auth/refresh

Refresh access token using HttpOnly cookie.

**Success (200)**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs..."
  }
}
```
*Rotates refresh token (new cookie)*

### POST /api/auth/verify-email

**Request**:
```json
{
  "token": "verification-token-from-email"
}
```

**Success (200)**:
```json
{
  "success": true,
  "data": {
    "message": "Email verified successfully"
  }
}
```

### POST /api/auth/forgot-password

**Request**:
```json
{
  "email": "user@example.com"
}
```

**Success (200)**: Always returns success (security best practice)

### POST /api/auth/reset-password

**Request**:
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewPassword123!",
  "newPasswordConfirm": "NewPassword123!"
}
```

### POST /api/auth/logout

**Headers**: `Authorization: Bearer <accessToken>`

**Success (200)**: Clears refresh token cookie

---

## Course Endpoints

### POST /api/courses

*Requires: Admin*

**Request**:
```json
{
  "titleEn": "Introduction to Web Development",
  "titleEs": "Introducción al Desarrollo Web",
  "descriptionEn": "Learn web development basics",
  "descriptionEs": "Aprende los fundamentos del desarrollo web"
}
```

**Success (201)**:
```json
{
  "success": true,
  "data": {
    "courseId": "550e8400-e29b-41d4-a716-446655440001",
    "isPublished": false
  }
}
```

### GET /api/courses/{courseId}

**Headers**: `Accept-Language: en` or `es`

**Success (200)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "title": "Introduction to Web Development",
    "description": "Learn web development basics",
    "isPublished": false,
    "totalDurationMinutes": 120,
    "topics": [...]
  }
}
```

### GET /api/courses

**Query Parameters**:
- `page`: int (default 1)
- `pageSize`: int (default 10)
- `published`: boolean (optional)
- `search`: string (optional)

### PUT /api/courses/{courseId}

*Requires: Admin*

### PATCH /api/courses/{courseId}/publish

*Requires: Admin*

### PATCH /api/courses/{courseId}/unpublish

*Requires: Admin*

### DELETE /api/courses/{courseId}

*Requires: Admin* - Soft delete

---

## Topic Endpoints

### POST /api/courses/{courseId}/topics

*Requires: Admin*

**Request**:
```json
{
  "titleEn": "Frontend Basics",
  "titleEs": "Fundamentos de Frontend",
  "order": 1
}
```

### GET /api/topics/{topicId}

### PUT /api/topics/{topicId}

*Requires: Admin*

### DELETE /api/topics/{topicId}

*Requires: Admin* - Soft delete, cascades to subtopics/sessions

---

## Subtopic Endpoints

### POST /api/topics/{topicId}/subtopics

*Requires: Admin*

### GET /api/subtopics/{subtopicId}

### PUT /api/subtopics/{subtopicId}

*Requires: Admin*

### DELETE /api/subtopics/{subtopicId}

*Requires: Admin*

---

## Session Endpoints

### POST /api/subtopics/{subtopicId}/sessions

*Requires: Admin*

**Request**:
```json
{
  "titleEn": "Introduction to HTML",
  "titleEs": "Introducción a HTML",
  "contentMarkdown": "# HTML Basics...",
  "videoUrl": "https://youtube.com/watch?v=...",
  "durationMinutes": 15,
  "order": 1
}
```

### GET /api/sessions/{sessionId}

### PUT /api/sessions/{sessionId}

*Requires: Admin*

### PATCH /api/sessions/{sessionId}/move

*Requires: Admin* - Move session to different subtopic

**Request**:
```json
{
  "newSubtopicId": "550e8400-e29b-41d4-a716-446655440031"
}
```

### DELETE /api/sessions/{sessionId}

*Requires: Admin*

---

## Accept-Language Header

```
Accept-Language: en          → English
Accept-Language: es          → Spanish
Accept-Language: es-MX       → Spanish (fallback)
Accept-Language: fr          → English (not supported, fallback)
```

---

## Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| EMAIL_ALREADY_EXISTS | 400 | Email is already registered |
| INVALID_CREDENTIALS | 401 | Email or password incorrect |
| EMAIL_NOT_VERIFIED | 403 | Email verification pending |
| REFRESH_TOKEN_EXPIRED | 401 | Refresh token has expired |
| VERIFICATION_TOKEN_EXPIRED | 400 | Verification token expired |
| COURSE_NOT_FOUND | 404 | Course does not exist |
| TOPIC_NOT_FOUND | 404 | Topic does not exist |
| SUBTOPIC_NOT_FOUND | 404 | Subtopic does not exist |
| SESSION_NOT_FOUND | 404 | Session does not exist |
| UNAUTHORIZED | 401 | Not authenticated |
| FORBIDDEN | 403 | Insufficient permissions |
| VALIDATION_FAILED | 400 | Validation errors |

---

## JWT Authentication

### Access Token
- Lifetime: 15 minutes
- Sent in: `Authorization: Bearer <token>`
- Contains: userId, email, role

### Refresh Token
- Lifetime: 7 days
- Stored in: HttpOnly cookie
- Rotates on each use

### Flow

```
1. User logs in
2. Server issues accessToken (15 min) + refreshToken (7 day)
3. refreshToken stored in HttpOnly cookie
4. accessToken expires after 15 min
5. Frontend uses refreshToken to get new accessToken
6. Server rotates refreshToken (invalidates old)
```

---

## Authorization Policies

```csharp
// Admin only
app.MapPost("/api/courses", ...).RequireAuthorization("AdminOnly");

// Authenticated users
app.MapGet("/api/courses/{id}", ...).RequireAuthorization("AuthenticatedUser");

// Public
app.MapPost("/api/auth/register", ...).AllowAnonymous();
```

---

## Related Documentation

- [Backend Architecture](./ARCHITECTURE.md) - How endpoints are implemented
- [Domain Model](./DOMAIN.md) - Entity definitions
- [Database](./DATABASE.md) - Schema details
