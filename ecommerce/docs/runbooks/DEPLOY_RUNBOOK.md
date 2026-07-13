# Deploy runbook

Guía operativa para preparar, validar y desplegar el proyecto ecommerce full-stack.

## Requisitos

- Docker y Docker Compose instalados.
- Acceso al repositorio actualizado.
- Variables de entorno configuradas.
- Puertos disponibles según el entorno: frontend, backend y PostgreSQL.

## Validación previa

Desde `ecommerce/frontend`:

```bash
npm install
npm run lint
npm run test:unit
npm run build
```

Desde `ecommerce/backend`:

```bash
dotnet restore
dotnet test
```

## Despliegue local con Docker Compose

Desde `ecommerce`:

```bash
cp env.example .env
docker compose up --build
```

Validar:

- Frontend: `http://localhost:3000`
- Backend: `http://localhost:8080`
- Health check: `http://localhost:8080/healthz`
- Métricas: `http://localhost:8080/metrics`

## Despliegue base de producción

1. Crear un `.env` seguro con credenciales reales.
2. Configurar `ASPNETCORE_ENVIRONMENT=Production`.
3. Usar una `JWT_KEY` fuerte de al menos 32 caracteres.
4. Configurar `NEXT_PUBLIC_API_URL` apuntando al backend publicado.
5. Levantar servicios:

```bash
docker compose -f docker-compose-prod.yml up --build -d
```

## Rollback

1. Identificar la versión anterior estable.
2. Restaurar variables de entorno si fueron modificadas.
3. Reconstruir imágenes con la versión anterior.
4. Ejecutar health check y flujo crítico de compra.

## Smoke test post-deploy

- [ ] `/healthz` responde correctamente.
- [ ] El catálogo carga productos.
- [ ] Un usuario puede registrarse o iniciar sesión.
- [ ] El carrito permite agregar productos.
- [ ] El checkout crea una orden.
- [ ] `/metrics` expone métricas para Prometheus.