# 📡 API_DESIGN.md - REST Endpoints & Contracts

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📋 Overview

The API follows RESTful conventions with:
- Clear resource-based endpoints
- Consistent response formats
- Standardized error handling
- Multilenguaje support via Accept-Language

---

## 🔐 Authentication Endpoints

### Register
```
POST /api/auth/register
Content-Type: application/json

Request:
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "passwordConfirm": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}

Success Response (201 Created):
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "message": "Verification email sent"
  },
  "code": "REGISTRATION_SUCCESSFUL"
}

Error Response (400 Bad Request):
{
  "success": false,
  "error": {
    "code": "EMAIL_ALREADY_EXISTS",
    "message": "Email already registered"
  }
}
```

### Login
```
POST /api/auth/login
Content-Type: application/json

Request:
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

Success Response (200 OK):
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
Headers:
  Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict

Error Response (401 Unauthorized):
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Email or password is incorrect"
  }
}
```

### Refresh Token
```
POST /api/auth/refresh
Authorization: Bearer <accessToken>
Cookie: refreshToken=<refreshToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs..."
  }
}
Headers:
  Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict

Error Response (401 Unauthorized):
{
  "success": false,
  "error": {
    "code": "REFRESH_TOKEN_EXPIRED",
    "message": "Please login again"
  }
}
```

### Verify Email
```
POST /api/auth/verify-email
Content-Type: application/json

Request:
{
  "token": "verification-token-from-email"
}

Success Response (200 OK):
{
  "success": true,
  "data": {
    "message": "Email verified successfully"
  }
}

Error Response (400 Bad Request):
{
  "success": false,
  "error": {
    "code": "VERIFICATION_TOKEN_EXPIRED",
    "message": "Token has expired, request a new one"
  }
}
```

### Request Password Reset
```
POST /api/auth/forgot-password
Content-Type: application/json

Request:
{
  "email": "user@example.com"
}

Success Response (200 OK):
{
  "success": true,
  "data": {
    "message": "Password reset email sent"
  }
}
Note: Always returns success (security best practice)
```

### Reset Password
```
POST /api/auth/reset-password
Content-Type: application/json

Request:
{
  "token": "reset-token-from-email",
  "newPassword": "NewPassword123!",
  "newPasswordConfirm": "NewPassword123!"
}

Success Response (200 OK):
{
  "success": true,
  "data": {
    "message": "Password reset successfully"
  }
}
```

### Logout
```
POST /api/auth/logout
Authorization: Bearer <accessToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "message": "Logged out successfully"
  }
}
Headers:
  Set-Cookie: refreshToken=; Max-Age=0; HttpOnly
```

---

## 📚 Course Endpoints

### Create Course
```
POST /api/courses
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Introduction to Web Development",
  "titleEs": "Introducción al Desarrollo Web",
  "descriptionEn": "Learn web development basics",
  "descriptionEs": "Aprende los fundamentos del desarrollo web"
}

Success Response (201 Created):
{
  "success": true,
  "data": {
    "courseId": "550e8400-e29b-41d4-a716-446655440001",
    "titleEn": "Introduction to Web Development",
    "titleEs": "Introducción al Desarrollo Web",
    "isPublished": false,
    "totalDurationMinutes": 0
  }
}

Authorization: Admin Only
```

### Get Course
```
GET /api/courses/{courseId}
Authorization: Bearer <accessToken>
Accept-Language: en (default: en, es)

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "title": "Introduction to Web Development",
    "description": "Learn web development basics",
    "isPublished": false,
    "totalDurationMinutes": 120,
    "topics": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440010",
        "title": "HTML Basics",
        "order": 1,
        "subtopics": [...]
      }
    ],
    "createdAtUtc": "2025-01-10T12:00:00Z",
    "createdBy": "admin@example.com"
  }
}

Supports: Admin + Student (only published)
```

### Get All Courses
```
GET /api/courses?page=1&pageSize=10&published=true
Authorization: Bearer <accessToken>
Accept-Language: en

Success Response (200 OK):
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "title": "Introduction to Web Development",
        "description": "Learn web development basics",
        "isPublished": true,
        "totalDurationMinutes": 120
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
}

Query Parameters:
  page: int (default 1)
  pageSize: int (default 10)
  published: boolean (optional, filters published courses)
  search: string (optional, searches in title/description)
```

