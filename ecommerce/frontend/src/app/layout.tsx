import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";
import { QueryProvider } from "@/prodivers/query-provider";
import { AuthProvider } from "@/app/auth/context/auth-context";
import { CartProvider } from "@/app/carrito/context/cart-context";

export const metadata: Metadata = {
   title: "Ecommerce",
  description: "Tienda online con catálogo, carrito, checkout seguro y seguimiento de compras.",};

const navLinks = [
  { href: "/catalogo", label: "Catálogo" },
  { href: "/carrito", label: "Carrito" },
  { href: "/ordenes", label: "Mis compras" },
  { href: "/login", label: "Login" },
  { href: "/admin", label: "Admin" },
];

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es">
      <body>
        <QueryProvider>
          <AuthProvider>
            <CartProvider>
              <div className="min-h-screen">
                <header className="sticky top-0 z-50 border-b border-white/60 bg-white/80 shadow-sm shadow-slate-200/60 backdrop-blur-xl">
                  <nav className="mx-auto flex w-full max-w-7xl items-center justify-between gap-6 px-6 py-4">
                    <Link href="/" className="group flex items-center gap-3 font-semibold text-slate-950">
                      <span className="grid size-10 place-items-center rounded-2xl bg-slate-950 text-lg text-white shadow-lg shadow-orange-200 transition group-hover:rotate-3">
                        ✦
                      </span>

                      <span>
                        <span className="block text-base leading-none">Ecommerce</span>
                        <span className="text-xs font-medium text-slate-500">Tienda online</span>                      </span>
                    </Link>

                    <div className="hidden items-center gap-1 rounded-full border border-slate-200 bg-white/70 p-1 text-sm text-slate-700 shadow-sm md:flex">
                      {navLinks.map((link) => (
                        <Link
                          key={link.href}
                          href={link.href}
                          className="rounded-full px-4 py-2 hover:bg-slate-950 hover:text-white"
                        >
                          {link.label}
                        </Link>
                      ))}
                    </div>

                    <Link
                      href="/catalogo"
                      className="rounded-full bg-orange-500 px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-orange-200 hover:bg-orange-600"
                    >
                      Comprar ahora
                    </Link>
                  </nav>
                </header>

                {children}
              </div>
            </CartProvider>
          </AuthProvider>
        </QueryProvider>
      </body>
    </html>
  );
}