import { Product, ProductFilters, ProductListResult } from "../types";

function normalize(value?: string) {
  return (value ?? "").trim().toLowerCase();
}

export function getFallbackProducts(products: Product[], filters: ProductFilters): ProductListResult {
  const filtered = products.filter((product) => {
    const matchesSearch =
      !filters.search ||
      normalize(product.name).includes(normalize(filters.search)) ||
      normalize(product.description).includes(normalize(filters.search));

    const matchesCategory = !filters.categorySlug || product.categorySlug === filters.categorySlug;
    const matchesMinPrice = filters.minPrice === undefined || product.price >= filters.minPrice;
    const matchesMaxPrice = filters.maxPrice === undefined || product.price <= filters.maxPrice;

    return matchesSearch && matchesCategory && matchesMinPrice && matchesMaxPrice;
  });

  const total = filtered.length;
  const safePageSize = Math.max(1, filters.pageSize);
  const totalPages = Math.max(1, Math.ceil(total / safePageSize));
  const currentPage = Math.min(Math.max(1, filters.page), totalPages);
  const start = (currentPage - 1) * safePageSize;

  return {
    products: filtered.slice(start, start + safePageSize),
    total,
    totalPages,
    currentPage,
  };
}
