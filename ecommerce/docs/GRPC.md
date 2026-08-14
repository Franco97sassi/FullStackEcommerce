# Comunicación gRPC interna

La API expone contratos versionados para catálogo, inventario, pedidos y pagos. El frontend continúa usando REST/Axios; gRPC es exclusivamente el límite síncrono entre el BFF y futuros servicios internos.

## Seguridad y resiliencia

Todos los métodos requieren `x-service-api-key` cuando `Grpc:Authentication:ApiKey` está configurada (es obligatorio configurarla fuera de Development/Testing). Los clientes aplican un deadline de cinco segundos y propagan el `CancellationToken` mediante las llamadas generadas. Solo Catalog y Order, operaciones de lectura seguras, reintentan `UNAVAILABLE`; Inventory y Payment nunca se reintentan automáticamente.

El interceptor traduce errores de contrato a códigos gRPC (`INVALID_ARGUMENT`, `NOT_FOUND`, `FAILED_PRECONDITION`) y oculta fallos inesperados. OpenTelemetry instrumenta servidor ASP.NET Core y clientes gRPC y exporta por OTLP. El servicio estándar de health checking gRPC se publica junto con `/healthz`.

## Desarrollo

Los contratos compartidos están en `Ecommerce.Grpc.Contracts/Protos` y se empaquetan como `Ecommerce.Grpc.Contracts`. Configure `Grpc__Authentication__ApiKey`, `Grpc__Services__Address` y `OTEL_EXPORTER_OTLP_ENDPOINT`. Use TLS y almacene la clave en el gestor de secretos, nunca en `appsettings.json`.

Los métodos mutables aceptan IDs idempotentes (`reservation_id` y `payment_id`). La reserva incluida facilita la extracción; un Inventory Service independiente debe persistir sus reservas y aplicar control de concurrencia en su propia base de datos.
