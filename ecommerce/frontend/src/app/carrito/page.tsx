"use client";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useCart } from "@/app/carrito/context/cart-context";
import { createOrder } from "@/modules/services/checkout-service";
import Link from "next/link";
export default function CartPage() {
  const router = useRouter();
  const { items, totalAmount, totalItems, updateQuantity, removeItem, clearCart } = useCart();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onCheckout() {
    if (items.length === 0 || isSubmitting) return;

    try {
      setIsSubmitting(true);
      setError(null);

      const order = await createOrder(items);
      clearCart();
      router.push(`/orden/${order.orderId}`);
    } catch {
      setError("No se pudo crear la orden. Intentá nuevamente.");
    } finally {
      setIsSubmitting(false);
    }
  }

   <main className="mx-auto min-h-screen w-full max-w-5xl space-y-6 px-6 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl font-bold">Carrito</h1>
        <p className="text-sm text-gray-600">Revisá tus productos antes de confirmar la compra.</p>
      </header>

      {items.length === 0 ? (
        <section className="space-y-4 rounded-xl border border-dashed p-6 text-center">
          <p className="text-gray-600">Tu carrito está vacío.</p>
          <Link href="/catalogo" className="inline-block rounded-md bg-black px-4 py-2 text-sm text-white">
            Ir al catálogo
          </Link>
        </section>
      ) : (
        <>
          <section className="space-y-3 rounded-xl border p-4">
            {items.map((item) => (
              <article
                key={item.productId}
                className="flex flex-col gap-3 border-b pb-3 last:border-0 last:pb-0 md:flex-row md:items-center md:justify-between"
              >
                <div>
                  <Link href={`/catalogo/${item.slug}`} className="font-medium hover:underline">
                    {item.name}
                  </Link>
                  <p className="text-sm text-gray-600">Precio unitario: ${item.price.toFixed(2)}</p>
                  <p className="text-xs text-gray-500">Stock máximo: {item.stock}</p>
                </div>

                   <div className="flex items-center gap-2">
                  <button
                    type="button"
                    className="rounded-md border px-3 py-1"
                    onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                    aria-label={`Disminuir cantidad de ${item.name}`}
                  >
                    -
                  </button>

                  <span className="min-w-10 text-center text-sm font-medium">{item.quantity}</span>

                  <button
                    type="button"
                    className="rounded-md border px-3 py-1"
                    onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                    disabled={item.quantity >= item.stock}
                    aria-label={`Aumentar cantidad de ${item.name}`}
                  >
                    +
                  </button>

                  <button
                    type="button"
                    className="ml-2 rounded-md border px-3 py-1 text-sm"
                    onClick={() => removeItem(item.productId)}
                  >
                    Quitar
                  </button>
                </div>
              </article>
            ))}
          </section>

          <section className="space-y-2 rounded-xl bg-gray-50 p-4">
            <p className="text-sm text-gray-700">Productos: {totalItems}</p>
            <p className="text-xl font-bold">Total: ${totalAmount.toFixed(2)}</p>

            {error && <p className="text-sm text-red-600">{error}</p>}

            <div className="flex flex-wrap gap-2 pt-2">
              <button
                type="button"
                onClick={onCheckout}
                disabled={isSubmitting}
                className="rounded-md bg-black px-4 py-2 text-sm text-white disabled:opacity-60"
              >
                {isSubmitting ? "Procesando..." : "Confirmar compra"}
              </button>

              <button
                type="button"
                onClick={clearCart}
                className="rounded-md border px-4 py-2 text-sm"
              >
                Vaciar carrito
              </button>

              <Link href="/catalogo" className="rounded-md border px-4 py-2 text-sm">
                Seguir comprando
              </Link>
            </div>
          </section>
        </>
      )}
    </main>

    }