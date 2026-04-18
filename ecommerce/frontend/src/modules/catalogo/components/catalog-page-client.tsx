"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { FormEvent } from "react";
import { getCategories, getProducts } from "../services/catalog-service";
import { apiClient } from "@/lib/api/api-client";
import { AddToCartButton } from "@/modules/carrito/components/add-to-cart-button";
const PAGE_SIZE = 6;

export function CatalogPageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const search = searchParams.get("q") ?? "";
  const categorySlug = searchParams.get("categoria") ?? "";
  const minPrice = searchParams.get("min") ? Number(searchParams.get("min")) : undefined;
  const maxPrice = searchParams.get("max") ? Number(searchParams.get("max")) : undefined;
  const page = Number(searchParams.get("page") ?? "1");

  const categoriesQuery = useQuery({
    queryKey: ["catalog-categories"],
    queryFn: getCategories,
  });


  const backendStatusQuery = useQuery({
    queryKey: ["backend-health"],
    queryFn: async () => {
      await apiClient.get("/api/health");
      return true;
    },
    retry: false,
  });

  const productsQuery = useQuery({
    queryKey: ["catalog-products", search, categorySlug, minPrice, maxPrice, page],
    queryFn: () =>
      getProducts({
        search,
        categorySlug,
        minPrice,
        maxPrice,
        page,
        pageSize: PAGE_SIZE,
      }),
  });

  function updateParam(next: Record<string, string | undefined>) {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(next).forEach(([key, value]) => {
      if (!value) {
        params.delete(key);
      } else {
        params.set(key, value);
      }
    });

    router.push(`/catalogo?${params.toString()}`);
  }

  function onSubmitSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const query = String(formData.get("q") ?? "").trim();

    updateParam({
      q: query || undefined,
      page: "1",
    });
  }

  const result = productsQuery.data;

  return (
    <main className="mx-auto min-h-screen w-full max-w-6xl space-y-6 px-6 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl font-bold">Catálogo</h1>
        <p className="text-sm text-gray-600">Explora productos por categoría, filtros y búsqueda.</p>
        <p className={`text-xs font-medium ${backendStatusQuery.data ? "text-emerald-600" : "text-amber-600"}`}>
          {backendStatusQuery.data
            ? "Backend conectado (API real)."
            : "Sin conexión al backend: mostrando datos mock."}
        </p>
      </header>

      <section className="grid gap-4 rounded-xl border p-4 md:grid-cols-4">
        <form className="md:col-span-2" onSubmit={onSubmitSearch}>
          <label className="mb-1 block text-sm font-medium">Búsqueda</label>
          <div className="flex gap-2">
            <input
              name="q"
              defaultValue={search}
              placeholder="Buscar por nombre o descripción"
              className="w-full rounded-md border px-3 py-2"
            />
            <button type="submit" className="rounded-md bg-black px-4 py-2 text-white">
              Buscar
            </button>
          </div>
        </form>

        <div>
          <label className="mb-1 block text-sm font-medium">Categoría</label>
          <select
            value={categorySlug}
            onChange={(event) => updateParam({ categoria: event.target.value || undefined, page: "1" })}
            className="w-full rounded-md border px-3 py-2"
          >
            <option value="">Todas</option>
            {categoriesQuery.data?.map((category) => (
              <option key={category.id} value={category.slug}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">Rango de precio</label>
          <div className="flex gap-2">
            <input
              type="number"
              placeholder="Mín"
              defaultValue={minPrice}
              className="w-full rounded-md border px-2 py-2"
              onBlur={(event) => updateParam({ min: event.target.value || undefined, page: "1" })}
            />
            <input
              type="number"
              placeholder="Máx"
              defaultValue={maxPrice}
              className="w-full rounded-md border px-2 py-2"
              onBlur={(event) => updateParam({ max: event.target.value || undefined, page: "1" })}
            />
          </div>
        </div>
      </section>

      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <p className="text-sm text-gray-600">{result?.total ?? 0} producto(s) encontrado(s)</p>
          <button
            type="button"
            onClick={() => router.push("/catalogo")}
            className="text-sm text-blue-600 underline"
          >
            Limpiar filtros
          </button>
        </div>

        {productsQuery.isLoading ? (
          <p>Cargando productos...</p>
        ) : result && result.products.length > 0 ? (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {result.products.map((product) => {
              return (
                <article key={product.id} className="overflow-hidden rounded-xl border">
                  <img src={product.image ?? "https://placehold.co/600x400?text=Producto"} alt={product.name} className="h-44 w-full object-cover" />
                  <div className="space-y-2 p-4">
                    <span className="inline-block rounded-full bg-gray-100 px-2 py-1 text-xs">
                      {product.categoryName}
                    </span>
                    <h2 className="line-clamp-2 text-lg font-semibold">{product.name}</h2>
                    <p className="line-clamp-2 text-sm text-gray-600">{product.description ?? "Sin descripción disponible."}</p>
                    <p className="text-xl font-bold">${product.price.toFixed(2)}</p>
                    <div className="flex items-center gap-2">
                      <Link
                        href={`/catalogo/${product.slug}`}
                        className="inline-block rounded-md border px-4 py-2 text-sm"
                      >
                        Ver detalle
                      </Link>
                      <AddToCartButton
                        product={{
                          id: product.id,
                          slug: product.slug,
                          name: product.name,
                          price: product.price,
                          stock: product.stock,
                        }}
                      />
                    </div>
                  </div>
                </article>
              );
            })}
          </div>
        ) : (
          <div className="rounded-xl border border-dashed p-8 text-center text-sm text-gray-600">
            No hay productos para los filtros seleccionados.
          </div>
        )}

        {result && result.totalPages > 1 && (
          <nav className="flex items-center justify-center gap-2">
            <button
              type="button"
              disabled={result.currentPage <= 1}
              onClick={() => updateParam({ page: String(result.currentPage - 1) })}
              className="rounded-md border px-3 py-1 disabled:opacity-40"
            >
              Anterior
            </button>

            <span className="text-sm">
              Página {result.currentPage} de {result.totalPages}
            </span>

            <button
              type="button"
              disabled={result.currentPage >= result.totalPages}
              onClick={() => updateParam({ page: String(result.currentPage + 1) })}
              className="rounded-md border px-3 py-1 disabled:opacity-40"
            >
              Siguiente
            </button>
          </nav>
        )}
      </section>
    </main>
  );
}
