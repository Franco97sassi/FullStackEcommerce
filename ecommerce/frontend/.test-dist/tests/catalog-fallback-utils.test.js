"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const node_test_1 = __importDefault(require("node:test"));
const strict_1 = __importDefault(require("node:assert/strict"));
const catalog_fallback_utils_1 = require("../src/modules/catalogo/services/catalog-fallback-utils");
const products = [
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
(0, node_test_1.default)("filtra por búsqueda, categoría y rango de precio", () => {
    const result = (0, catalog_fallback_utils_1.getFallbackProducts)(products, {
        search: "deportivo",
        categorySlug: "ropa",
        minPrice: 30,
        maxPrice: 40,
        page: 1,
        pageSize: 10,
    });
    strict_1.default.equal(result.total, 1);
    strict_1.default.equal(result.products[0]?.slug, "short-deportivo");
    strict_1.default.equal(result.totalPages, 1);
});
(0, node_test_1.default)("pagina correctamente y ajusta página fuera de rango", () => {
    const result = (0, catalog_fallback_utils_1.getFallbackProducts)(products, {
        search: "",
        categorySlug: "",
        page: 99,
        pageSize: 2,
    });
    strict_1.default.equal(result.total, 3);
    strict_1.default.equal(result.totalPages, 2);
    strict_1.default.equal(result.currentPage, 2);
    strict_1.default.equal(result.products.length, 1);
    strict_1.default.equal(result.products[0]?.slug, "short-deportivo");
});
(0, node_test_1.default)("mantiene totalPages en 1 cuando no hay resultados", () => {
    const result = (0, catalog_fallback_utils_1.getFallbackProducts)(products, {
        search: "no-existe",
        categorySlug: "",
        page: 1,
        pageSize: 6,
    });
    strict_1.default.equal(result.total, 0);
    strict_1.default.equal(result.totalPages, 1);
    strict_1.default.equal(result.currentPage, 1);
    strict_1.default.deepEqual(result.products, []);
});
