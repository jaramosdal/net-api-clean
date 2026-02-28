# Soliss.NuGetRepo.Mediator

## Índice

- [Introducción](#introducción)
- [Arquitectura del Proyecto](#arquitectura-del-proyecto)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Patrón Mediator](#patrón-mediator)
- [Instalación y Configuración](#instalación-y-configuración)
- [Ejemplos de Uso](#ejemplos-de-uso)
- [API Reference](#api-reference)
- [Flujo de Trabajo de Desarrollo](#flujo-de-trabajo-de-desarrollo)
- [Pipeline de CI/CD](#pipeline-de-cicd)
- [Testing](#testing)
- [Contribuir al Proyecto](#contribuir-al-proyecto)

## Introducción

**Soliss.NuGetRepo.Mediator** es una librería .NET que implementa el patrón de diseño **Mediator** para desacoplar las comunicaciones entre diferentes componentes de una aplicación. Este proyecto forma parte del ecosistema de herramientas internas de Soliss, diseñado para mejorar la arquitectura de software y promover las buenas prácticas de desarrollo.

### Objetivos del Proyecto

- **Desacoplamiento**: Reducir las dependencias directas entre componentes de la aplicación
- **Mantenibilidad**: Facilitar el mantenimiento y evolución del código
- **Testabilidad**: Mejorar la capacidad de realizar pruebas unitarias
- **Reutilización**: Proporcionar una solución estándar para la comunicación entre módulos
- **Escalabilidad**: Permitir el crecimiento del sistema sin aumentar la complejidad

### Contexto

Este proyecto surge de la necesidad de estandarizar la comunicación entre diferentes módulos en las aplicaciones desarrolladas por Soliss, aplicando principios SOLID y los patrones CQS y mediator, ampliamente reconocidos en la industria.

## Arquitectura del Proyecto

### Visión General

El proyecto implementa el patrón **Mediator**, donde todos los componentes se comunican a través de un mediador central, eliminando las referencias directas entre ellos.

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Component A   │    │   Component B   │    │   Component C   │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                        ┌────────▼────────┐
                        │    MEDIATOR     │
                        │   (Central)     │
                        └─────────────────┘
```

### Componentes Principales

#### 1. **IMediator Interface**

Interface principal que define el contrato para el mediador.

#### 2. **Mediator Implementation**

Implementación concreta del mediador que gestiona el enrutamiento de mensajes.

#### 3. **Request/Response Models**

Modelos que definen la estructura de las comunicaciones.

#### 4. **Handlers**

Componentes que procesan los diferentes tipos de requests.

## Tecnologías Utilizadas

| Tecnología   | Versión        | Propósito                 |
| ------------ | -------------- | ------------------------- |
| .NET         | Core/Framework | Plataforma de desarrollo  |
| C#           | Latest         | Lenguaje de programación  |
| NuGet        | -              | Sistema de paquetes       |
| Azure DevOps | -              | CI/CD y gestión de código |
| xUnit        | -              | Framework de testing      |

## Estructura del Proyecto

```
Soliss.NuGetRepo.Mediator/
├── src/
│   └── Soliss.NuGetRepo.Mediator/
│       ├── IMediator.cs
│       ├── IPipelineBehavior.cs
│       ├── IRequest.cs
│       ├── IRequestHandler.cs
│       ├── Mediator.cs
│       ├── Soliss.NuGetRepo.Mediator.csproj.cs
│       └── Unit.cs
├── tests/
│   └── Soliss.NuGetRepo.Mediator.Tests/
│       ├── MediatorFixture.cs
│       ├── MediatorTests.cs
│       └── Soliss.NuGetRepo.Mediator.Tests.csproj
├── azure-pipelines.yml
├── README.md
└── Soliss.NuGetRepo.Mediator.sln
```

## Patrón Mediator

### ¿Qué es el Patrón Mediator?

El patrón Mediator define cómo un conjunto de objetos interactúan entre sí. En lugar de que los objetos se comuniquen directamente, lo hacen a través de un objeto mediador que gestiona estas comunicaciones.

### Ventajas de la Implementación

1. **Bajo Acoplamiento**: Los componentes no necesitan conocer los detalles de otros componentes
2. **Centralización**: Toda la lógica de comunicación está centralizada
3. **Extensibilidad**: Fácil agregar nuevos tipos de mensajes y handlers
4. **Testabilidad**: Cada handler puede ser probado de forma independiente

### Flujo de Comunicación

```csharp
// 1. Cliente envía request através del mediator
var result = await mediator.Send(new GetUserRequest(userId));

// 2. Mediator localiza el handler apropiado
// 3. Handler procesa el request
// 4. Handler retorna el resultado
// 5. Mediator devuelve el resultado al cliente
```

## Instalación y Configuración

### Instalación via NuGet

```bash
# Instalar desde el feed interno de Soliss
dotnet add package Soliss.NuGetRepo.Mediator --source https://pkgs.dev.azure.com/soliss/_packaging/SolissPackages/nuget/v3/index.json
```

## Ejemplos de Uso

### 1. Definir un Request

```csharp
using Soliss.NuGetRepo.Mediator;

public class GetUserRequest : IRequest<UserDto>
{
    public int UserId { get; set; }

    public GetUserRequest(int userId)
    {
        UserId = userId;
    }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 2. Implementar un Handler

```csharp
using Soliss.NuGetRepo.Mediator;

public class GetUserHandler : IRequestHandler<GetUserRequest, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(IUserRepository userRepository, ILogger<GetUserHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Fetching user with ID: {request.UserId}");

        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            throw new UserNotFoundException($"User with ID {request.UserId} not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
```

### 3. Usar el Mediator en un Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _mediator.Send(new GetUserRequest(id));
            return Ok(user);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }
}
```

## API Reference

### IMediator Interface

| Método                         | Descripción                         | Parámetros                    | Retorno                          |
| ------------------------------ | ----------------------------------- | ----------------------------- | -------------------------------- |
| `Send<T>(IRequest<T> request)` | Envía un request y espera respuesta | `request`: Request a procesar | `Task<T>`: Resultado del request |

### IRequest Interfaces

| Interface     | Descripción                                                                                    | Uso                                       |
| ------------- | ---------------------------------------------------------------------------------------------- | ----------------------------------------- |
| `IRequest<T>` | Request (command o query) que retorna un resultado de tipo T                                   | Consultas, operaciones que retornan datos |
| `ICommand<T>` | Command que retorna un resultado de tipo T. Mismo resultado que IRequest<T>, aporta semántica. | Operaciones que retornan datos o Unit     |
| `IQuery<T>`   | Query que retorna un resultado de tipo T. Mismo resultado que IRequest<T>, aporta semántica.   | Consultas que retornan datos              |

### IRequestHandler Interfaces

| Interface                              | Descripción                         | Método                                |
| -------------------------------------- | ----------------------------------- | ------------------------------------- |
| `IRequestHandler<TRequest, TResponse>` | Handler para requests con respuesta | `Handle(TRequest, CancellationToken)` |
