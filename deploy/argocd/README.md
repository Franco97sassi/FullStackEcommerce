# Entrega continua con Argo CD

La entrega sigue un flujo GitOps. CI prueba la aplicación, construye y escanea las
imágenes, las publica en GHCR y actualiza únicamente los repositorios y tags de
`deploy/helm/ecommerce/values-prod.yaml`. Argo CD no construye imágenes: observa
ese cambio en Git, renderiza el chart Helm y reconcilia el clúster.

## Preparación

1. Instalar Argo CD en el namespace `argocd` siguiendo la documentación oficial.
2. Sustituir `spec.source.repoURL` y `targetRevision` en
   `ecommerce-production.yaml`. Para producción es preferible mover `deploy/` a un
   repositorio GitOps separado y apuntar la Application a él.
3. Conceder acceso al repositorio a Argo CD si es privado.
4. Proveer `ecommerce-secrets` mediante External Secrets o Sealed Secrets antes de
   sincronizar. El chart deliberadamente no guarda credenciales en Git.
5. Aplicar la Application:

   ```bash
   kubectl apply -f deploy/argocd/ecommerce-production.yaml
   argocd app get ecommerce-production
   argocd app sync ecommerce-production
   ```

La sincronización automática tiene `selfHeal` y `prune`: Argo CD revierte cambios
manuales en el clúster y elimina recursos retirados de Git. El namespace de destino
se crea automáticamente, pero los operadores requeridos por Ingress, cert-manager
y ServiceMonitor deben existir con anterioridad.

## Flujo de publicación

`.github/workflows/deploy.yml` se ejecuta para tags `v*` o manualmente. La secuencia
es:

```text
tests -> build local -> Trivy scan -> push GHCR -> commit values-prod.yaml
                                                   |
                                                   v
Git change -> Argo CD -> Helm render -> Kubernetes sync
```

Los tags por defecto (`sha-<commit>`) son inmutables. Un fallo de tests, build o una
vulnerabilidad HIGH/CRITICAL impide publicar y, por tanto, Git nunca declara una
versión que no haya superado CI. La credencial `GITHUB_TOKEN` necesita permisos de
escritura para Packages y Contents; el workflow los declara expresamente.

Si se usa un repositorio GitOps separado, se debe reemplazar el último checkout y
push del workflow por un token o GitHub App con acceso mínimo a dicho repositorio.