### Update Course
```
PUT /api/courses/{courseId}
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Updated Title EN",
  "titleEs": "Updated Title ES",
  "descriptionEn": "Updated description EN",
  "descriptionEs": "Updated description ES"
}

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "titleEn": "Updated Title EN",
    "titleEs": "Updated Title ES",
    ...
  }
}

Authorization: Admin Only
```

### Publish Course
```
PATCH /api/courses/{courseId}/publish
Authorization: Bearer <accessToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "isPublished": true
  }
}

Authorization: Admin Only
```

### Unpublish Course
```
PATCH /api/courses/{courseId}/unpublish
Authorization: Bearer <accessToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "isPublished": false
  }
}

Authorization: Admin Only
```

### Delete Course
```
DELETE /api/courses/{courseId}
Authorization: Bearer <accessToken>

Success Response (204 No Content)

Note: Soft delete - data is preserved, just marked as deleted.

Authorization: Admin Only
```

---

## 📖 Topic Endpoints

### Create Topic
```
POST /api/courses/{courseId}/topics
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Frontend Basics",
  "titleEs": "Fundamentos de Frontend",
  "order": 1
}

Success Response (201 Created):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440020",
    "courseId": "550e8400-e29b-41d4-a716-446655440001",
    "titleEn": "Frontend Basics",
    "titleEs": "Fundamentos de Frontend",
    "order": 1
  }
}

Authorization: Admin Only
```

### Get Topic
```
GET /api/topics/{topicId}
Authorization: Bearer <accessToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440020",
    "courseId": "550e8400-e29b-41d4-a716-446655440001",
    "title": "Frontend Basics",
    "order": 1,
    "subtopics": [...]
  }
}
```

### Update Topic
```
PUT /api/topics/{topicId}
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Updated Frontend Basics",
  "titleEs": "Fundamentos de Frontend Actualizado",
  "order": 2
}

Success Response (200 OK):
{
  "success": true,
  "data": { ... }
}

Authorization: Admin Only
```

### Delete Topic
```
DELETE /api/topics/{topicId}
Authorization: Bearer <accessToken>

Success Response (204 No Content)

Authorization: Admin Only
```

---

## 🏷️ Subtopic Endpoints

### Create Subtopic
```
POST /api/topics/{topicId}/subtopics
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "HTML Tags",
  "titleEs": "Etiquetas HTML",
  "order": 1
}

Success Response (201 Created):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440030",
    "topicId": "550e8400-e29b-41d4-a716-446655440020",
    "titleEn": "HTML Tags",
    "titleEs": "Etiquetas HTML",
    "order": 1
  }
}

Authorization: Admin Only
```

### Get Subtopic
```
GET /api/subtopics/{subtopicId}
Authorization: Bearer <accessToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440030",
    "topicId": "550e8400-e29b-41d4-a716-446655440020",
    "title": "HTML Tags",
    "order": 1,
    "sessions": [...]
  }
}
```

### Update Subtopic
```
PUT /api/subtopics/{subtopicId}
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Updated HTML Tags",
  "titleEs": "Etiquetas HTML Actualizado",
  "order": 2
}

Success Response (200 OK):
{
  "success": true,
  "data": { ... }
}

Authorization: Admin Only
```

### Delete Subtopic
```
DELETE /api/subtopics/{subtopicId}
Authorization: Bearer <accessToken>

Success Response (204 No Content)

Authorization: Admin Only
```

---

## 📝 Session Endpoints

### Create Session
```
POST /api/subtopics/{subtopicId}/sessions
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Introduction to HTML",
  "titleEs": "Introducción a HTML",
  "contentMarkdown": "# HTML Basics\n\nHTML is a markup language...",
  "videoUrl": "https://youtube.com/watch?v=dQw4w9WgXcQ",
  "durationMinutes": 15,
  "documentationSource": "https://mdn.mozilla.org/en-US/docs/Web/HTML",
  "order": 1
}

Success Response (201 Created):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440040",
    "subtopicId": "550e8400-e29b-41d4-a716-446655440030",
    "titleEn": "Introduction to HTML",
    "titleEs": "Introducción a HTML",
    "order": 1,
    "videoUrl": "https://youtube.com/watch?v=dQw4w9WgXcQ",
    "durationMinutes": 15
  }
}

Authorization: Admin Only
```

