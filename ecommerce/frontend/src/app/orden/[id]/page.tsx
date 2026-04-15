"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { createOrder } from "@/modules/checkout/services/checkout-service";
import { useCart } from "@/modules/carrito/context/cart-context";

export default function CartPage() {
  const router = useRouter();
  const { items, removeItem, updateQuantity, totalAmount, totalItems, clearCart } = useCart();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleCheckout() {
    if (items.length === 0) {
      setError("El carrito está vacío.");
      return;
    }

    try {
      setError(null);
      setIsSubmitting(true);
      const order = await createOrder(items);
      clearCart();
      router.push(`/orden/${order.orderId}`);
    } catch {
      setError("No se pudo completar la compra. Revisá stock o intentá nuevamente.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-4xl space-y-6 px-6 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl font-bold">Carrito</h1>
        <p className="text-sm text-gray-600">Revisá tus productos antes de confirmar la compra.</p>
      </header>

      {items.length === 0 ? (
        <section className="rounded-xl border border-dashed p-8 text-center">
          <p className="text-sm text-gray-600">Todavía no agregaste productos.</p>
          <Link href="/catalogo" className="mt-3 inline-block text-sm text-blue-600 underline">
            Ir al catálogo
          </Link>
        </section>
      ) : (
        <section className="space-y-4">
          {items.map((item) => (
            <article key={item.productId} className="flex items-center justify-between rounded-xl border p-4">
              <div>
                <h2 className="font-semibold">{item.name}</h2>
                <p className="text-sm text-gray-600">${item.price.toFixed(2)} c/u</p>
              </div>

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                  className="rounded border px-3 py-1"
                >
                  -
                </button>
                <span className="w-8 text-center">{item.quantity}</span>
                <button
                  type="button"
                  onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                  className="rounded border px-3 py-1"
                >
                  +
                </button>
                <button
                  type="button"
                  onClick={() => removeItem(item.productId)}
                  className="ml-3 text-sm text-red-600 underline"
                >
                  Eliminar
                </button>
              </div>
            </article>
          ))}

          <section className="space-y-2 rounded-xl bg-gray-50 p-4">
            <p className="text-sm">Items: {totalItems}</p>
            <p className="text-2xl font-bold">Total: ${totalAmount.toFixed(2)}</p>

            {error && <p className="text-sm text-red-600">{error}</p>}

            <button
              type="button"
              disabled={isSubmitting}
              onClick={handleCheckout}
              className="rounded-md bg-black px-4 py-2 text-white disabled:opacity-50"
            >
              {isSubmitting ? "Procesando..." : "Confirmar compra"}
            </button>
          </section>
        </section>
      )}
    </main>
  );
}
