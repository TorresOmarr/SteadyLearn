# 🌍 I18N_STRATEGY.md - Multilenguaje Implementation

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Overview

SteadyLearn supports English (EN) and Spanish (ES) from Day 1. The strategy uses:
- Separate translation tables in the database
- Accept-Language header for API responses
- Validation messages with language codes
- Frontend-driven translation loading

---

## 🗂️ Database Strategy: Separate Translation Tables

### Why Separate Tables?
```
✓ Normalized (no redundant columns)
✓ Scales to N languages (not just 2)
✓ Easy to query specific language
✓ Independent translation updates
✓ Can filter out incomplete translations

✗ Requires joins (minor performance cost)
✗ Slightly more complex queries
```

### Pattern
```
Entity table          Translation table
Course            ←→  CourseTranslation
Topic             ←→  TopicTranslation
Subtopic          ←→  SubtopicTranslation
Session           ←→  SessionTranslation

Each translation row = (Entity, LanguageCode, content...)
Unique constraint = (Entity, LanguageCode)
```

### Schema Example
```
Course
├── Id
├── IsPublished
├── TotalDurationMinutes
└── Audit fields

CourseTranslation
├── Id
├── CourseId (FK)
├── LanguageCode (en, es)
├── Title
├── Description
└── Unique: (CourseId, LanguageCode)
```

### Data Example
```
Courses table:
  Id: 123
  IsPublished: false
  TotalDurationMinutes: 120

CourseTranslations table:
  Id: 1001
  CourseId: 123
  LanguageCode: en
  Title: "Introduction to Web Development"
  Description: "Learn the basics of web development"

  Id: 1002
  CourseId: 123
  LanguageCode: es
  Title: "Introducción al Desarrollo Web"
  Description: "Aprende los fundamentos del desarrollo web"
```

---

## 📡 API Endpoints & Accept-Language

### Request
```
GET /api/courses/123
Accept-Language: es

Server reads Accept-Language header
Loads Spanish translation
Returns response in Spanish
```

### Accept-Language Header
```
Accept-Language: en          → English (primary)
Accept-Language: es          → Spanish (Spain)
Accept-Language: es-MX       → Spanish (Mexico) → fallback to es
Accept-Language: fr          → French → NOT SUPPORTED, fallback to en
Accept-Language: es, en;q=0.9 → Spanish preferred, English fallback
```

### Response Format
```
GET /api/courses/123
Accept-Language: es

{
  "success": true,
  "data": {
    "id": "123",
    "title": "Introducción al Desarrollo Web",  ← Spanish
    "description": "Aprende los fundamentos...",  ← Spanish
    "isPublished": false,
    "topics": [...]
  }
}
```

---

## ✅ Validation & Error Messages

### Validation Message Format
```
Instead of:
  "message": "Email is required"

Use error code:
  "code": "EMAIL_REQUIRED"

Frontend translates based on Accept-Language
```

### Error Response
```
POST /api/auth/register
{
  "email": "",
  "password": "weak"
}

Response:
{
  "success": false,
  "error": {
    "code": "VALIDATION_FAILED",
    "details": [
      {
        "field": "email",
        "code": "EMAIL_REQUIRED"
      },
      {
        "field": "password",
        "code": "PASSWORD_TOO_WEAK"
      }
    ]
  }
}

Frontend translates codes:
  en:
    EMAIL_REQUIRED: "Email is required"
    PASSWORD_TOO_WEAK: "Password is too weak"
  
  es:
    EMAIL_REQUIRED: "El correo electrónico es requerido"
    PASSWORD_TOO_WEAK: "La contraseña es muy débil"
```

### Accept-Language Middleware
```csharp
// Common/Middleware/AcceptLanguageMiddleware.cs
public class AcceptLanguageMiddleware
{
    private readonly RequestDelegate _next;

    public AcceptLanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var language = context.Request.Headers["Accept-Language"]
            .ToString()
            .Split(',')[0]
            .Split(';')[0]
            .Trim()
            .ToLower();

        // Fallback to EN if language not supported
        context.Items["Language"] = language switch
        {
            "es" or "es-mx" or "es-ar" => "es",
            _ => "en"
        };

        await _next(context);
    }
}
```

---

## 🛠️ Implementation in Code

### Creating a Course (with both translations)
```csharp
// Command
public record CreateCourseCommand(
    string TitleEn,
    string TitleEs,
    string DescriptionEn,
    string DescriptionEs
) : IRequest<Result<CreateCourseResponse>>;

// Handler
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CreateCourseResponse>>
{
    private readonly ApplicationDbContext _db;

    public async Task<Result<CreateCourseResponse>> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        // Create course entity
        var course = new Course
        {
            Id = Guid.NewGuid(),
            IsPublished = false,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "admin@example.com"
        };

        // Create translations
        var courseTranslations = new List<CourseTranslation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                LanguageCode = "en",
                Title = request.TitleEn,
                Description = request.DescriptionEn
            },
            new()
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                LanguageCode = "es",
                Title = request.TitleEs,
                Description = request.DescriptionEs
            }
        };

        // Save
        _db.Courses.Add(course);
        _db.CourseTranslations.AddRange(courseTranslations);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new CreateCourseResponse(course.Id));
    }
}
```

