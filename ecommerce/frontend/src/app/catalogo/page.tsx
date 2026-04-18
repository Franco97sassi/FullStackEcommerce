import { CatalogPageClient } from "@/modules/catalogo/components/catalog-page-client";
import { Suspense } from "react";
export default function CatalogPage() {
 return (
    <Suspense fallback={<p className="px-6 py-10">Cargando catálogo...</p>}>
      <CatalogPageClient />
    </Suspense>
  );}
