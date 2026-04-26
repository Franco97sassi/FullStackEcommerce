# EcommerceFullStack

Proyecto **e-commerce full stack** con arquitectura separada por servicios:

- **Frontend**: Next.js 16 + React 19 + TypeScript.
- **Backend**: ASP.NET Core Web API + Entity Framework Core + JWT.
- **Base de datos**: PostgreSQL 16.
- **Observabilidad**: endpoint `/metrics` estilo Prometheus + stack opcional Prometheus/Grafana por Docker Compose.

> Ideal para demo técnica a reclutadores/empresas porque cubre autenticación, catálogo, carrito, checkout, órdenes y panel admin.

---

## Tabla de contenidos

1. [Arquitectura](#arquitectura)
2. [Funcionalidades principales](#funcionalidades-principales)
3. [Stack tecnológico](#stack-tecnológico)
4. [Estructura del repositorio](#estructura-del-repositorio)
5. [Requisitos previos](#requisitos-previos)
6. [Configuración rápida (local)](#configuración-rápida-local)
7. [Ejecución con Docker Compose](#ejecución-con-docker-compose)
8. [Ejecución manual (sin Docker)](#ejecución-manual-sin-docker)
9. [Variables de entorno](#variables-de-entorno)
10. [API: endpoints principales](#api-endpoints-principales)
11. [Pruebas](#pruebas)
12. [Observabilidad y monitoreo](#observabilidad-y-monitoreo)
13. [Seguridad aplicada](#seguridad-aplicada)
14. [Checklist para demo a empresas](#checklist-para-demo-a-empresas)
15. [Roadmap sugerido](#roadmap-sugerido)
16. [Licencia](#licencia)

---

## Arquitectura

```text
[ Next.js Frontend ]  <----HTTP---->  [ ASP.NET Core API ]  <---->  [ PostgreSQL ]
        |                                      |
        └---------- JWT (Auth) ----------------┘

API expone /healthz y /metrics
/metrics puede ser scrapeado por Prometheus
Grafana se conecta a Prometheus para dashboards
```

Flujo típico de usuario:

1. Registro/Login (se emite JWT).
2. Consulta de catálogo/categorías/productos.
3. Alta y gestión de carrito.
4. Checkout y creación de orden.
5. Consulta de órdenes del usuario.

---

## Funcionalidades principales

- Registro y login con JWT.
- Catálogo de productos y categorías.
- Carrito de compras autenticado.
- Checkout con validaciones de stock y dirección.
- Historial y detalle de órdenes por usuario.
- Endpoints de salud y métricas para operación.
- Middleware global de manejo de excepciones, logging, headers de seguridad y correlation id.
- Rate limiting global con límites más estrictos para auth y checkout.

---

## Stack tecnológico

### Frontend

- Next.js `16.2.3`
- React `19.2.4`
- TypeScript
- TanStack Query
- Axios
- Tailwind CSS 4

### Backend

- ASP.NET Core Web API (.NET)
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- BCrypt para hash de contraseñas
- Swagger (en entorno Development)

### Infra

- Docker / Docker Compose
- Prometheus
- Grafana

---

## Estructura del repositorio

```text
.
├─ readme.md
├─ docker-compose.observability.yml
├─ ecommerce/
│  ├─ docker-compose.yml
│  ├─ docker-compose-prod.yml
│  ├─ env.example
│  ├─ backend/
│  │  ├─ Ecommerce.API/
│  │  ├─ Ecommerce.Application/
│  │  ├─ Ecommerce.Domain/
│  │  ├─ Ecommerce.Infrastructure/
│  │  └─ Ecommerce.Tests/
│  ├─ frontend/
│  │  └─ src/
│  ├─ monitoring/
│  │  ├─ prometheus.yml
│  │  └─ alerts.yml
│  └─ docs/
│     ├─ runbooks/
│     └─ security/
```

---

## Requisitos previos

- Docker + Docker Compose
- (Opcional para correr sin Docker) Node.js 20+ y .NET SDK compatible con el backend.
- Puerto disponibles: `3000` (frontend), `8080` (backend), `5433` (postgres local mapeado), `9090` (prometheus), `3001` (grafana).

---

## Configuración rápida (local)

1. Ir al directorio del proyecto principal:

   ```bash
   cd ecommerce
   ```

2. Crear archivo de entorno:

   ```bash
   cp env.example .env
   ```

3. Levantar servicios:

   ```bash
   docker compose up --build
   ```

4. Abrir:

- Frontend: http://localhost:3000
- Backend: http://localhost:8080
- Healthcheck API: http://localhost:8080/healthz
- Swagger (si `ASPNETCORE_ENVIRONMENT=Development`): http://localhost:8080/swagger

---

## Ejecución con Docker Compose

### Desarrollo

Desde `ecommerce/`:

```bash
docker compose up --build
```

Servicios levantados:

- `postgres` (con volumen persistente)
- `backend` (migraciones automáticas al iniciar)
- `frontend`

### Producción (base)

Desde `ecommerce/`:

```bash
docker compose -f docker-compose-prod.yml up --build -d
```

> Antes de producción, cambia credenciales/secretos y no uses valores por defecto.

---

## Ejecución manual (sin Docker)

> Recomendado solo para desarrollo local.

### 1) Base de datos

Levanta PostgreSQL local y crea una DB con valores equivalentes a `.env`.

### 2) Backend

Desde `ecommerce/backend/Ecommerce.API`:

```bash
dotnet restore
dotnet run
```

### 3) Frontend

Desde `ecommerce/frontend`:

```bash
npm install
npm run dev
```

---

## Variables de entorno

Basado en `ecommerce/env.example`:

```env
# PostgreSQL
POSTGRES_DB=ecommerce_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5433

# Backend
ASPNETCORE_ENVIRONMENT=Development
BACKEND_PORT=8080
JWT_KEY=supersecretkey_dev_only_change_me
JWT_ISSUER=Ecommerce.API
JWT_AUDIENCE=Ecommerce.Web

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:8080
```

### Recomendaciones

- En producción usa una `JWT_KEY` robusta (>= 32 chars).
- Nunca subas secretos reales al repositorio.
- Define CORS estricto para tus dominios reales.

---

## API: endpoints principales

Base URL local: `http://localhost:8080`

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

### Catálogo

- `GET /api/catalog/categories`
- `GET /api/catalog/products`
- `GET /api/catalog/products/{slug}`

### Carrito (requiere JWT)

- `GET /api/cart`
- `POST /api/cart/items`
- `PATCH /api/cart/items/{productId}`
- `DELETE /api/cart/items/{productId}`
- `DELETE /api/cart`

### Checkout / Órdenes (requiere JWT)

- `POST /api/checkout/orders`
- `GET /api/checkout/orders/me`
- `GET /api/checkout/orders/{id}`

### Admin

- Endpoints bajo `/api/admin/...` (requieren rol admin)

### Operación

- `GET /healthz`
- `GET /metrics`

---

## Pruebas

### Frontend

Desde `ecommerce/frontend`:

```bash
npm run test
```

### Backend

Desde `ecommerce/backend`:

```bash
dotnet test
```

---

## Observabilidad y monitoreo

Este repo incluye compose dedicado para Prometheus + Grafana.

Desde la raíz del repo:

```bash
docker compose -f docker-compose.observability.yml up -d
```

Accesos:

- Prometheus: http://localhost:9090
- Grafana: http://localhost:3001 (default: `admin` / `admin`)

---

## Seguridad aplicada

En el backend se observa:

- Autenticación JWT Bearer.
- Validación de issuer/audience/signature/lifetime.
- Password hashing con BCrypt.
- Rate limiting por IP/ruta.
- Middleware de headers de seguridad.
- Middleware global de excepción con respuesta consistente.
- Correlation ID para trazabilidad de requests.

---

## Checklist para demo a empresas

Antes de compartir tu proyecto:

- [ ] `README` actualizado y claro.
- [ ] Demo deployada (frontend + backend).
- [ ] Datos de prueba listos (usuario normal + admin).
- [ ] Capturas o video corto del flujo completo.
- [ ] Historias técnicas preparadas: decisiones, trade-offs y mejoras futuras.

Guion de demo recomendado (5-8 min):

1. Problema y objetivo (30s)
2. Arquitectura y stack (60s)
3. Flujo usuario end-to-end (3-4 min)
4. Seguridad/observabilidad (60s)
5. Próximos pasos (30s)

---

## Roadmap sugerido

- Integración de pasarela de pago real (Stripe/Mercado Pago).
- CI/CD con validaciones automáticas.
- Cobertura de tests (unit + integration + e2e).
- Gestión de inventario y cancelaciones.
- Roles/permisos más granulares en admin.
- Multi-idioma y mejoras de accesibilidad.

---

## Licencia

Define aquí la licencia de tu proyecto (por ejemplo MIT) y agrega el archivo `LICENSE`.

Si quieres, te puedo dejar también una versión **en inglés** orientada a recruiters internacionales.