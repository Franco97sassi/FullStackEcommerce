"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getCategories = getCategories;
exports.getProducts = getProducts;
exports.getProductBySlug = getProductBySlug;
const api_client_1 = require("@/lib/api/api-client");
const mock_catalog_data_1 = require("../data/mock-catalog-data");
const catalog_fallback_utils_1 = require("./catalog-fallback-utils");
async function getCategories() {
    try {
        const response = await api_client_1.apiClient.get("/api/catalog/categories");
        return response.data;
    }
    catch {
        return mock_catalog_data_1.mockCategories;
    }
}
async function getProducts(filters) {
    try {
        const response = await api_client_1.apiClient.get("/api/catalog/products", {
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
    }
    catch {
        return (0, catalog_fallback_utils_1.getFallbackProducts)(mock_catalog_data_1.mockProducts, filters);
    }
}
async function getProductBySlug(slug) {
    try {
        const response = await api_client_1.apiClient.get(`/api/catalog/products/${slug}`);
        return response.data;
    }
    catch {
        return mock_catalog_data_1.mockProducts.find((item) => item.slug === slug) ?? null;
    }
}