### Get Session
```
GET /api/sessions/{sessionId}
Authorization: Bearer <accessToken>

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440040",
    "subtopicId": "550e8400-e29b-41d4-a716-446655440030",
    "title": "Introduction to HTML",
    "contentMarkdown": "# HTML Basics\n\nHTML is a markup language...",
    "videoUrl": "https://youtube.com/watch?v=dQw4w9WgXcQ",
    "durationMinutes": 15,
    "order": 1
  }
}
```

### Update Session
```
PUT /api/sessions/{sessionId}
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "titleEn": "Updated Introduction to HTML",
  "titleEs": "Introducción a HTML Actualizado",
  "contentMarkdown": "# HTML Basics...",
  "videoUrl": "https://youtube.com/watch?v=dQw4w9WgXcQ",
  "durationMinutes": 20,
  "order": 1
}

Success Response (200 OK):
{
  "success": true,
  "data": { ... }
}

Authorization: Admin Only
```

### Move Session
```
PATCH /api/sessions/{sessionId}/move
Authorization: Bearer <accessToken>
Content-Type: application/json

Request:
{
  "newSubtopicId": "550e8400-e29b-41d4-a716-446655440031"
}

Success Response (200 OK):
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440040",
    "subtopicId": "550e8400-e29b-41d4-a716-446655440031"
  }
}

Note: Sessions can move between subtopics, topics cannot move between courses.

Authorization: Admin Only
```

### Delete Session
```
DELETE /api/sessions/{sessionId}
Authorization: Bearer <accessToken>

Success Response (204 No Content)

Authorization: Admin Only
```

---

## 📊 Common Response Structure

### Success Response
```json
{
  "success": true,
  "data": { /* payload */ },
  "code": "OPERATION_SUCCESSFUL"
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable message (EN by default, respects Accept-Language)"
  }
}
```

### Validation Error Response
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_FAILED",
    "details": [
      {
        "field": "email",
        "code": "INVALID_EMAIL_FORMAT",
        "message": "Email format is invalid"
      },
      {
        "field": "password",
        "code": "PASSWORD_TOO_WEAK",
        "message": "Password must be at least 8 characters"
      }
    ]
  }
}
```

---

## 🌍 Accept-Language Header

All endpoints support multilenguaje via Accept-Language:

```
GET /api/courses/123
Accept-Language: es

Response will include Spanish translations (if available).
Error messages will also be in Spanish.
```

Supported languages:
- `en` (English) - Default
- `es` (Spanish)

---

## 🔑 Error Codes

Common error codes returned by the API:

| Code | HTTP Status | Description |
|------|---|---|
| EMAIL_ALREADY_EXISTS | 400 | Email is already registered |
| INVALID_CREDENTIALS | 401 | Email or password is incorrect |
| EMAIL_NOT_VERIFIED | 403 | Email verification pending |
| REFRESH_TOKEN_EXPIRED | 401 | Refresh token has expired |
| VERIFICATION_TOKEN_EXPIRED | 400 | Verification token has expired |
| COURSE_NOT_FOUND | 404 | Course does not exist |
| TOPIC_NOT_FOUND | 404 | Topic does not exist |
| SUBTOPIC_NOT_FOUND | 404 | Subtopic does not exist |
| SESSION_NOT_FOUND | 404 | Session does not exist |
| UNAUTHORIZED | 401 | Not authenticated |
| FORBIDDEN | 403 | Insufficient permissions |
| VALIDATION_FAILED | 400 | Validation errors |

---

## 🔗 Related Documents

- **ARCHITECTURE.md** - How endpoints are implemented
- **AUTH_IMPLEMENTATION.md** - Auth flow details
- **I18N_STRATEGY.md** - How multilenguaje works
- **AGENTS.md** - Overall vision

---

*Boring, consistent, predictable API design.*
