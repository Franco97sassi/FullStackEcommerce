"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.apiClient = void 0;
const axios_1 = __importDefault(require("axios"));
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5138";
exports.apiClient = axios_1.default.create({
    // baseURL: process.env.NEXT_PUBLIC_API_URL,
    baseURL: API_BASE_URL,
    headers: {
        "Content-Type": "application/json",
    },
    // });
});
exports.apiClient.interceptors.request.use((config) => {
    if (typeof window === "undefined") {
        return config;
    }
    const raw = window.localStorage.getItem("ecommerce_auth_session");
    if (!raw) {
        return config;
    }
    try {
        const session = JSON.parse(raw);
        if (session.token) {
            config.headers.Authorization = `Bearer ${session.token}`;
        }
    }
    catch {
        window.localStorage.removeItem("ecommerce_auth_session");
    }
    return config;
});
