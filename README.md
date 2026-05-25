Documentación Técnica de Arquitectura: TractorEcommerce Backend
1. Patrón Arquitectónico: Monolito Modular
El sistema implementa un enfoque de Monolito Modular estructurado en un único repositorio físico y desplegado como una sola unidad de ejecución (Host). A diferencia de un monolito tradicional, este diseño impone un aislamiento rígido entre los distintos contextos de negocio mediante fronteras de diseño y dependencias de compilación estrictas, impidiendo la degradación hacia un acoplamiento cruzado ("Big Ball of Mud").

1.1. Reglas de Aislamiento e Integridad
Fronteras de Dominio (Bounded Contexts): Cada módulo representa un contexto delimitado del negocio. Las reglas de validación y modelos internos están completamente encapsulados dentro de sus respectivas fronteras de código.

Autonomía de Datos: No existen consultas transversales (como operaciones JOIN en base de datos) entre tablas que pertenezcan a dominios distintos. Cada módulo gestiona de manera exclusiva su propio subesquema o tablas asignadas.

Comunicación Desacoplada: Toda interacción que requiera modificar el estado de múltiples módulos concurrentemente se realiza de manera asíncrona mediante eventos de integración, eliminando la dependencia en memoria.

2. Topología del Workspace (Solution Layout)
La jerarquía del código fuente está diseñada para segregar la capa de presentación global de los componentes de lógica pura y sus implementaciones de bajo nivel:

TractorEcommerce/
│
├── .github/workflows/          # Orquestación de Integración Continua (CI/CD)
│   └── backend-ci-cd.yml       # Pipelines automáticos de compilación, análisis estático y testing
│
├── docker-compose.yml          # Infraestructura local parametrizada (PostgreSQL, Apache Kafka)
├── TractorEcommerce.sln        # Archivo de solución unificado de la plataforma (.NET 10.0)
│
└── src/                        
    ├── Host/                   # Capa Transversal de Presentación y Orquestación
    │   └── TractorEcommerce.Api/
    │
    └── Modules/                # Componentes Encapsulados de Negocio (Fronteras Físicas)
        ├── Catalog/            # Gestión de productos, variantes jerárquicas y sucursales
        ├── Cart/               # Control de carritos de compra y lógica transitoria de sesión
        ├── Order/              # Orquestación del ciclo de vida y procesamiento de órdenes
        ├── Inventory/          # Control de existencias físicas en tiempo real por SKU
        └── Shared/             # Contratos globales, DTOs del sistema y Eventos de Integración
3. Arquitectura Interna de los Módulos (Clean Architecture)
Cada módulo dentro del directorio src/Modules/ está subdividido en proyectos independientes para garantizar la separación de responsabilidades y asegurar que las reglas del negocio no dependan de frameworks, ORMs ni infraestructura externa.

3.1. Capa de Dominio (.Domain)
Representa el núcleo inviolable del negocio. No contiene dependencias a librerías externas ni de acceso a datos.

Agregados y Entidades: Modelos con identidad única que encapsulan el estado y las invariantes del negocio (ej. Cart, CartItem, Order). Las mutaciones de estado se realizan únicamente a través de métodos de comportamiento públicos expuestos por la raíz del agregado.

Objetos de Valor (Value Objects): Estructuras inmutables que no poseen identidad propia y se definen exclusivamente por el valor de sus propiedades (ej. Precios, Dimensiones, Códigos SKU).

Excepciones de Dominio: Fallos de negocio fuertemente tipados (ej. OutOfStockException, CartEmptyException) que impiden transiciones de estado inválidas.

3.2. Capa de Aplicación (.Application)
Orquesta la ejecución de los casos de uso del sistema.

Casos de Uso (Handlers / UseCases): Implementan la lógica procedimental de los flujos de negocio. Coordinan las entidades del dominio y consumen las abstracciones de infraestructura.

Abstracciones de Persistencia: Interfaces de repositorios (ej. ICartRepository) que delimitan los contratos de lectura y escritura sin detallar la tecnología subyacente.

Segregación de Comandos y Consultas (CQRS): Separación conceptual estricta entre operaciones que mutan el estado del sistema (Commands) y aquellas destinadas exclusivamente a la lectura u obtención de datos (Queries).

3.3. Capa de Infraestructura (.Infrastructure)
Provee el soporte tecnológico a los requerimientos de la capa de aplicación.

Contexto de Persistencia: Implementación específica del mapeo de datos objeto-relacional (ej. CartDbContext, OrderDbContext). Controla la configuración de las tablas y los índices específicos de la base de datos PostgreSQL.

Implementación de Repositorios: Clases que interactúan directamente con los adaptadores de datos para materializar y persistir el estado de los agregados.

Consumidores / Suscriptores: Componentes encargados de escuchar canales de mensajería (ej. OrderPlacedConsumer) para reaccionar a cambios de estado del ecosistema general.

4. Capa Host: TractorEcommerce.Api
Ubicada en src/Host/, esta aplicación web actúa como el orquestador global del monolito modular y expone las capacidades del negocio al exterior. Sus responsabilidades son estrictamente perimetrales:

Exposición de Endpoints REST: Enrutamiento HTTP de recursos mediante controladores estructurados y versionados (ej. /api/v1/cart, /api/v1/catalog).

Validación de Contratos: Intercepción previa de peticiones mediante esquemas de validación formales antes de invocar la lógica interna de los casos de uso.

Tratamiento Transversal de Errores: Middleware global de captura de excepciones (GlobalExceptionMiddleware.cs) que formatea las fallas técnicas o de negocio bajo el estándar RFC 7807 (Problem Details), garantizando respuestas semánticas limpias con identificadores de traza unificados (TraceId).

