# EcommerceFullStack

Proyecto **e-commerce full stack** pensado para portfolio técnico: una tienda online con frontend moderno, API REST, base de datos relacional, autenticación, carrito, checkout, panel admin, Docker y observabilidad.

> Objetivo: demostrar capacidad para construir, documentar y operar una aplicación web end-to-end con buenas prácticas de arquitectura, seguridad y despliegue.

---

## Demo para recruiters

> Completar antes de compartir el portfolio públicamente.

- **Frontend:** `pendiente de deploy`
- **Backend/API:** `pendiente de deploy`
- **Swagger:** disponible en local en `http://localhost:8080/swagger` cuando el backend corre en entorno `Development`.
- **Usuario demo:** `pendiente`
- **Usuario admin demo:** `pendiente`
- **Video/capturas:** `pendiente`

### Guion sugerido de presentación (5-8 minutos)

1. **Problema y objetivo:** e-commerce completo para simular un flujo real de compra.
2. **Arquitectura:** Next.js consume una API ASP.NET Core; PostgreSQL persiste usuarios, productos, carritos y órdenes.
3. **Flujo usuario:** registro/login, catálogo, detalle de producto, carrito, checkout y consulta de órdenes.
4. **Panel admin:** gestión de productos, categorías, stock, usuarios y órdenes.
5. **Calidad técnica:** JWT, rate limiting, security headers, healthchecks, métricas y Docker Compose.
6. **Roadmap:** pagos, emails, imágenes cloud, cache y jobs en background.

---

## Características principales

### Usuario final

- Registro e inicio de sesión con JWT.
- Catálogo de productos y categorías.
- Búsqueda, filtros y paginación en catálogo.
- Detalle de producto.
- Carrito autenticado.
- Checkout con dirección de envío y validación de stock.
- Historial y detalle de órdenes.

### Administración

- Panel admin protegido por rol.
- Gestión de productos y categorías.
- Actualización de stock.
- Gestión de usuarios y roles.
- Gestión de estado de órdenes.
- Generación asistida de copy de producto mediante endpoint de IA.

### Operación, seguridad y observabilidad

- Healthcheck en `/healthz`.
- Métricas estilo Prometheus en `/metrics`.
- Middleware global de excepciones.
- Logging estructurado.
- Correlation ID por request.
- Headers de seguridad.
- Rate limiting global, con límites más estrictos para auth y checkout.
- Docker Compose para levantar frontend, backend y PostgreSQL.
- Stack opcional Prometheus + Grafana.

---

## Stack tecnológico

### Frontend

- **Next.js 16**
- **React 19**
- **TypeScript**
- **TanStack Query**
- **Axios**
- **Tailwind CSS 4**

### Backend

- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL 16**
- **JWT Bearer Authentication**
- **BCrypt** para hash de contraseñas
- **Swagger/OpenAPI** en desarrollo

### Infraestructura y calidad

- **Docker / Docker Compose**
- **Prometheus**
- **Grafana**
- **GitHub Actions CI**
- Tests frontend y backend

---

## Arquitectura

```text
[ Browser ]
    |
    v
[ Next.js Frontend ]  <---- REST/HTTP ---->  [ ASP.NET Core API ]  <---- EF Core ---->  [ PostgreSQL ]
        |                                            |
        |                                            +--> /healthz
        |                                            +--> /metrics
        |
        +--> JWT almacenado del lado cliente para consumir endpoints protegidos

[ Prometheus ] ---- scrape ----> /metrics
[ Grafana ] ---- dashboards ---> Prometheus
```

### Flujo principal

1. El usuario se registra o inicia sesión.
2. El backend emite un JWT.
3. El frontend usa el token para consumir carrito, checkout y órdenes.
4. El usuario navega el catálogo y agrega productos al carrito.
5. El checkout valida stock, crea la orden y descuenta inventario.
6. El usuario puede consultar sus órdenes.
7. El admin puede gestionar productos, categorías, stock, usuarios y órdenes.

---

## Estructura del repositorio

```text
.
├── readme.md
├── docker-compose.observability.yml
├── env.prod.example
└── ecommerce/
    ├── docker-compose.yml
    ├── docker-compose-prod.yml
    ├── env.example
    ├── backend/
    │   ├── Ecommerce.API/
    │   ├── Ecommerce.Application/
    │   ├── Ecommerce.Domain/
    │   ├── Ecommerce.Infrastructure/
    │   └── Ecommerce.Tests/
    ├── frontend/
    │   ├── src/
    │   ├── tests/
    │   └── package.json
    ├── monitoring/
    │   ├── prometheus.yml
    │   └── alerts.yml
    └── docs/
        ├── runbooks/
        └── security/
```

---

## Requisitos previos

Para correr con Docker:

- Docker
- Docker Compose

Para correr manualmente:

- Node.js 20+
- npm
- .NET SDK 8+
- PostgreSQL 16+

Puertos usados por defecto:

- Frontend: `3000`
- Backend: `8080`
- PostgreSQL local mapeado: `5433`
- Prometheus: `9090`
- Grafana: `3001`

---

## Inicio rápido con Docker

Desde la raíz del repositorio:

```bash
cd ecommerce
cp env.example .env
docker compose up --build
```

