# Domain Events en este proyecto (guía para agentes de IA)

## Objetivo

Este documento define **cómo gestionamos los Domain Events** en este repositorio y cómo un agente debe crear nuevos eventos y manejadores de forma consistente.

El proyecto usa `Soliss.NuGetRepo.Mediator`, pero los eventos de dominio (`IDomainEvent`) **no se publican directamente** como notificaciones del mediator. Para eso existe un **adaptador**.

---

## Decisión de diseño: adaptador entre Domain Events y Mediator

### Problema

`IDomainEvent` pertenece al dominio y no está acoplado a `INotification` del mediator.

### Solución aplicada

Se implementa un wrapper genérico:

- `DomainEventNotification<TDomainEvent> : INotification`
- Recibe el evento real en su propiedad `Event`.

Y un contrato de handler específico de dominio:

- `IDomainEventHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>`
- Expone `Handle(TDomainEvent domainEvent, CancellationToken cancellationToken)` para que los handlers trabajen con el evento de dominio puro.
- La implementación explícita de `INotificationHandler` desempaqueta el wrapper automáticamente.

Con esto conseguimos:

1. Mantener el dominio limpio (sin dependencia directa de `INotification`).
2. Reutilizar el pipeline de `Soliss.NuGetRepo.Mediator`.
3. Simplificar los handlers (reciben el evento de dominio, no el wrapper).

---

## Componentes implicados

- `Domain.DomainEvents.IDomainEvent`: contrato marcador de evento de dominio.
- `Domain.Entity<TId>`: almacena y expone `DomainEvents`, y permite `RaiseDomainEvent(...)`.
- `Application.Abstractions.Events.IDomainEventPublisher`: puerto para publicar eventos.
- `Infrastructure.Services.DomainEventPublisher`: adaptador que envuelve en `DomainEventNotification<T>` y llama a `IMediator.Publish(...)`.
- `Application.Abstractions.Events.IDomainEventHandler<T>`: contrato recomendado para handlers.
- `Infrastructure.Database.ApplicationDbContext`: publica eventos tras `SaveChangesAsync` exitoso y luego limpia eventos.

---

## Flujo completo de publicación

1. Una entidad de dominio llama a `RaiseDomainEvent(...)`.
2. `ApplicationDbContext.SaveChangesAsync(...)` detecta entidades con eventos (`IHasDomainEvents`).
3. Se persisten cambios con `base.SaveChangesAsync(...)`.
4. Si la persistencia tiene éxito, se publica cada evento vía `IDomainEventPublisher`.
5. `DomainEventPublisher` crea `DomainEventNotification<TDomainEvent>` y hace `mediator.Publish(...)`.
6. El mediator resuelve `INotificationHandler<DomainEventNotification<TDomainEvent>>`.
7. Los handlers que implementan `IDomainEventHandler<TDomainEvent>` reciben directamente `TDomainEvent`.
8. Al finalizar, el contexto ejecuta `ClearDomainEvents()` por entidad.

> Importante: en este diseño, los eventos se publican **después** de persistir en base de datos.

---

## Cómo crear un nuevo Domain Event

### Reglas

- Ubicar el evento en `src/Domain/DomainEvents` o subcarpeta por feature.
- Debe implementar `IDomainEvent`.
- Para inmutabilidad, usar preferentemente `record`.
- Incluir solo datos de dominio necesarios para reaccionar al evento.

### Plantilla

```csharp
using Domain.DomainEvents;

namespace Domain.DomainEvents.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Email) : IDomainEvent;
```

### Cómo dispararlo desde una entidad

```csharp
public void Register(string email)
{
    // lógica de negocio...

    RaiseDomainEvent(new UserRegisteredDomainEvent(Id, email));
}
```

---

## Cómo crear un nuevo handler de Domain Event

### Reglas

- El handler va en la capa `Application` (casos de uso/reacciones de aplicación).
- Implementar `IDomainEventHandler<TDomainEvent>` (no la interfaz del mediator directamente, salvo necesidad concreta).
- Clase `internal sealed` siempre que sea posible.
- Usar constructor primario para dependencias.
- `Handle(...)` debe ser `async` si hay I/O y respetar `CancellationToken`.

### Plantilla

```csharp
using Application.Abstractions.Events;
using Domain.DomainEvents.Users;
using Microsoft.Extensions.Logging;

namespace Application.Users.Events;

internal sealed class SendWelcomeEmailOnUserRegisteredHandler(
    ILogger<SendWelcomeEmailOnUserRegisteredHandler> logger)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Usuario registrado: {UserId} - {Email}", domainEvent.UserId, domainEvent.Email);
        return Task.CompletedTask;
    }
}
```

### Registro y descubrimiento

- `AddApplication()` ejecuta `services.AddMediator(typeof(DependencyInjection).Assembly);`
- Por tanto, los handlers de eventos deben estar en el assembly de `Application` para ser descubiertos automáticamente.

---

## Reglas operativas para agentes de IA

1. **No** publicar eventos desde la capa Domain con mediator directamente.
2. **No** acoplar `IDomainEvent` a `INotification`.
3. Crear evento (`IDomainEvent`) + dispararlo con `RaiseDomainEvent(...)` en la entidad.
4. Crear handler en `Application` implementando `IDomainEventHandler<TEvent>`.
5. Mantener handlers idempotentes cuando sea posible (evitar efectos duplicados).
6. Evitar lógica de negocio crítica dentro del handler si debe formar parte de la transacción principal.
7. Si se modifica el flujo de eventos, cubrir con tests unitarios/integración en `tests/Infrastructure.UnitTests/DomainEvents` y `tests/Domain.UnitTests/DomainEvents`.

---

## Checklist rápido antes de cerrar una tarea

- [ ] El evento nuevo implementa `IDomainEvent`.
- [ ] La entidad dispara el evento con `RaiseDomainEvent(...)`.
- [ ] Existe al menos un handler en `Application` usando `IDomainEventHandler<TEvent>`.
- [ ] No hay dependencia de mediator en Domain.
- [ ] Se han añadido/ajustado tests de eventos según el cambio.
