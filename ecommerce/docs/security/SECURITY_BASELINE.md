# Security baseline

Este documento resume las medidas mínimas de seguridad aplicadas y los puntos que deben revisarse antes de
publicar una demo o desplegar el proyecto en un entorno compartido.

## Medidas implementadas

- Autenticación con JWT Bearer para endpoints protegidos.
- Hash de contraseñas con BCrypt antes de persistir usuarios.
- Validación de issuer, audience, firma y expiración del token.
- Rechazo del arranque en entornos no Development cuando la clave JWT tiene menos de 32 caracteres.
- Rate limiting global con límites más estrictos para rutas de autenticación y checkout.
- Middleware de headers de seguridad, logging de requests, correlation id, métricas y manejo global de excepciones.
- Endpoints de operación separados: `/healthz` para salud y `/metrics` para scraping estilo Prometheus.

## Requisitos antes de producción

1. Cambiar todos los secretos por valores fuertes y únicos.
2. No versionar archivos `.env` reales ni credenciales.
3. Usar HTTPS frente a usuarios finales.
4. Configurar `NEXT_PUBLIC_API_URL` con el dominio público correcto.
5. Restringir CORS al dominio real del frontend.
6. Revisar logs para evitar datos sensibles.
7. Ejecutar migraciones y seed de datos en un entorno controlado.
8. Ejecutar pruebas automáticas antes de cada despliegue.

## Variables sensibles

- `POSTGRES_PASSWORD`
- `JWT_KEY`
- Credenciales del proveedor de hosting o contenedores.
- Cualquier token de servicios externos usado por integraciones futuras.

## Checklist de revisión

- [ ] `JWT_KEY` tiene al menos 32 caracteres y no es el valor de ejemplo.
- [ ] `.env` local no está versionado.
- [ ] CORS no permite orígenes innecesarios.
- [ ] Swagger está expuesto solo en Development.
- [ ] El frontend compila con `npm run build`.
- [ ] El backend pasa `dotnet test`.