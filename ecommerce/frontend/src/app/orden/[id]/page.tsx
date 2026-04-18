"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { getOrderById } from "@/modules/services/checkout-service";

export default function OrderDetailPage() {
  const params = useParams<{ id: string }>();
  const orderId = params?.id;

  const orderQuery = useQuery({
    queryKey: ["order-detail", orderId],
    queryFn: () => getOrderById(orderId!),
    enabled: Boolean(orderId),
    retry: false,
  });

  if (orderQuery.isLoading) {
    return (
      <main className="mx-auto min-h-screen w-full max-w-4xl px-6 py-10">
        <p>Cargando orden...</p>
      </main>
    );
  }

  if (orderQuery.isError || !orderQuery.data) {
    return (
      <main className="mx-auto min-h-screen w-full max-w-4xl space-y-4 px-6 py-10">
        <h1 className="text-2xl font-bold">No pudimos cargar la orden</h1>
        <p className="text-sm text-gray-600">Verificá el ID o intentá nuevamente.</p>
        <Link href="/catalogo" className="text-sm text-blue-600 underline">
          Volver al catálogo
        </Link>
      </main>
    );
  }

  const order = orderQuery.data;

  return (
    <main className="mx-auto min-h-screen w-full max-w-4xl space-y-6 px-6 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl font-bold">¡Compra confirmada!</h1>
        <p className="text-sm text-gray-600">Orden #{order.orderId}</p>
        <p className="text-sm text-gray-600">Estado: {order.status}</p>
        <p className="text-sm text-gray-600">
          Fecha: {new Date(order.createdAtUtc).toLocaleString("es-AR")}
        </p>
      </header>

      <section className="space-y-3 rounded-xl border p-4">
        {order.items.map((item) => (
          <article key={item.productId} className="flex items-center justify-between border-b pb-2 last:border-0 last:pb-0">
            <div>
              <p className="font-medium">{item.productName}</p>
              <p className="text-sm text-gray-600">Cantidad: {item.quantity}</p>
            </div>
            <p className="font-semibold">${item.lineTotal.toFixed(2)}</p>
          </article>
        ))}

        <div className="pt-2 text-right">
          <p className="text-xl font-bold">Total: ${order.total.toFixed(2)}</p>
        </div>
      </section>

      <Link href="/catalogo" className="inline-block rounded-md border px-4 py-2 text-sm">
        Seguir comprando
      </Link>
    </main>
  );
}