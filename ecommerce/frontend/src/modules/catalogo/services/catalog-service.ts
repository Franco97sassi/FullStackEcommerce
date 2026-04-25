import { apiClient } from "@/lib/api/api-client";
import { mockCategories, mockProducts } from "../data/mock-catalog-data";
import { Category, Product, ProductFilters, ProductListResult } from "../types";
import { getFallbackProducts } from "./catalog-fallback-utils";
type ApiPagedProducts = {
  items: Product[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
};

 

export async function getCategories(): Promise<Category[]> {
  try {
    const response = await apiClient.get<Category[]>("/api/catalog/categories");
    return response.data;
  } catch {
    return mockCategories;
  }
}

export async function getProducts(filters: ProductFilters): Promise<ProductListResult> {
  try {
    const response = await apiClient.get<ApiPagedProducts>("/api/catalog/products", {
      params: {
        search: filters.search?.trim() || undefined,
        category: filters.categorySlug || undefined,
        minPrice: filters.minPrice,
        maxPrice: filters.maxPrice,
        page: filters.page,
        pageSize: filters.pageSize,
      },
    });

    return {
      products: response.data.items,
      total: response.data.total,
      totalPages: response.data.totalPages,
      currentPage: response.data.page,
    };
  } catch {
        return getFallbackProducts(mockProducts, filters);
 
  }
}

export async function getProductBySlug(slug: string): Promise<Product | null> {
  try {
    const response = await apiClient.get<Product>(`/api/catalog/products/${slug}`);
    return response.data;
  } catch {
    return mockProducts.find((item) => item.slug === slug) ?? null;
  }
}
