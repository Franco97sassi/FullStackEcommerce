import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";
import { QueryProvider } from "@/prodivers/query-provider";
import { AuthProvider } from "@/app/auth/context/auth-context";
import { CartProvider } from "@/app/carrito/context/cart-context";

export const metadata: Metadata = {
  title: "VANTA — Movimiento cotidiano",
  description: "Moda, tecnología y rendimiento para todos los días.",
};

const categories = ["Novedades", "Calzado", "Indumentaria", "Tecnología", "Deportes", "Ofertas"];

function SearchIcon() {
  return <svg viewBox="0 0 24 24" aria-hidden="true" className="size-5 fill-none stroke-current stroke-2"><circle cx="11" cy="11" r="7"/><path d="m16.5 16.5 4 4"/></svg>;
}

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="es">
      <body>
        <QueryProvider><AuthProvider><CartProvider>
          <div className="min-h-screen bg-white">
            <div className="bg-black px-4 py-2 text-center text-[11px] font-bold uppercase tracking-[0.14em] text-white">
              Envío gratis desde $100 · Cambios sin costo por 30 días
            </div>
            <header className="sticky top-0 z-50 border-b border-neutral-200 bg-white/95 backdrop-blur-lg">
              <div className="mx-auto flex h-[70px] max-w-[1440px] items-center gap-6 px-4 sm:px-6 lg:px-8">
                <Link href="/" className="text-2xl font-black tracking-[-0.08em]">VANTA<span className="text-[#7da900]">✦</span></Link>
                <nav className="hidden flex-1 items-center justify-center gap-7 lg:flex">
                  {categories.map((category) => <Link key={category} href={`/catalogo?q=${encodeURIComponent(category)}`} className={`text-sm font-semibold hover:text-neutral-500 ${category === "Ofertas" ? "text-red-600" : ""}`}>{category}</Link>)}
                </nav>
                <div className="ml-auto flex items-center gap-1 sm:gap-3">
                  <Link href="/catalogo" aria-label="Buscar" className="icon-link"><SearchIcon /></Link>
                  <Link href="/login" aria-label="Mi cuenta" className="icon-link"><svg viewBox="0 0 24 24" className="size-5 fill-none stroke-current stroke-2"><circle cx="12" cy="8" r="4"/><path d="M4.5 21a7.5 7.5 0 0 1 15 0"/></svg></Link>
                  <Link href="/carrito" aria-label="Carrito" className="icon-link relative"><svg viewBox="0 0 24 24" className="size-5 fill-none stroke-current stroke-2"><path d="M5 8h14l-1 13H6L5 8Z"/><path d="M9 10V6a3 3 0 0 1 6 0v4"/></svg><span className="absolute right-0 top-0 grid size-4 place-items-center rounded-full bg-[#d7ff3f] text-[9px] font-black">0</span></Link>
                  <button className="icon-link lg:hidden" aria-label="Abrir menú"><svg viewBox="0 0 24 24" className="size-5 stroke-current stroke-2"><path d="M3 7h18M3 12h18M3 17h18"/></svg></button>
                </div>
              </div>
            </header>
            {children}
            <footer className="bg-black px-6 py-14 text-white">
              <div className="mx-auto grid max-w-[1440px] gap-10 md:grid-cols-4"><div className="md:col-span-2"><p className="text-3xl font-black tracking-[-0.08em]">VANTA<span className="text-[#d7ff3f]">✦</span></p><p className="mt-4 max-w-sm text-sm leading-6 text-neutral-400">Productos elegidos para moverte, crear y disfrutar más cada día.</p></div><div><p className="text-xs font-bold uppercase tracking-widest">Ayuda</p><div className="mt-4 grid gap-3 text-sm text-neutral-400"><Link href="/ordenes">Seguí tu pedido</Link><Link href="/carrito">Envíos y devoluciones</Link><Link href="/login">Mi cuenta</Link></div></div><div><p className="text-xs font-bold uppercase tracking-widest">Tienda</p><div className="mt-4 grid gap-3 text-sm text-neutral-400"><Link href="/catalogo">Catálogo</Link><Link href="/catalogo?q=novedades">Novedades</Link><Link href="/catalogo?q=ofertas">Ofertas</Link></div></div></div>
              <div className="mx-auto mt-12 flex max-w-[1440px] flex-col gap-3 border-t border-white/15 pt-6 text-xs text-neutral-500 sm:flex-row sm:justify-between"><p>© 2026 VANTA. Todos los derechos reservados.</p><p>Argentina · Español</p></div>
            </footer>
          </div>
        </CartProvider></AuthProvider></QueryProvider>
      </body>
    </html>
  );
}
