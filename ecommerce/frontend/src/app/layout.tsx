import type { Metadata } from "next";
import "./globals.css";
 import { QueryProvider } from "@/prodivers/query-provider";
import { CartProvider } from "@/app/carrito/context/cart-context";
import Link from "next/link";
 import { AuthProvider } from "@/app/auth/context/auth-context";
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
          {/* <CartProvider>
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
                    Carrito */}
                      <AuthProvider>
            <CartProvider>
                            <header className="border-b bg-white text-slate-900">
                <nav className="mx-auto flex w-full max-w-6xl items-center justify-between px-6 py-4">
                 <Link href="/" className="font-semibold text-slate-900">
                    Ecommerce
                  </Link>
                  
                {/* </div>
              </nav>
            </header>  

            {children}
          </CartProvider>*/}

  <div className="flex items-center gap-4 text-sm text-slate-900">
                    <Link href="/catalogo" className="hover:text-slate-700 hover:underline">
                      Catálogo
                    </Link>
                    <Link href="/carrito" className="hover:text-slate-700 hover:underline">
                      Carrito
                    </Link>
                     <Link href="/ordenes" className="hover:text-slate-700 hover:underline">
                      Mis compras
                    </Link>
                    <Link href="/login" className="hover:text-slate-700 hover:underline">
                      Login
                    </Link>
                    <Link href="/admin" className="hover:text-slate-700 hover:underline">
                      Admin
                    </Link>
                  </div>
                </nav>
              </header>

              {children}
            </CartProvider>
          </AuthProvider>


        </QueryProvider>
      </body>
    </html>
  );
}