"use client";

import Image from "next/image";
import Link from "next/link";
import { FormEvent } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/api-client";
import { AddToCartButton } from "@/modules/catalogo/components/add-to-cart-button";
import { getCategories, getProducts } from "../services/catalog-service";

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

  const result = productsQuery.data;

  function updateParam(next: Record<string, string | undefined>) {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(next).forEach(([key, value]) => {
      if (!value) {
        params.delete(key);
        return;
      }

      params.set(key, value);
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

  return (
    <main className="mx-auto min-h-screen w-full max-w-7xl space-y-8 px-6 py-10">
      <header className="rounded-[2rem] border border-white/70 bg-white/80 p-8 shadow-xl shadow-slate-200/70 backdrop-blur">
        <p className="text-sm font-semibold uppercase tracking-[0.25em] text-orange-600">Catálogo premium</p>

        <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950 sm:text-5xl">
          Productos destacados
        </h1>

        <p className="mt-3 max-w-2xl text-base leading-7 text-slate-600">
          Explora productos por categoría, filtros y búsqueda con una experiencia limpia y responsiva.
        </p>

        <p
          className={`mt-4 inline-flex rounded-full px-3 py-1 text-xs font-semibold ${
            backendStatusQuery.data ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"
          }`}
        >
          {backendStatusQuery.data
            ? "Backend conectado (API real)."
            : "Sin conexión al backend: mostrando datos mock."}
        </p>
      </header>

      <section className="grid gap-4 rounded-[1.75rem] border border-white/70 bg-white/80 p-5 shadow-lg shadow-slate-200/60 backdrop-blur md:grid-cols-4">
        <form className="md:col-span-2" onSubmit={onSubmitSearch}>
          <label className="mb-1 block text-sm font-medium">Búsqueda</label>

          <div className="flex gap-2">
            <input
              name="q"
              defaultValue={search}
              placeholder="Buscar por nombre o descripción"
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 outline-none ring-orange-200 transition focus:ring-4"
            />

            <button
              type="submit"
              className="rounded-2xl bg-slate-950 px-5 py-3 font-semibold text-white shadow-lg shadow-slate-200 hover:bg-slate-800"
            >
              Buscar
            </button>
          </div>
        </form>

        <div>
          <label className="mb-1 block text-sm font-medium">Categoría</label>

          <select
            value={categorySlug}
            onChange={(event) =>
              updateParam({
                categoria: event.target.value || undefined,
                page: "1",
              })
            }
            className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 outline-none ring-orange-200 transition focus:ring-4"
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
              className="w-full rounded-2xl border border-slate-200 bg-white px-3 py-3 outline-none ring-orange-200 transition focus:ring-4"
              onBlur={(event) =>
                updateParam({
                  min: event.target.value || undefined,
                  page: "1",
                })
              }
            />

            <input
              type="number"
              placeholder="Máx"
              defaultValue={maxPrice}
              className="w-full rounded-2xl border border-slate-200 bg-white px-3 py-3 outline-none ring-orange-200 transition focus:ring-4"
              onBlur={(event) =>
                updateParam({
                  max: event.target.value || undefined,
                  page: "1",
                })
              }
            />
          </div>
        </div>
      </section>

      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <p className="text-sm text-slate-600">{result?.total ?? 0} producto(s) encontrado(s)</p>

          <button
            type="button"
            onClick={() => router.push("/catalogo")}
            className="rounded-full bg-white px-4 py-2 text-sm font-semibold text-orange-600 shadow-sm hover:text-orange-700"
          >
            Limpiar filtros
          </button>
        </div>

        {productsQuery.isLoading ? (
          <p>Cargando productos...</p>
        ) : result && result.products.length > 0 ? (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {result.products.map((product) => (
              <article
                key={product.id}
                className="group overflow-hidden rounded-[1.75rem] border border-white/70 bg-white/90 shadow-xl shadow-slate-200/70 transition hover:-translate-y-1 hover:shadow-2xl"
              >
                <Image
                  src={product.image ?? "https://placehold.co/600x400?text=Producto"}
                  alt={product.name}
                  width={600}
                  height={400}
                  className="h-52 w-full object-cover transition duration-300 group-hover:scale-105"
                />

                <div className="space-y-2 p-4">
                  <span className="inline-block rounded-full bg-orange-100 px-3 py-1 text-xs font-semibold text-orange-700">
                    {product.categoryName}
                  </span>

                  <h2 className="line-clamp-2 text-lg font-semibold">{product.name}</h2>

                  <p className="line-clamp-2 text-sm leading-6 text-slate-600">
                    {product.description ?? "Sin descripción disponible."}
                  </p>

                  <p className="text-2xl font-black text-slate-950">${product.price.toFixed(2)}</p>

                  <div className="flex items-center gap-2">
                    <Link
                      href={`/catalogo/${product.slug}`}
                      className="inline-block rounded-full border border-slate-200 px-4 py-2 text-sm font-semibold hover:border-orange-300 hover:text-orange-700"
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
            ))}
          </div>
        ) : (
          <div className="rounded-[1.75rem] border border-dashed border-slate-300 bg-white/70 p-10 text-center text-sm text-slate-600">
            No hay productos para los filtros seleccionados.
          </div>
        )}

        {result && result.totalPages > 1 && (
          <nav className="flex items-center justify-center gap-2">
            <button
              type="button"
              disabled={result.currentPage <= 1}
              onClick={() => updateParam({ page: String(result.currentPage - 1) })}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 font-semibold disabled:opacity-40"
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
              className="rounded-full border border-slate-200 bg-white px-4 py-2 font-semibold disabled:opacity-40"
            >
              Siguiente
            </button>
          </nav>
        )}
      </section>
    </main>
  );
}