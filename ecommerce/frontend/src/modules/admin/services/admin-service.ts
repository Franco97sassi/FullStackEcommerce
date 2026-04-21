import { apiClient } from "@/lib/api/api-client";
import { AdminCategory, AdminOrder, AdminProduct, AdminUser } from "@/modules/admin/types";

export async function getAdminCategories() {
  const response = await apiClient.get<AdminCategory[]>("/api/admin/categories");
  return response.data;
}

export async function getAdminProducts() {
  const response = await apiClient.get<AdminProduct[]>("/api/admin/products");
  return response.data;
}

export async function getAdminOrders() {
  const response = await apiClient.get<AdminOrder[]>("/api/admin/orders");
  return response.data;
}

export async function getAdminUsers() {
  const response = await apiClient.get<AdminUser[]>("/api/admin/users");
  return response.data;
}

export async function updateOrderStatus(orderId: string, status: string) {
  const response = await apiClient.patch<AdminOrder>(`/api/admin/orders/${orderId}/status`, { status });
  return response.data;
}

export async function updateUserRole(userId: string, role: string) {
  const response = await apiClient.patch<AdminUser>(`/api/admin/users/${userId}/role`, { role });
  return response.data;
}

export async function updateProductStock(productId: string, stock: number) {
  const response = await apiClient.patch<AdminProduct>(`/api/admin/products/${productId}/stock`, { stock });
  return response.data;
}
