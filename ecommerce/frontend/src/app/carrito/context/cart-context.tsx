"use client";

import { useCart } from "../context/cart-context";

type AddToCartButtonProps = {
  product: {
    id: string;
    slug: string;
    name: string;
    price: number;
    stock: number;
  };
};

export function AddToCartButton({ product }: AddToCartButtonProps) {
  const { addItem } = useCart();

  const disabled = product.stock <= 0;

  return (
    <button
      type="button"
      disabled={disabled}
      onClick={() =>
        addItem({
          productId: product.id,
          slug: product.slug,
          name: product.name,
          price: product.price,
          stock: product.stock,
        })
      }
      className="rounded-md bg-black px-4 py-2 text-sm text-white disabled:cursor-not-allowed disabled:opacity-40"
    >
      {disabled ? "Sin stock" : "Agregar al carrito"}
    </button>
  );
}
