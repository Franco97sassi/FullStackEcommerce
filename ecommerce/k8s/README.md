# Despliegue en Kubernetes

Los manifiestos de `base/` despliegan frontend, backend y PostgreSQL en el namespace
`ecommerce`. Se gestionan con Kustomize e incluyen autoscaling, alta disponibilidad,
TLS, observabilidad y políticas de red.

## Requisitos

- Kubernetes 1.27+ y `kubectl`.
- Metrics Server para los HPA.
- Ingress NGINX y cert-manager para Ingress/TLS.
- Prometheus Operator para el recurso `ServiceMonitor`.
- Imágenes de frontend y backend publicadas en un registry accesible por el clúster.

## Secretos

Los valores sensibles no se guardan en Git. Copiar el ejemplo, completar valores
reales y crear el Secret directamente en el clúster:

```bash
cd ecommerce/k8s
cp secret.env.example secret.env
kubectl apply -f base/namespace.yaml
kubectl -n ecommerce create secret generic ecommerce-secrets \
  --from-env-file=secret.env \
  --dry-run=client -o yaml | kubectl apply -f -
```

En producción se recomienda sustituir este paso por External Secrets o Sealed
Secrets, conectado a un gestor de secretos administrado. `secret.env` y los
manifiestos `*-secret.yaml` están ignorados por Git.

## Imágenes y despliegue

Cambiar las dos imágenes de ejemplo en `base/backend.yaml` y `base/frontend.yaml`,
o aplicarlas sin editar archivos:

```bash
cd ecommerce/k8s
kustomize edit set image \
  ghcr.io/example/ecommerce-backend=registry.example.com/ecommerce/backend:1.0.0 \
  ghcr.io/example/ecommerce-frontend=registry.example.com/ecommerce/frontend:1.0.0
kubectl apply -k base
kubectl -n ecommerce rollout status deployment/backend
kubectl -n ecommerce rollout status deployment/frontend
```

Antes de producción también se deben reemplazar los hosts y el `ClusterIssuer` de
`base/ingress.yaml`, así como `NEXT_PUBLIC_API_URL`. Esta variable de Next.js puede
quedar incorporada durante el build si se usa desde código cliente; la imagen de
producción debe construirse con el mismo URL público configurado aquí.

Para un clúster local, construir imágenes con los nombres del overlay y aplicar:

```bash
docker build -t ecommerce-backend:local ecommerce/backend
docker build -t ecommerce-frontend:local -f ecommerce/frontend/Dockerfile.prod ecommerce/frontend
kubectl apply -k ecommerce/k8s/overlays/local
```

Añadir `tienda.local` y `api.tienda.local` a `/etc/hosts` apuntando a la IP del
Ingress Controller. El overlay local elimina TLS; no debe utilizarse en producción.

## Componentes

- PostgreSQL usa `StatefulSet`, Service headless y un PVC de 5 GiB.
- Frontend y backend usan Deployments, Services, ConfigMaps, ServiceAccounts sin
  token montado, contextos sin privilegios, filesystem raíz de solo lectura y
  requests/limits.
- Los HPA escalan el frontend por CPU y el backend por CPU/memoria. Metrics Server
  debe estar operativo y los requests de recursos no deben eliminarse.
- Los PDB mantienen al menos una réplica de frontend y backend durante disrupciones
  voluntarias.
- El backend expone `/health/live`, `/health/ready` y `/metrics`; el ServiceMonitor
  permite que Prometheus Operator descubra las métricas.
- Las NetworkPolicies parten de deny-by-default y sólo permiten Ingress, DNS y los
  flujos frontend → backend → PostgreSQL necesarios.

El escalado por requests por segundo requiere un adapter de métricas custom para
Prometheus. El consumer lag de Kafka puede añadirse con un `ScaledObject` de KEDA
cuando Kafka se habilite; no se activa en la base porque esta instalación local sólo
despliega los tres servicios existentes.

## Verificación y diagnóstico

```bash
kubectl kustomize ecommerce/k8s/base >/tmp/ecommerce-k8s.yaml
kubectl -n ecommerce get pods,svc,ingress,hpa,pdb
kubectl -n ecommerce describe hpa backend
kubectl -n ecommerce port-forward svc/backend 8080:8080
curl --fail http://localhost:8080/health/live
curl --fail http://localhost:8080/health/ready
curl --fail http://localhost:8080/metrics
```

PostgreSQL dentro de Kubernetes sirve para portfolio/desarrollo. Para producción,
usar preferentemente una base administrada u operador, backups probados, cifrado,
monitorización y una estrategia explícita de recuperación ante desastres.

Para instalaciones parametrizadas por entorno, usar alternativamente el chart de
`deploy/helm/ecommerce`; no se deben instalar la base Kustomize y el chart Helm con
los mismos nombres dentro de un mismo namespace.
