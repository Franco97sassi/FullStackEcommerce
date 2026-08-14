# Ecommerce Helm chart

Chart único para desplegar el frontend Next.js y la API ASP.NET Core. PostgreSQL,
Redis, Kafka, Elasticsearch y OpenTelemetry se configuran como endpoints externos;
para producción se recomienda administrarlos mediante servicios gestionados u
operadores independientes.

## Validación e instalación

```bash
helm lint deploy/helm/ecommerce
helm template ecommerce deploy/helm/ecommerce \
  --namespace ecommerce \
  --values deploy/helm/ecommerce/values-dev.yaml
```

El chart no crea secretos de forma predeterminada. Crear el Secret referenciado por
`secrets.existingSecret` antes de instalar:

```bash
kubectl create namespace ecommerce
kubectl -n ecommerce create secret generic ecommerce-secrets \
  --from-literal=ConnectionStrings__DefaultConnection='Host=postgres;Port=5432;Database=ecommerce;Username=ecommerce;Password=CHANGE_ME' \
  --from-literal=Jwt__Key='CHANGE_ME_WITH_AT_LEAST_32_RANDOM_CHARACTERS'
helm upgrade --install ecommerce deploy/helm/ecommerce \
  --namespace ecommerce \
  --values deploy/helm/ecommerce/values-dev.yaml
```

En staging/producción se debe proporcionar ese Secret mediante External Secrets o
Sealed Secrets. Como alternativa sólo para desarrollo, `secrets.create=true` permite
que Helm lo cree; los valores sensibles deben pasarse por un archivo local ignorado
o mediante `--set-file`, nunca en un values versionado.

## Entornos

- `values-dev.yaml`: una réplica, sin HPA/PDB/TLS ni ServiceMonitor.
- `values-staging.yaml`: dominios e imágenes de staging.
- `values-prod.yaml`: al menos tres réplicas y límites de escalado mayores.

Los archivos de entorno se combinan con `values.yaml`; sólo contienen overrides. Se
pueden reemplazar repositorio y tag sin modificar archivos:

```bash
helm upgrade --install ecommerce deploy/helm/ecommerce \
  --namespace ecommerce --create-namespace \
  -f deploy/helm/ecommerce/values-prod.yaml \
  --set backend.image.repository=registry.example.com/ecommerce-backend \
  --set backend.image.tag=1.2.3 \
  --set frontend.image.repository=registry.example.com/ecommerce-frontend \
  --set frontend.image.tag=1.2.3
```

`NEXT_PUBLIC_API_URL` normalmente se incorpora en el bundle del navegador durante
`next build`. La imagen del frontend debe construirse con la misma URL indicada en
`frontend.config.apiUrl`; el ConfigMap también la expone para código ejecutado en el
servidor.

## Dependencias opcionales

HPA necesita Metrics Server, Ingress TLS necesita Ingress NGINX y cert-manager, y
`serviceMonitor.enabled=true` necesita los CRD de Prometheus Operator. Deshabilitar
el recurso opcional cuando el clúster no tenga su operador:

```bash
helm upgrade --install ecommerce deploy/helm/ecommerce \
  --set serviceMonitor.enabled=false
```

Si el sistema crece, este chart puede actuar como chart de plataforma y declarar
como dependencias charts separados (`ecommerce-backend`, `ecommerce-frontend` y
`ecommerce-workers`) sin cambiar la interfaz principal de values.
