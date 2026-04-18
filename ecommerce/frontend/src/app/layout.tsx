import type { Metadata } from "next";
import "./globals.css";
 import { QueryProvider } from "@/prodivers/query-provider";
import { CartProvider } from "@/app/carrito/context/cart-context";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Ecommerce",
  description: "Ecommerce con Next.js y ASP.NET Core",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es">
      <body>
        <QueryProvider>
          <CartProvider>
            <header className="border-b bg-white">
              <nav className="mx-auto flex w-full max-w-6xl items-center justify-between px-6 py-4">
                <Link href="/" className="font-semibold">
                  Ecommerce
                </Link>

                <div className="flex items-center gap-4 text-sm">
                  <Link href="/catalogo" className="hover:underline">
                    Catálogo
                  </Link>
                  <Link href="/carrito" className="hover:underline">
                    Carrito
                  </Link>
                </div>
              </nav>
            </header>

            {children}
          </CartProvider>
        </QueryProvider>
      </body>
    </html>
  );
}