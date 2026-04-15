import Link from "next/link";
import { notFound } from "next/navigation";
import { getOrderById } from "@/modules/checkout/services/checkout-service";

type OrderDetailPageProps = {
  params: Promise<{ id: string }>;
};

export default async function OrderDetailPage({ params }: OrderDetailPageProps) {
  const { id } = await params;

  let order;
  try {
    order = await getOrderById(id);
  } catch {
    notFound();
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-4xl space-y-6 px-6 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl font-bold">¡Compra confirmada!</h1>
        <p className="text-sm text-gray-600">Orden #{order.orderId}</p>
      </header>

      <section className="space-y-2 rounded-xl border p-4">
        <p className="text-sm text-gray-600">Estado: {order.status}</p>
        <p className="text-sm text-gray-600">Fecha: {new Date(order.createdAtUtc).toLocaleString("es-AR")}</p>
        <p className="text-2xl font-bold">Total: ${order.total.toFixed(2)}</p>
      </section>

      <section className="space-y-3">
        {order.items.map((item) => (
          <article key={item.productId} className="rounded-lg border p-3">
            <h2 className="font-semibold">{item.productName}</h2>
            <p className="text-sm text-gray-600">
              {item.quantity} x ${item.unitPrice.toFixed(2)} = ${item.lineTotal.toFixed(2)}
            </p>
          </article>
        ))}
      </section>

      <Link href="/catalogo" className="text-sm text-blue-600 underline">
        Seguir comprando
      </Link>
    </main>
  );
}
