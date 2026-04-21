"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getAdminCategories, getAdminOrders, getAdminProducts, getAdminUsers, updateOrderStatus, updateProductStock, updateUserRole } from "@/modules/admin/services/admin-service";

export function AdminDashboard() {
  const queryClient = useQueryClient();

  const categoriesQuery = useQuery({ queryKey: ["admin-categories"], queryFn: getAdminCategories });
  const productsQuery = useQuery({ queryKey: ["admin-products"], queryFn: getAdminProducts });
  const ordersQuery = useQuery({ queryKey: ["admin-orders"], queryFn: getAdminOrders });
  const usersQuery = useQuery({ queryKey: ["admin-users"], queryFn: getAdminUsers });

  const refreshAll = () => {
    void queryClient.invalidateQueries({ queryKey: ["admin-categories"] });
    void queryClient.invalidateQueries({ queryKey: ["admin-products"] });
    void queryClient.invalidateQueries({ queryKey: ["admin-orders"] });
    void queryClient.invalidateQueries({ queryKey: ["admin-users"] });
  };

  const orderMutation = useMutation({
    mutationFn: ({ orderId, status }: { orderId: string; status: string }) => updateOrderStatus(orderId, status),
    onSuccess: refreshAll,
  });

  const userMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) => updateUserRole(userId, role),
    onSuccess: refreshAll,
  });

  const stockMutation = useMutation({
    mutationFn: ({ productId, stock }: { productId: string; stock: number }) => updateProductStock(productId, stock),
    onSuccess: refreshAll,
  });

  return (
    <main className="mx-auto w-full max-w-6xl space-y-10 px-6 py-8">
      <h1 className="text-3xl font-bold">Panel Admin</h1>

      <section>
        <h2 className="mb-3 text-xl font-semibold">Categorías</h2>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {(categoriesQuery.data ?? []).map((category) => (
            <article key={category.id} className="rounded-lg border p-4">
              <p className="font-semibold">{category.name}</p>
              <p className="text-sm text-gray-600">slug: {category.slug}</p>
              <p className="text-sm text-gray-600">Productos: {category.productCount}</p>
              <p className="text-sm text-gray-600">Activa: {category.isActive ? "Sí" : "No"}</p>
            </article>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-xl font-semibold">Productos (Stock)</h2>
        <div className="space-y-2">
          {(productsQuery.data ?? []).slice(0, 12).map((product) => (
            <div key={product.id} className="flex flex-wrap items-center gap-3 rounded-lg border p-3">
              <div className="min-w-64 flex-1">
                <p className="font-semibold">{product.name}</p>
                <p className="text-sm text-gray-600">{product.categoryName} · stock actual: {product.stock}</p>
              </div>
              <button
                className="rounded-md border px-3 py-1 text-sm"
                onClick={() => stockMutation.mutate({ productId: product.id, stock: Math.max(0, product.stock + 5) })}
              >
                +5 stock
              </button>
              <button
                className="rounded-md border px-3 py-1 text-sm"
                onClick={() => stockMutation.mutate({ productId: product.id, stock: Math.max(0, product.stock - 5) })}
              >
                -5 stock
              </button>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-xl font-semibold">Órdenes</h2>
        <div className="space-y-2">
          {(ordersQuery.data ?? []).slice(0, 10).map((order) => (
            <div key={order.id} className="flex flex-wrap items-center gap-3 rounded-lg border p-3">
              <div className="min-w-64 flex-1 text-sm">
                <p className="font-semibold">{order.userEmail}</p>
                <p>Estado: {order.status} · Ítems: {order.itemCount}</p>
              </div>
              <button
                className="rounded-md border px-3 py-1 text-sm"
                onClick={() => orderMutation.mutate({ orderId: order.id, status: "Shipped" })}
              >
                Marcar Shipped
              </button>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-xl font-semibold">Usuarios</h2>
        <div className="space-y-2">
          {(usersQuery.data ?? []).slice(0, 10).map((user) => (
            <div key={user.id} className="flex flex-wrap items-center gap-3 rounded-lg border p-3">
              <div className="min-w-64 flex-1 text-sm">
                <p className="font-semibold">{user.fullName}</p>
                <p>{user.email} · Rol: {user.role}</p>
              </div>
              <button
                className="rounded-md border px-3 py-1 text-sm"
                onClick={() => userMutation.mutate({ userId: user.id, role: user.role === "Admin" ? "Customer" : "Admin" })}
              >
                Cambiar rol
              </button>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}
