import { apiClient } from "@/lib/api/api-client";
import { AdminCategory, AdminOrder, AdminProduct, AdminUser } from "@/modules/admin/types";

type CategoryPayload = {
  name: string;
  slug: string;
  description?: string;
  isActive: boolean;
};

type ProductPayload = {
  name: string;
  slug: string;
  description?: string;
  price: number;
  stock: number;
  isActive: boolean;
  categoryId: string;
};

export async function getAdminCategories() {
  const response = await apiClient.get<AdminCategory[]>("/api/admin/categories");
  return response.data;
}

export async function createAdminCategory(payload: CategoryPayload) {
  const response = await apiClient.post<AdminCategory>("/api/admin/categories", payload);
  return response.data;
}

export async function updateAdminCategory(categoryId: string, payload: CategoryPayload) {
  const response = await apiClient.put<AdminCategory>(`/api/admin/categories/${categoryId}`, payload);
  return response.data;
}

export async function deleteAdminCategory(categoryId: string) {
  await apiClient.delete(`/api/admin/categories/${categoryId}`);
}

export async function getAdminProducts() {
  const response = await apiClient.get<AdminProduct[]>("/api/admin/products");
  return response.data;
}

export async function createAdminProduct(payload: ProductPayload) {
  const response = await apiClient.post<AdminProduct>("/api/admin/products", payload);
  return response.data;
}

export async function updateAdminProduct(productId: string, payload: ProductPayload) {
  const response = await apiClient.put<AdminProduct>(`/api/admin/products/${productId}`, payload);
  return response.data;
}

export async function deleteAdminProduct(productId: string) {
  await apiClient.delete(`/api/admin/products/${productId}`);
}

export async function getAdminOrders() {
  const response = await apiClient.get<AdminOrder[]>("/api/admin/orders");
  return response.data;
}

export async function updateOrderStatus(orderId: string, status: string) {
  const response = await apiClient.patch<AdminOrder>(`/api/admin/orders/${orderId}/status`, { status });
  return response.data;
}

export async function getAdminUsers() {
  const response = await apiClient.get<AdminUser[]>("/api/admin/users");
  return response.data;
}

export async function updateUserRole(userId: string, role: string) {
  const response = await apiClient.patch<AdminUser>(`/api/admin/users/${userId}/role`, { role });
  return response.data;
}

export async function updateUserActive(userId: string, isActive: boolean) {
  const response = await apiClient.patch<AdminUser>(`/api/admin/users/${userId}/active`, { isActive });
  return response.data;
}

export async function updateProductStock(productId: string, stock: number) {
  const response = await apiClient.patch<AdminProduct>(`/api/admin/products/${productId}/stock`, { stock });
  return response.data;
}