### Querying with Language
```csharp
// Get course in specific language
public async Task<Course?> GetCourseAsync(Guid courseId, string language)
{
    return await _db.Courses
        .Include(x => x.CourseTranslations
            .Where(t => t.LanguageCode == language))
        .FirstOrDefaultAsync(x => x.Id == courseId);
}

// In endpoint
public static async Task GetCourse(
    Guid courseId,
    [FromServices] ApplicationDbContext db,
    HttpContext context)
{
    var language = context.Items["Language"] as string ?? "en";
    var course = await db.Courses
        .Include(x => x.CourseTranslations
            .Where(t => t.LanguageCode == language))
        .FirstOrDefaultAsync(x => x.Id == courseId);

    return Results.Ok(course);
}
```

---

## 📝 Error Code Constants

```csharp
// Common/Constants/ErrorCodes.cs
public static class ErrorCodes
{
    // Auth
    public const string INVALID_EMAIL_FORMAT = "INVALID_EMAIL_FORMAT";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string PASSWORD_TOO_WEAK = "PASSWORD_TOO_WEAK";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_NOT_VERIFIED = "EMAIL_NOT_VERIFIED";
    
    // Courses
    public const string COURSE_NOT_FOUND = "COURSE_NOT_FOUND";
    public const string COURSE_CANNOT_BE_EMPTY = "COURSE_CANNOT_BE_EMPTY";
    public const string COURSE_ALREADY_PUBLISHED = "COURSE_ALREADY_PUBLISHED";
    
    // Validation
    public const string VALIDATION_FAILED = "VALIDATION_FAILED";
    public const string REQUIRED_FIELD = "REQUIRED_FIELD";
}
```

---

## 🔄 Translation Seeding

### Add to Database.Seed
```csharp
// Seed course with translations
public static async Task SeedCoursesAsync(this ApplicationDbContext context)
{
    if (await context.Courses.AnyAsync())
        return;

    var course = new Course
    {
        Id = Guid.NewGuid(),
        IsPublished = false,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = "admin@example.com"
    };

    context.Courses.Add(course);

    context.CourseTranslations.AddRange(
        new CourseTranslation
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            LanguageCode = "en",
            Title = "Web Development Basics",
            Description = "Learn HTML, CSS, and JavaScript"
        },
        new CourseTranslation
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            LanguageCode = "es",
            Title = "Fundamentos de Desarrollo Web",
            Description = "Aprende HTML, CSS y JavaScript"
        }
    );

    await context.SaveChangesAsync();
}
```

---

## 🎨 Frontend i18n Translation

### JSON Translation Files
```
/client/public/locales/
├── en/
│   ├── common.json
│   ├── auth.json
│   └── courses.json
└── es/
    ├── common.json
    ├── auth.json
    └── courses.json
```

### Example Translation File
```json
// /client/public/locales/en/auth.json
{
  "email": "Email",
  "password": "Password",
  "login": "Log In",
  "register": "Sign Up",
  "errors": {
    "INVALID_EMAIL_FORMAT": "Email format is invalid",
    "EMAIL_ALREADY_EXISTS": "Email already registered",
    "PASSWORD_TOO_WEAK": "Password must be at least 8 characters with uppercase, lowercase, number, and special character",
    "INVALID_CREDENTIALS": "Email or password is incorrect"
  }
}

// /client/public/locales/es/auth.json
{
  "email": "Correo electrónico",
  "password": "Contraseña",
  "login": "Iniciar sesión",
  "register": "Registrarse",
  "errors": {
    "INVALID_EMAIL_FORMAT": "El formato de correo electrónico no es válido",
    "EMAIL_ALREADY_EXISTS": "El correo electrónico ya está registrado",
    "PASSWORD_TOO_WEAK": "La contraseña debe tener al menos 8 caracteres con mayúscula, minúscula, número y carácter especial",
    "INVALID_CREDENTIALS": "El correo electrónico o la contraseña es incorrecta"
  }
}
```

### i18n Hook
```typescript
// /client/src/hooks/useTranslate.ts
import { useEffect, useState } from 'react';

export const useTranslate = () => {
  const [language, setLanguage] = useState<'en' | 'es'>('en');
  const [translations, setTranslations] = useState<Record<string, any>>({});

  useEffect(() => {
    // Detect from browser or localStorage
    const lang = (navigator.language.startsWith('es') ? 'es' : 'en') as 'en' | 'es';
    setLanguage(lang);

    // Load translations
    import(`../locales/${lang}/auth.json`)
      .then(module => setTranslations(module.default))
      .catch(console.error);
  }, []);

  const t = (key: string, defaultValue?: string) => {
    return key.split('.').reduce((acc, part) => acc?.[part], translations) || defaultValue || key;
  };

  return { language, t, setLanguage };
};
```

---

## ✅ Multilenguaje Checklist

Before launching:

- [ ] All entity tables have translation tables
- [ ] Unique constraint (EntityId, LanguageCode) applied
- [ ] Accept-Language middleware added to pipeline
- [ ] Error codes used instead of hardcoded messages
- [ ] All CRUD operations create EN + ES translations
- [ ] Database seeding includes translations
- [ ] Frontend translation files for EN + ES
- [ ] i18n hook implemented
- [ ] Tests verify both languages work
- [ ] Validation messages support both languages

---

## 🔗 Related Documents

- **AGENTS.md** - Overall vision (multilenguaje from Day 1)
- **API_DESIGN.md** - Error codes + responses
- **DOMAIN_MODEL.md** - Translation tables
- **DATABASE.md** - Schema for translations
- **FRONTEND.md** - Frontend i18n

---

*Multilenguaje support requires careful planning but pays dividends in user reach.*
