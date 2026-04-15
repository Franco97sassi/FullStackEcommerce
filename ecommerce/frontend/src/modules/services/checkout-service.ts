import { apiClient } from "@/lib/api/api-client";
import { CartItem } from "@/modules/carrito/types";

export type OrderItemResponse = {
  productId: string;
  productName: string;
  productSlug: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
};

export type OrderResponse = {
  orderId: string;
  createdAtUtc: string;
  status: string;
  total: number;
  items: OrderItemResponse[];
};

export async function createOrder(items: CartItem[]) {
  const response = await apiClient.post<OrderResponse>("/api/checkout/orders", {
    items: items.map((item) => ({
      productId: item.productId,
      quantity: item.quantity,
    })),
  });

  return response.data;
}

export async function getOrderById(orderId: string) {
  const response = await apiClient.get<OrderResponse>(`/api/checkout/orders/${orderId}`);
  return response.data;
}
