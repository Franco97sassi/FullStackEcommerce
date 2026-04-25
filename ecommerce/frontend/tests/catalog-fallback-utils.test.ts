import test from "node:test";
import assert from "node:assert/strict";

import { getFallbackProducts } from "../src/modules/catalogo/services/catalog-fallback-utils";
import type { Product } from "../src/modules/catalogo/types";

const products: Product[] = [
  {
    id: "1",
    name: "Camiseta Running",
    slug: "camiseta-running",
    description: "Tela transpirable",
    price: 50,
    stock: 10,
    categoryName: "Ropa",
    categorySlug: "ropa",
    image: undefined,
  },
  {
    id: "2",
    name: "Zapatillas Pro",
    slug: "zapatillas-pro",
    description: "Suela reforzada",
    price: 120,
    stock: 5,
    categoryName: "Calzado",
    categorySlug: "calzado",
    image: undefined,
  },
  {
    id: "3",
    name: "Short deportivo",
    slug: "short-deportivo",
    description: "Ideal para entrenamiento",
    price: 35,
    stock: 12,
    categoryName: "Ropa",
    categorySlug: "ropa",
    image: undefined,
  },
];

test("filtra por búsqueda, categoría y rango de precio", () => {
  const result = getFallbackProducts(products, {
    search: "deportivo",
    categorySlug: "ropa",
    minPrice: 30,
    maxPrice: 40,
    page: 1,
    pageSize: 10,
  });

  assert.equal(result.total, 1);
  assert.equal(result.products[0]?.slug, "short-deportivo");
  assert.equal(result.totalPages, 1);
});

test("pagina correctamente y ajusta página fuera de rango", () => {
  const result = getFallbackProducts(products, {
    search: "",
    categorySlug: "",
    page: 99,
    pageSize: 2,
  });

  assert.equal(result.total, 3);
  assert.equal(result.totalPages, 2);
  assert.equal(result.currentPage, 2);
  assert.equal(result.products.length, 1);
  assert.equal(result.products[0]?.slug, "short-deportivo");
});

test("mantiene totalPages en 1 cuando no hay resultados", () => {
  const result = getFallbackProducts(products, {
    search: "no-existe",
    categorySlug: "",
    page: 1,
    pageSize: 6,
  });

  assert.equal(result.total, 0);
  assert.equal(result.totalPages, 1);
  assert.equal(result.currentPage, 1);
  assert.deepEqual(result.products, []);
});
