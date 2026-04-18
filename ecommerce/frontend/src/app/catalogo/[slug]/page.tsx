import Link from "next/link";
import { notFound } from "next/navigation";
import { getProductBySlug } from "@/modules/catalogo/services/catalog-service";
import { AddToCartButton } from "@/modules/catalogo/components/add-to-cart-button";
 type ProductDetailPageProps = {
  params: Promise<{ slug: string }>;
};

export default async function ProductDetailPage({ params }: ProductDetailPageProps) {
  const { slug } = await params;
  const product = await getProductBySlug(slug);

  if (!product) {
    notFound();
  }


  return (
    <main className="mx-auto min-h-screen w-full max-w-4xl space-y-6 px-6 py-10">
      <Link href="/catalogo" className="text-sm text-blue-600 underline">
        ← Volver al catálogo
      </Link>

      <article className="grid gap-6 rounded-xl border p-6 md:grid-cols-2">
        <img src={product.image ?? "https://placehold.co/800x600?text=Producto"} alt={product.name} className="h-72 w-full rounded-lg object-cover" />

        <div className="space-y-4">
          <span className="inline-block rounded-full bg-gray-100 px-3 py-1 text-xs">
            {product.categoryName}
          </span>

          <h1 className="text-3xl font-bold">{product.name}</h1>
          <p className="text-gray-700">{product.description}</p>

          <div className="space-y-2 rounded-lg bg-gray-50 p-4">
            <p className="text-2xl font-bold">${product.price.toFixed(2)}</p>
            <p className="text-sm text-gray-600">Stock disponible: {product.stock}</p>
          </div>
{/* 
          <button type="button" className="rounded-md bg-black px-4 py-2 text-white">
            Agregar al carrito
          </button> */}
           <AddToCartButton
            product={{
              id: product.id,
              slug: product.slug,
              name: product.name,
              price: product.price,
              stock: product.stock,
            }}
          />
        </div>
      </article>
    </main>
  );
}