Configuración del Entorno: Inyección de configuraciones (appsettings.json), gestión de variables de entorno, mapeo de políticas de CORS para microfrontends (MFE) y auto-generación de documentación interactiva de la API (OpenAPI/Swagger).

5. Estrategia de Comunicación e Integración Inter-Módulos
Para mitigar el acoplamiento directo entre bases de datos y procesos en memoria, la sincronización de datos crítica e irreversible del sistema se gobierna mediante una Arquitectura Guiada por Eventos (EDA).

+--------------------+                       +---------------------+                       +-----------------------+
|  Módulo Cart       |                       |  Módulo Order       |                       |  Módulo Inventory     |
|                    |                       |                     |                       |                       |
|  Publica:          |==[ Apache Kafka ]====>|  Consume y Procesa: |==[ Apache Kafka ]====>|  Consume y Procesa:   |
|  CheckoutRequested |                       |  OrderPlacedEvent   |                       |  Descuenta Stock SKU  |
+--------------------+                       +---------------------+                       +-----------------------+
5.1. Mecanismo de Coreografía Asíncrona
Publicación: Cuando un flujo de negocio concluye de manera local (por ejemplo, la confirmación de una solicitud de compra en el módulo Cart), se emite un Evento de Integración inmutable hacia un clúster distribuido de Apache Kafka.

Consumo Desacoplado: Los módulos interesados de aguas abajo (Downstream Contexts), como Order o Inventory, interceptan de manera asíncrona dicho mensaje a través de Workers que corren en hilos de fondo independientes dentro del host.

Consistencia Eventual: Cada consumidor ejecuta la lógica secundaria dentro de su propia transacción local de base de datos. Si un módulo experimenta indisponibilidad o fallos, los mecanismos de reintento nativos del bus aíslan el error sin penalizar ni bloquear el hilo transaccional del flujo principal expuesto al cliente.

6. Stack Tecnológico de Referencia
Runtime: .NET 10.0 con C# moderno, explotando características avanzadas de inmutabilidad (records) y expresiones nativas de coincidencia de patrones (pattern matching).

Motor de Persistencia: PostgreSQL de grado de producción, configurado con contextos aislados para mitigar bloqueos transaccionales cruzados y estructurado para soportar tipos avanzados (UUIDs, JSONB).

Broker de Mensajería: Apache Kafka (Confluent.Kafka) para la coreografía tolerante a fallos y el manejo masivo de eventos de integración.

Observabilidad y Registro: Telemetría estructural en formato JSON plano a través de Serilog. Inyección automática de TraceId compartidos en las cabeceras de las peticiones para asegurar la trazabilidad completa del ciclo de vida de cada operación.

7. Estrategia de Calidad y Pruebas (Testing Strategy)
El sistema valida su integridad arquitectónica y de negocio mediante una pirámide de pruebas automatizada, cuyos resultados son auditados de forma mandatoria en el pipeline de CI/CD:

Pruebas Unitarias de Dominio: Enfocadas en verificar algoritmos puros y las invariantes de negocio (ej. cálculo de recomendaciones por distancia de color de SKUs, lógica de cálculo de totales del carrito) en un entorno completamente aislado del framework.

Pruebas de Integración y Capa de Datos: Verificación de repositorios, consultas complejas, paginación y el correcto comportamiento de los mapeos del ORM contra esquemas de prueba reales.

Reportes de Cobertura Dinámicos: Generación centralizada de informes detallados de cobertura (coverage-report-new), garantizando que los cambios en las capas críticas del core empresarial mantengan los umbrales de seguridad establecidos antes de proceder al despliegue.

Guía de Inicio (Getting Started)
Para desplegar el entorno local de desarrollo, asegúrese de contar con Docker Desktop y .NET 10.0 SDK instalados.

Clonación del repositorio:

Bash
git clone <url-del-repositorio>
cd TractorEcommerce
Levantamiento de infraestructura:

Bash
docker-compose up -d
Aplicación de migraciones:

Bash
dotnet ef database update --project src/Host/TractorEcommerce.Api
Nota: Asegúrese de configurar las variables de entorno necesarias en el archivo .env en la raíz del proyecto para las cadenas de conexión a PostgreSQL y los parámetros de conexión de Kafka.

9. API Reference (OpenAPI/Swagger)
El sistema genera automáticamente documentación interactiva mediante Swagger/OpenAPI.

Acceso: Una vez iniciada la aplicación, navegue a /swagger para explorar los endpoints.

Contrato de ejemplo: El proceso de creación de órdenes (POST /api/v1/orders) utiliza un formato JSON estándar que incluye el CartId y ShippingDetails. La respuesta retorna un 201 Created con el identificador único de la orden recién creada.

10. Observabilidad y Resiliencia
Observabilidad: La aplicación está diseñada como Observability Ready. Los logs se centralizan mediante Serilog en formato JSON, permitiendo una fácil ingesta en herramientas como Grafana Loki o el stack ELK. Se han implementado TraceIds globales para el rastreo de peticiones a través de los diferentes módulos.

Seguridad: El acceso a los recursos está protegido mediante [inserta tu método: e.g., JWT Bearer Authentication], garantizando que solo los usuarios autorizados puedan interactuar con los módulos de negocio.

Resiliencia: Se integra la librería Polly para la gestión de políticas de reintento (retries) y circuitos cerrados (circuit breakers) al interactuar con servicios externos y el broker de mensajería, asegurando que fallos temporales no degraden la experiencia del usuario.

Estrategia de Comunicación e Integración Inter-Módulos:

```
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
```