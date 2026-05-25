El problema es que estás usando texto plano en un archivo .md (Markdown). Para que los títulos, listas y bloques de código se vean profesionales en GitHub, necesitas usar la sintaxis de Markdown (hachas # para títulos, asteriscos * para negritas, etc.).

Copia y pega este contenido directamente en tu archivo README.md. He aplicado el formato correcto para que GitHub lo renderice perfectamente:

Documentación Técnica de Arquitectura: TractorEcommerce Backend
1. Patrón Arquitectónico: Monolito Modular
El sistema implementa un enfoque de Monolito Modular estructurado en un único repositorio físico y desplegado como una sola unidad de ejecución (Host).

1.1. Reglas de Aislamiento e Integridad
Fronteras de Dominio (Bounded Contexts): Cada módulo representa un contexto delimitado del negocio.

Autonomía de Datos: No existen consultas transversales (JOINs) entre tablas de dominios distintos.

Comunicación Desacoplada: Interacción asíncrona mediante eventos de integración.

2. Topología del Workspace
Plaintext
TractorEcommerce/
├── .github/workflows/          # CI/CD Pipelines
├── docker-compose.yml          # Infraestructura (PostgreSQL, Kafka)
├── TractorEcommerce.sln        # .NET 10.0 Solution
└── src/                        
    ├── Host/                   # TractorEcommerce.Api
    └── Modules/                # Componentes Encapsulados
        ├── Catalog/, Cart/, Order/, Inventory/
        └── Shared/             # Contratos y Eventos
3. Arquitectura Interna (Clean Architecture)
3.1. Capa de Dominio (.Domain)
Agregados y Entidades: Modelos con identidad única.

Objetos de Valor (Value Objects): Estructuras inmutables.

Excepciones de Dominio: Fallos fuertemente tipados.

3.2. Capa de Aplicación (.Application)
Handlers / UseCases: Lógica procedimental.

CQRS: Segregación estricta de Commands y Queries.

3.3. Capa de Infraestructura (.Infrastructure)
Persistencia: Mapeos de EF Core.

Consumidores: Workers para Apache Kafka.

4. Capa Host: TractorEcommerce.Api
Endpoints: REST versionado.

Validación: Esquemas formales.

Tratamiento de Errores: Middleware global (RFC 7807).

5. Estrategia de Comunicación (EDA)
Para mitigar el acoplamiento, utilizamos una Arquitectura Guiada por Eventos:

Fragmento de código
sequenceDiagram
    participant User
    participant Cart as Módulo Cart
    participant Kafka as Apache Kafka
    participant Order as Módulo Order
    participant Inv as Módulo Inventory

    User->>Cart: Finalizar compra
    Cart->>Cart: Persistir estado (Transacción Local)
    Cart->>Kafka: Publica CheckoutRequested
    Kafka-->>Order: Consume evento
    Order->>Order: Crear Orden (Transacción Local)
    Order->>Kafka: Publica OrderPlacedEvent
    Kafka-->>Inv: Consume evento
    Inv->>Inv: Descuenta Stock (Transacción Local)
6. Stack Tecnológico
Runtime: .NET 10.0 (C#).

Base de Datos: PostgreSQL (UUIDs, JSONB).

Broker: Apache Kafka (Confluent.Kafka).

Observabilidad: Serilog (JSON) + TraceId.

7. Estrategia de Testing
Unitarias: Lógica de negocio pura.

Integración: Repositorios y ORM.

Cobertura: Reportes automáticos en CI/CD.

8. Guía de Inicio (Getting Started)
Clonar: git clone <url>

Levantar: docker-compose up -d

Migrar: dotnet ef database update --project src/Host/TractorEcommerce.Api

9. API Reference
Acceso: /swagger una vez iniciada la app.

Ejemplo: POST /api/v1/orders requiere CartId y ShippingDetails.

10. Observabilidad y Resiliencia
Observabilidad: Ready para ELK/Loki mediante logs estructurados.

Seguridad: [JWT Bearer Authentication].

Resiliencia: Políticas de reintento mediante Polly.