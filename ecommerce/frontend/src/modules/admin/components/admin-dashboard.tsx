"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createAdminCategory,
  createAdminProduct,
  deleteAdminCategory,
  deleteAdminProduct,
  getAdminCategories,
  getAdminOrders,
  getAdminProducts,
  getAdminUsers,
  updateAdminCategory,
  updateAdminProduct,
  updateOrderStatus,
  updateProductStock,
  updateUserActive,
  updateUserRole,
} from "@/modules/admin/services/admin-service";
import { FormEvent, useMemo, useState } from "react";

const INITIAL_CATEGORY_FORM = {
  name: "",
  slug: "",
  description: "",
  isActive: true,
};

const INITIAL_PRODUCT_FORM = {
  name: "",
  slug: "",
  description: "",
  price: 0,
  stock: 0,
  isActive: true,
  categoryId: "",
};

export function AdminDashboard() {
  const queryClient = useQueryClient();
  const [categoryForm, setCategoryForm] = useState(INITIAL_CATEGORY_FORM);
  const [productForm, setProductForm] = useState(INITIAL_PRODUCT_FORM);

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

  const categoryMutation = useMutation({
    mutationFn: createAdminCategory,
    onSuccess: () => {
      setCategoryForm(INITIAL_CATEGORY_FORM);
      refreshAll();
    },
  });

  const categoryDeleteMutation = useMutation({
    mutationFn: deleteAdminCategory,
    onSuccess: refreshAll,
  });

  const categoryToggleMutation = useMutation({
    mutationFn: ({ categoryId, isActive, name, slug, description }: { categoryId: string; isActive: boolean; name: string; slug: string; description?: string | null }) =>
      updateAdminCategory(categoryId, {
        name,
        slug,
        description: description ?? "",
        isActive,
      }),
    onSuccess: refreshAll,
  });

  const productMutation = useMutation({
    mutationFn: createAdminProduct,
    onSuccess: () => {
      setProductForm((prev) => ({ ...INITIAL_PRODUCT_FORM, categoryId: prev.categoryId }));
      refreshAll();
    },
  });

  const productDeleteMutation = useMutation({
    mutationFn: deleteAdminProduct,
    onSuccess: refreshAll,
  });

  const productToggleMutation = useMutation({
    mutationFn: ({
      productId,
      name,
      slug,
      description,
      price,
      stock,
      isActive,
      categoryId,
    }: {
      productId: string;
      name: string;
      slug: string;
      description?: string | null;
      price: number;
      stock: number;
      isActive: boolean;
      categoryId: string;
    }) =>
      updateAdminProduct(productId, {
        name,
        slug,
        description: description ?? "",
        price,
        stock,
        isActive,
        categoryId,
      }),
    onSuccess: refreshAll,
  });

  const orderMutation = useMutation({
    mutationFn: ({ orderId, status }: { orderId: string; status: string }) => updateOrderStatus(orderId, status),
    onSuccess: refreshAll,
  });

  const userRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) => updateUserRole(userId, role),
    onSuccess: refreshAll,
  });

  const userActiveMutation = useMutation({
    mutationFn: ({ userId, isActive }: { userId: string; isActive: boolean }) => updateUserActive(userId, isActive),
    onSuccess: refreshAll,
  });

  const stockMutation = useMutation({
    mutationFn: ({ productId, stock }: { productId: string; stock: number }) => updateProductStock(productId, stock),
    onSuccess: refreshAll,
  });

  const canCreateProduct = useMemo(
    () => Boolean(productForm.name.trim() && productForm.slug.trim() && productForm.categoryId),
    [productForm],
  );

  function onCreateCategory(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!categoryForm.name.trim() || !categoryForm.slug.trim()) return;

    categoryMutation.mutate({
      ...categoryForm,
      name: categoryForm.name.trim(),
      slug: categoryForm.slug.trim().toLowerCase(),
      description: categoryForm.description.trim(),
    });
  }

  function onCreateProduct(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!canCreateProduct) return;

    productMutation.mutate({
      ...productForm,
      name: productForm.name.trim(),
      slug: productForm.slug.trim().toLowerCase(),
      description: productForm.description.trim(),
    });
  }

  return (
    <main className="mx-auto w-full max-w-6xl space-y-10 px-6 py-8">
      <h1 className="text-3xl font-bold">Panel Admin</h1>

      <section className="space-y-4">
        <h2 className="text-xl font-semibold">Categorías (CRUD)</h2>
        <form className="grid gap-3 rounded-lg border p-4 md:grid-cols-4" onSubmit={onCreateCategory}>
          <input
            value={categoryForm.name}
            onChange={(event) => setCategoryForm((prev) => ({ ...prev, name: event.target.value }))}
            placeholder="Nombre"
            className="rounded-md border px-3 py-2"
          />
          <input
            value={categoryForm.slug}
            onChange={(event) => setCategoryForm((prev) => ({ ...prev, slug: event.target.value }))}
            placeholder="slug"
            className="rounded-md border px-3 py-2"
          />
          <input
            value={categoryForm.description}
            onChange={(event) => setCategoryForm((prev) => ({ ...prev, description: event.target.value }))}
            placeholder="Descripción"
            className="rounded-md border px-3 py-2"
          />
          <button type="submit" className="rounded-md bg-black px-4 py-2 text-white">
            Crear categoría
          </button>
        </form>

        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {(categoriesQuery.data ?? []).map((category) => (
            <article key={category.id} className="space-y-2 rounded-lg border p-4">
              <p className="font-semibold">{category.name}</p>
              <p className="text-sm text-gray-600">slug: {category.slug}</p>
              <p className="text-sm text-gray-600">Productos: {category.productCount}</p>
              <p className="text-sm text-gray-600">Activa: {category.isActive ? "Sí" : "No"}</p>
              <div className="flex flex-wrap gap-2">
                <button
                  className="rounded-md border px-3 py-1 text-sm"
                  onClick={() =>
                    categoryToggleMutation.mutate({
                      categoryId: category.id,
                      name: category.name,
                      slug: category.slug,
                      description: category.description,
                      isActive: !category.isActive,
                    })
                  }
                >
                  {category.isActive ? "Desactivar" : "Activar"}
                </button>
                <button
                  className="rounded-md border px-3 py-1 text-sm text-red-600"
                  onClick={() => categoryDeleteMutation.mutate(category.id)}
                >
                  Eliminar
                </button>
              </div>
            </article>
          ))}
        </div>
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-semibold">Productos (CRUD + stock)</h2>
        <form className="grid gap-3 rounded-lg border p-4 md:grid-cols-4" onSubmit={onCreateProduct}>
          <input value={productForm.name} onChange={(event) => setProductForm((prev) => ({ ...prev, name: event.target.value }))} placeholder="Nombre" className="rounded-md border px-3 py-2" />
          <input value={productForm.slug} onChange={(event) => setProductForm((prev) => ({ ...prev, slug: event.target.value }))} placeholder="slug" className="rounded-md border px-3 py-2" />
          <input type="number" value={productForm.price} onChange={(event) => setProductForm((prev) => ({ ...prev, price: Number(event.target.value) }))} placeholder="Precio" className="rounded-md border px-3 py-2" />
          <input type="number" value={productForm.stock} onChange={(event) => setProductForm((prev) => ({ ...prev, stock: Number(event.target.value) }))} placeholder="Stock" className="rounded-md border px-3 py-2" />
          <select value={productForm.categoryId} onChange={(event) => setProductForm((prev) => ({ ...prev, categoryId: event.target.value }))} className="rounded-md border px-3 py-2">
            <option value="">Seleccionar categoría</option>
            {(categoriesQuery.data ?? []).map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
          <input value={productForm.description} onChange={(event) => setProductForm((prev) => ({ ...prev, description: event.target.value }))} placeholder="Descripción" className="rounded-md border px-3 py-2 md:col-span-2" />
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={productForm.isActive} onChange={(event) => setProductForm((prev) => ({ ...prev, isActive: event.target.checked }))} />
            Activo
          </label>
          <button type="submit" disabled={!canCreateProduct} className="rounded-md bg-black px-4 py-2 text-white disabled:opacity-50">
            Crear producto
          </button>
        </form>

        <div className="space-y-2">
          {(productsQuery.data ?? []).map((product) => (
            <div key={product.id} className="flex flex-wrap items-center gap-3 rounded-lg border p-3">
              <div className="min-w-64 flex-1">
                <p className="font-semibold">{product.name}</p>
                <p className="text-sm text-gray-600">{product.categoryName} · stock: {product.stock} · ${product.price.toFixed(2)}</p>
              </div>
              <button className="rounded-md border px-3 py-1 text-sm" onClick={() => stockMutation.mutate({ productId: product.id, stock: Math.max(0, product.stock + 5) })}>
                +5 stock
              </button>
              <button className="rounded-md border px-3 py-1 text-sm" onClick={() => stockMutation.mutate({ productId: product.id, stock: Math.max(0, product.stock - 5) })}>
                -5 stock
              </button>
              <button
                className="rounded-md border px-3 py-1 text-sm"
                onClick={() =>
                  productToggleMutation.mutate({
                    productId: product.id,
                    name: product.name,
                    slug: product.slug,
                    description: product.description,
                    price: product.price,
                    stock: product.stock,
                    isActive: !product.isActive,
                    categoryId: product.categoryId,
                  })
                }
              >
                {product.isActive ? "Desactivar" : "Activar"}
              </button>
              <button className="rounded-md border px-3 py-1 text-sm text-red-600" onClick={() => productDeleteMutation.mutate(product.id)}>
                Eliminar
              </button>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-xl font-semibold">Órdenes</h2>
        <div className="space-y-2">
          {(ordersQuery.data ?? []).slice(0, 20).map((order) => (
            <div key={order.id} className="flex flex-wrap items-center gap-3 rounded-lg border p-3">
              <div className="min-w-64 flex-1 text-sm">
                <p className="font-semibold">{order.userEmail}</p>
                <p>Estado: {order.status} · Ítems: {order.itemCount}</p>
              </div>
              <button className="rounded-md border px-3 py-1 text-sm" onClick={() => orderMutation.mutate({ orderId: order.id, status: "Shipped" })}>
                Marcar Shipped
              </button>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-xl font-semibold">Usuarios</h2>
        <div className="space-y-2">
          {(usersQuery.data ?? []).slice(0, 20).map((user) => (
            <div key={user.id} className="flex flex-wrap items-center gap-3 rounded-lg border p-3">
              <div className="min-w-64 flex-1 text-sm">
                <p className="font-semibold">{user.fullName}</p>
                <p>{user.email} · Rol: {user.role}</p>
                <p>Activo: {user.isActive ? "Sí" : "No"}</p>
              </div>
              <button className="rounded-md border px-3 py-1 text-sm" onClick={() => userRoleMutation.mutate({ userId: user.id, role: user.role === "Admin" ? "Customer" : "Admin" })}>
                Cambiar rol
              </button>
              <button className="rounded-md border px-3 py-1 text-sm" onClick={() => userActiveMutation.mutate({ userId: user.id, isActive: !user.isActive })}>
                {user.isActive ? "Desactivar" : "Activar"}
              </button>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}