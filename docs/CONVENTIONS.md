# Coding Conventions (Backend)

Guía breve para construir funcionalidades sin repasar todos los endpoints.

## Principios Rápidos
- Código aburrido > código ingenioso.
- Mutaciones sólo dentro de entidades (setters privados + métodos).
- `DateTimeOffset` para tiempos; almacenar como `timestamp with time zone`.
- Result Pattern para errores; sin lanzar excepciones de control de flujo.
- No generes ni modifiques migraciones (las hace el humano).

## Entidades: Patrón
- Setters privados; campos obligatorios inicializados en fábrica estática descriptiva (`Foo.Create(...)`).
- Métodos de comportamiento para cada mutación; actualiza audit (`UpdatedAt`, etc.) dentro del método.
- Soft delete: `IsDeleted` + `DeletedAt`; evita borrados físicos salvo GDPR.
- Invariantes en el modelo (p.ej. límites de tokens, estados válidos) y no en el handler.
- Usa `DateTimeOffset.UtcNow` dentro de la entidad al mutar tiempo.

## Pasos para Crear una Nueva Entidad
1) Define la fábrica (`Create`) con argumentos obligatorios y saneo básico (trim/normalize, lower en emails).
2) Declara métodos de mutación nombrados por intención (`Publish`, `SetProfile`, `MarkDeleted`).
3) Implementa reglas dentro de la entidad (validaciones de negocio, transiciones válidas).
4) Añade configuración Fluent API en `api/src/Data/Configurations/{Entity}Configuration.cs` (propiedades, índices, relaciones, conversiones de fecha/hora).
5) Expón la entidad vía vertical slice (Command/Query, Validator, Handler, Endpoint) sólo usando métodos del modelo.
6) Considera traducciones EN/ES si aplica (tablas `*Translation`).

## Vertical Slice (resumen)
- Command/Query: sólo datos.
- Validator: FluentValidation.
- Handler: orquesta; no muta propiedades directas, llama métodos de dominio.
- Endpoint: mapea HTTP → Mediator → Result.

## Naming y Organización
- Clases/records: PascalCase; propiedades públicas PascalCase; campos privados `_camelCase`.
- Un tipo por archivo, ubicado en su carpeta de feature: `/api/src/Modules/{Modulo}/{Feature}/`.
- Commands/Queries terminan en `Command`/`Query`; handlers `CommandHandler`/`QueryHandler`.
- Configuraciones EF: `{Entity}Configuration` en `Data/Configurations`.
- Services compartidos en `Common/{Área}` (Security, Extensions, Behaviors, etc.).

## Validación y Errores
- Usa FluentValidation para entrada; nada de `if` manual en endpoint.
- Emplea `ErrorCodes` conocidos; Result Pattern (`Result<T>.Success/Failure`).
- Respuestas HTTP mapean `Result` sin excepciones personalizadas.

## Datos y EF Core
- `DateTimeOffset` siempre; columnas `timestamp with time zone`.
- Índices y unicidad en configuraciones; evita anotaciones en entidades.
- Relaciones y delete behavior explícitos; preferir restrict + soft delete.
- No tocar ni generar migraciones; coordina con humano.

## Seguridad / Auth
- Tokens JWT de acceso (15m) + refresh en cookie HttpOnly (rotación, 1 activo + 5 históricos).
- Hashea passwords con bcrypt mediante `IPasswordHasher`.
- No exponer detalles sensibles en errores.

## Testing
- Unit tests para servicios y dominio (métodos de entidad). 
- Handlers probados con dobles de infraestructura o `DbContext` en memoria según patrón del repo.
- Valida casos felices y de error (ErrorCodes adecuados).

## I18N
- Traducciones EN y ES donde aplique; claves únicas `(EntityId, LanguageCode)`.
- No hardcodear strings que deban traducirse.

## Checklist Rápido antes de PR
- Fábrica + métodos de mutación en la entidad.
- Setters privados; sin asignaciones directas en handlers.
- Validadores listos; errores usan `ErrorCodes`.
- Configuración EF creada/actualizada; índices definidos.
- Tests mínimos para lógica nueva.
- Sin migraciones generadas por el agente.
