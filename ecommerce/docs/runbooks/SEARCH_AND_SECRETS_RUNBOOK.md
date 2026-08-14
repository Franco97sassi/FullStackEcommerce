# Elasticsearch y Vault

## Proyección de búsqueda

PostgreSQL continúa siendo la fuente de verdad del catálogo. Cada alta, cambio de
datos/stock o borrado de producto guarda un `ProductChangedEvent` en el outbox en
la misma transacción. El publicador entrega el evento a Kafka y
`ProductSearchIndexer` actualiza `ecommerce-products-v1` en Elasticsearch.

El endpoint de catálogo usa Elasticsearch para texto, filtros, ranking y
tolerancia a errores. Si Elasticsearch está desactivado o no responde, vuelve a
consultar PostgreSQL. `GET /api/catalog/products/suggest?search=...` ofrece el
autocompletado con el mismo fallback. Esta degradación evita convertir el índice
en una dependencia de escritura o en la fuente primaria.

Para reconstruir el índice, elimínalo y reproduce el topic desde un consumer
group nuevo, o vuelve a emitir eventos para el catálogo desde PostgreSQL. Los
consumidores confirman el offset sólo después de indexar, por lo que la entrega
es al menos una vez y la escritura por id de producto es idempotente.

## Secretos

Compose levanta un Vault de desarrollo, carga el KV v2 `secret/ecommerce` y la
API incorpora esos valores al final de su configuración. Las claves usan la
convención de .NET (`Jwt__Key` se transforma en `Jwt:Key`). Comprueba el secreto:

```bash
docker compose exec vault vault kv get secret/ecommerce
```

El token raíz y los valores de `env.example` son sólo para desarrollo local. En
producción no se debe ejecutar Vault en modo dev ni guardar un token estático:
inyecta un token efímero mediante AppRole/Kubernetes Auth, limita la policy a
`read` sobre `secret/data/ecommerce`, habilita audit logs y rota JWT y credenciales
de base de datos. GitHub Actions debe continuar usando GitHub Secrets/OIDC para
el despliegue y nunca copiar secretos de producción al repositorio.
