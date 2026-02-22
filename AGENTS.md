# 🤖 Protocolo de Agentes y Directrices del Proyecto

Este documento establece las reglas de comportamiento, gestión de contexto y flujos de trabajo obligatorios para cualquier IA que interactúe con este repositorio.

## 🧠 Gestión de Contexto Obligatoria

Para asegurar la continuidad y coherencia del proyecto, el agente **debe siempre**:

1.  **Cargar el Contexto:** Antes de iniciar cualquier tarea, leer y procesar la totalidad de los archivos ubicados en la carpeta `/context`.
2.  **Prioridad de Verdad:** En caso de conflicto entre las instrucciones de una tarea y el contenido de `/context`, se priorizará lo establecido en la carpeta de contexto.

## 🛠 Flujo de Trabajo y Persistencia Obligatoria

Cualquier intervención que suponga un cambio en el código, la arquitectura o la lógica debe seguir este protocolo de salida sin excepción:

### 1. Generación de tests

- **Acción:** Cada vez que se realice una modificación, corrección o mejora, se debe implementar un test que cubra la funcionalidad modificada usando el framework **xunit**.
- **Criterio:** Seguir las buenas prácticas de testing, incluyendo pruebas unitarias, de integración y end-to-end.
  - **Unit Tests:** Para lógica de negocio y componentes individuales.
  - **Integration Tests:** Para API endpoints y casos de uso.
  - **End-to-End Tests:** Para flujos completos de usuario o procesos críticos.

### 2. Documentación de Tareas (`/docs`)

- **Acción:** Generar o actualizar un archivo Markdown en la carpeta `/docs` por cada sesión de trabajo.
- **Nombre:** Formato `YYYY-MM-DD-descripcion-breve.md`.
- **Contenido:** Objetivo, cambios realizados, archivos afectados y la nueva versión establecida.

### 3. Registro de Cambios (`CHANGELOG.md`)

Mantener el archivo `CHANGELOG.md` siguiendo estrictamente las buenas prácticas de [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

- **Secciones Obligatorias:** `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`.
- **Referencia de Versión:** Cada entrada debe coincidir con la nueva versión indicada en Scalar/Swagger.

### 4. Otras Consideraciones

- **Usar Principios SOLID:** Asegurar que el código modificado o añadido sigue los principios SOLID para mantener la calidad y mantenibilidad del proyecto.
- **Usar primary constructors**
- **Usar Record types** para objetos inmutables.
- **Usar Minimal APIs** para exponer solo lo necesario y reducir el acoplamiento. Dentro de Endpoints, crear una carpeta por cada feature y dentro de esta, un endpoint por cada caso de uso.
  Ejemplo:

```
/API
    /Endpoints
        /Todos
            Complete.cs
            Copy.cs
            Create.cs
            Delete.cs
            Get.cs
            GetById.cs
        /Users
            GetById.cs
            Login.cs
            Permissions.cs
            Register.cs

/Application
    /Todos
        Complete.cs
        Copy.cs
        Create.cs
        Delete.cs
        Get.cs
        GetById.cs
    /Users
        GetById.cs
        Login.cs
        Permissions.cs
        Register.cs
```

- **Usar CQRS:** Separar claramente las operaciones de lectura y escritura para mejorar la escalabilidad y mantenibilidad del código.
- **Usar Scalar/OpenAPI:** Para documentar el API.
- **Implementar logging apropiado:** Asegurar que todas las operaciones críticas están debidamente registradas para facilitar la depuración y el monitoreo.
- **Usar Background Services:** Para tareas que requieran procesamiento asíncrono o en segundo plano, asegurando que no bloqueen el flujo principal de la aplicación.
- **Favorecer tipado explicito**: (esto es muy importante). Solo usar var cuando sea evidente.
- **Clases internal y sealed** siempre que sea posible. Para evitar que el código sea accesible desde fuera del assembly, promoviendo la encapsulación y reduciendo el acoplamiento.
- **Usar "is null" en lugar de "== null"** para evitar problemas con operadores de igualdad sobrecargados y mejorar la legibilidad del código.
- **Usar "is not null" en lugar de "!= null"** para las mismas razones mencionadas anteriormente, asegurando una verificación de nulidad más segura y clara.
- **Usar IOptions pattern** para la configuración, promoviendo una gestión de configuración más limpia y desacoplada.
- **Gestionar las excepciones de manera adecuada:** Implementar un manejo de errores robusto para con mensajes de error claros y útiles. Siempre que sea posible, serán manejadas a nivel de middleware.
- **Usar async/await:** Para operaciones asíncronas, asegurando que el código sea no bloqueante y eficiente.
- **Evitar el uso de "magic strings":** Utilizar constantes o enumeraciones para valores que se repiten, mejorando la mantenibilidad y reduciendo errores.
- **Usar Dependency Injection:** Para gestionar las dependencias de manera eficiente y promover un diseño desacoplado.
- **Seguir las convenciones de nomenclatura de C#:** Para clases, métodos, variables y otros elementos del código, asegurando una consistencia en todo el proyecto.
- **Utilizar Entity Framework Core:** Para la gestión de datos, aprovechando sus capacidades de mapeo objeto-relacional y migraciones.
- **Implementar autenticación y autorización**
* **GlobalUsings:** Para evitar la repetición de directivas `using` en cada archivo, promoviendo un código más limpio y organizado.

## 📝 Formato de Respuesta y Estilo

- **Idioma:** Español (salvo términos técnicos específicos).
- **Precisión:** Las respuestas deben ser técnicas y concisas.
- **Definición de Terminado (DoD):** Una tarea no se considera finalizada hasta que:
  1. Se han generado los tests correspondientes.
  2. Se ha creado el reporte en `/docs`.
  3. Se ha actualizado el `CHANGELOG.md`.

---

**Nota:** El incumplimiento de cualquiera de estos pasos resultará en una tarea marcada como incompleta.
