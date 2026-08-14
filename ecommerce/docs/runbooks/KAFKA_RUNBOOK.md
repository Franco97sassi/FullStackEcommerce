# Kafka y transactional outbox

La API publica el evento `OrderCreatedEvent` en el topic
`ecommerce.orders.created.v1`. La orden y el mensaje pendiente se guardan en la
misma transacción de PostgreSQL; un `BackgroundService` publica después el
mensaje en Kafka. Este patrón evita perder el evento si Kafka no está disponible
durante el checkout.

## Inicio local

```bash
cd ecommerce
cp env.example .env
docker compose up --build
```

Compose inicia un broker Kafka en modo KRaft, crea el topic y configura la API
con `kafka:9092`. Desde el host, el broker está disponible en
`localhost:29092`.

Para ejecutar la API fuera de Docker, establece `Kafka:Enabled=true` y conserva
`Kafka:BootstrapServers=localhost:29092`.

## Inspección

Listar topics:

```bash
docker compose exec kafka /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 --list
```

Consumir eventos desde el principio:

```bash
docker compose exec kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic ecommerce.orders.created.v1 \
  --from-beginning \
  --property print.key=true \
  --property print.headers=true
```

## Garantías y operación

- El productor solicita confirmación de todas las réplicas y tiene idempotencia
  habilitada.
- La entrega del outbox es **al menos una vez**. Un fallo entre la confirmación
  de Kafka y la actualización de PostgreSQL puede producir un duplicado.
- Los consumidores deben usar el header `event-id` como clave de idempotencia.
- Los fallos se guardan en `outbox_messages` y se reintentan con backoff
  exponencial hasta el límite configurado.
- `ProcessedAtUtc IS NULL` identifica mensajes pendientes. Conviene alertar por
  su antigüedad y limpiar periódicamente mensajes procesados antiguos.

## Configuración

| Clave | Valor local | Uso |
| --- | --- | --- |
| `Kafka:Enabled` | `false` fuera de Compose | Activa el publicador. |
| `Kafka:BootstrapServers` | `localhost:29092` | Brokers separados por coma. |
| `Kafka:ClientId` | `ecommerce-api` | Identidad del productor. |
| `Kafka:OrderCreatedTopic` | `ecommerce.orders.created.v1` | Topic versionado. |
| `Kafka:OutboxBatchSize` | `50` | Mensajes procesados por ciclo. |
| `Kafka:OutboxPollingIntervalSeconds` | `5` | Intervalo de consulta. |
| `Kafka:OutboxMaxRetryDelaySeconds` | `300` | Máximo backoff. |

En producción, crea el topic antes del despliegue, configura replicación acorde
al clúster y proporciona credenciales/TLS mediante secretos; no guardes secretos
en `appsettings.json`.