Luego abrir:

- Frontend: http://localhost:3000
- Backend: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Healthcheck: http://localhost:8080/healthz
- Métricas: http://localhost:8080/metrics

---

## Ejecución manual

### Backend

```bash
cd ecommerce/backend/Ecommerce.API
dotnet restore
dotnet run
```

### Frontend

```bash
cd ecommerce/frontend
npm install
npm run dev
```

---

## Variables de entorno

Archivo base: `ecommerce/env.example`.

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

> Para producción: reemplazar credenciales por secretos reales, usar una `JWT_KEY` fuerte y configurar CORS con el dominio definitivo del frontend.

---

## Endpoints principales

Base URL local: `http://localhost:8080`.

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

### Catálogo

- `GET /api/catalog/categories`
- `GET /api/catalog/products`
- `GET /api/catalog/products/{slug}`

### Carrito

Requiere JWT.

- `GET /api/cart`
- `POST /api/cart/items`
- `PATCH /api/cart/items/{productId}`
- `DELETE /api/cart/items/{productId}`
- `DELETE /api/cart`

### Checkout / órdenes

Requiere JWT.

- `POST /api/checkout/orders`
- `GET /api/checkout/orders/me`
- `GET /api/checkout/orders/{id}`

### Admin

Requiere rol admin.

- Endpoints bajo `/api/admin/...`

### Operación

- `GET /healthz`
- `GET /metrics`

---

## Pruebas y validaciones

### Frontend

```bash
cd ecommerce/frontend
npm run test
npm run lint
npm run build
```

### Backend

```bash
cd ecommerce/backend
dotnet test Ecommerce.slnx
```

### CI

El repositorio incluye workflow de CI con validaciones separadas para backend y frontend:

- Backend: restore, build y test.
- Frontend: install, lint y build.

---

## Observabilidad

Levantar Prometheus y Grafana desde la raíz del repositorio:

```bash
docker compose -f docker-compose.observability.yml up -d
```

Accesos por defecto:

- Prometheus: http://localhost:9090
- Grafana: http://localhost:3001

---

## Seguridad aplicada

- JWT Bearer Authentication.
- Validación de issuer, audience, firma y expiración del token.
- BCrypt para contraseñas.
- Rate limiting por IP/ruta.
- Headers de seguridad.
- Manejo global de excepciones.
- Correlation ID para trazabilidad.
- Healthchecks para servicios Docker.

---

## Estado para portfolio

### Listo para mostrar

- Arquitectura full stack separada.
- Flujo e-commerce end-to-end.
- Panel admin.
- Seguridad básica aplicada.
- Docker Compose.
- Observabilidad con métricas.
- CI configurado.
- Documentación técnica inicial.

### Pendiente antes de compartir con recruiters

- Deploy público de frontend y backend.
- Usuarios demo normal/admin.
- Capturas o video corto.
- Limpieza final del repositorio.
- Confirmar que `npm run test`, `npm run lint`, `npm run build` y `dotnet test` pasan en CI.
- Agregar licencia.

---

## Roadmap técnico

Estas mejoras no son obligatorias para presentarlo en portfolio, pero subirían el nivel del proyecto:

1. **Stripe o Mercado Pago en modo test**
   - Crear intención/preferencia de pago.
   - Confirmar orden solo luego del pago aprobado.
   - Guardar estado de pago y referencia externa.

2. **Emails transaccionales**
   - Confirmación de registro.
   - Confirmación de orden.
   - Cambio de estado de pedido.

3. **Cloudinary, S3 o Azure Blob Storage para imágenes**
   - Subida de imágenes desde el panel admin.
   - URLs optimizadas para productos.
   - Eliminación o reemplazo de imágenes antiguas.

4. **Redis para cache**
   - Cache de catálogo y categorías.
   - Cache de productos destacados.
   - Base para rate limiting distribuido si se escala horizontalmente.

5. **Background jobs con Hangfire**
   - Envío de emails fuera del request principal.
   - Limpieza de carritos abandonados.
   - Reintentos de tareas fallidas.

6. **Playwright para pruebas end-to-end**
   - Login.
   - Catálogo.
   - Carrito.
   - Checkout.
   - Panel admin.

---

## Cómo hablar de este proyecto en una entrevista

> “Construí un e-commerce full stack con Next.js, ASP.NET Core y PostgreSQL. Implementé autenticación JWT, catálogo, carrito, checkout, órdenes y panel admin. También agregué Docker Compose, healthchecks, métricas tipo Prometheus, rate limiting, security headers y CI. El objetivo fue practicar una arquitectura realista de producto, no solo un CRUD.”

Puntos técnicos para destacar:

- Separación de responsabilidades entre frontend, API, dominio, infraestructura y tests.
- Uso de JWT y roles para proteger funcionalidades.
- Persistencia relacional con Entity Framework Core.
- Middleware para seguridad, trazabilidad y errores.
- Observabilidad básica con `/healthz` y `/metrics`.
- Docker Compose para facilitar onboarding y demo local.

---

## Licencia

Pendiente de definir. Recomendación para portfolio: agregar una licencia `MIT` si quieres que el código pueda ser reutilizado con atribución.