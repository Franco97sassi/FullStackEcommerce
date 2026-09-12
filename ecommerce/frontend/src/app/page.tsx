import Image from "next/image";
import Link from "next/link";

const categories = [
  { name: "Running", image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=800&q=85" },
  { name: "Tecnología", image: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&q=85" },
  { name: "Entrenamiento", image: "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800&q=85" },
  { name: "Accesorios", image: "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=800&q=85" },
];

const products = [
  { name: "Air Motion One", category: "Zapatillas urbanas", price: "129,90", oldPrice: "159,90", badge: "-20%", image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=900&q=90" },
  { name: "Studio Headphones", category: "Audio inalámbrico", price: "149,90", badge: "Nuevo", image: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=900&q=90" },
  { name: "Active Watch S", category: "Tecnología", price: "199,00", image: "https://images.unsplash.com/photo-1544117519-31a4b719223d?w=900&q=90" },
  { name: "Everyday Pack", category: "Accesorios", price: "58,00", image: "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=900&q=90" },
];

const Arrow = () => <span aria-hidden="true">↗</span>;

export default function HomePage() {
  return (
    <main>
      <section className="mx-auto w-full max-w-[1440px] px-4 pb-14 pt-4 sm:px-6 lg:px-8">
        <div className="hero-grid relative min-h-[650px] overflow-hidden rounded-sm bg-[#d9d6cf]">
          <Image
            src="https://images.unsplash.com/photo-1552346154-21d32810aba3?w=1800&q=90"
            alt="Zapatillas de la nueva colección urbana"
            fill
            priority
            sizes="(max-width: 1440px) 100vw, 1440px"
            className="object-cover object-[68%_center]"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-black/75 via-black/20 to-transparent" />
          <div className="relative flex min-h-[650px] max-w-2xl flex-col justify-end p-7 text-white sm:p-12 lg:p-16">
            <p className="mb-5 text-xs font-bold uppercase tracking-[0.25em]">Nueva colección · 2026</p>
            <h1 className="text-5xl font-black uppercase leading-[0.88] tracking-[-0.055em] sm:text-7xl lg:text-[92px]">
              Muévete<br />sin límites
            </h1>
            <p className="mt-6 max-w-md text-base leading-7 text-white/85 sm:text-lg">
              Diseñados para seguirte el ritmo. Descubrí básicos elevados, tecnología y rendimiento para todos los días.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link href="/catalogo" className="bg-white px-7 py-3.5 text-sm font-bold text-black hover:bg-[#d7ff3f]">
                Comprar colección
              </Link>
              <Link href="/catalogo?categoria=moda" className="border border-white px-7 py-3.5 text-sm font-bold text-white hover:bg-white hover:text-black">
                Ver novedades
              </Link>
            </div>
          </div>
          <div className="absolute bottom-6 right-6 hidden items-center gap-3 bg-white px-5 py-4 text-black sm:flex">
            <span className="text-2xl">✦</span>
            <div><p className="text-xs font-bold uppercase tracking-wider">Envío gratis</p><p className="text-xs text-neutral-500">En compras desde $100</p></div>
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-[1440px] px-4 py-14 sm:px-6 lg:px-8">
        <div className="mb-8 flex items-end justify-between">
          <div><p className="section-kicker">Explorá lo mejor</p><h2 className="section-title">Comprá por categoría</h2></div>
          <Link href="/catalogo" className="hidden border-b border-black pb-1 text-sm font-bold sm:block">Ver todo <Arrow /></Link>
        </div>
        <div className="grid grid-cols-2 gap-3 lg:grid-cols-4 lg:gap-5">
          {categories.map((category) => (
            <Link key={category.name} href={`/catalogo?q=${encodeURIComponent(category.name)}`} className="group relative aspect-[3/4] overflow-hidden bg-neutral-100">
              <Image src={category.image} alt={category.name} fill sizes="(max-width: 768px) 50vw, 25vw" className="object-cover transition duration-700 group-hover:scale-105" />
              <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 to-transparent p-5 pt-20 text-white">
                <span className="flex items-center justify-between text-lg font-bold sm:text-xl">{category.name}<Arrow /></span>
              </div>
            </Link>
          ))}
        </div>
      </section>

      <section className="bg-[#f4f4f2] py-20">
        <div className="mx-auto max-w-[1440px] px-4 sm:px-6 lg:px-8">
          <div className="mb-9 flex items-end justify-between">
            <div><p className="section-kicker">Favoritos de la semana</p><h2 className="section-title">Lo más buscado</h2></div>
            <div className="hidden gap-2 sm:flex"><button className="product-arrow" aria-label="Productos anteriores">←</button><button className="product-arrow" aria-label="Más productos">→</button></div>
          </div>
          <div className="grid grid-cols-2 gap-x-3 gap-y-9 lg:grid-cols-4 lg:gap-x-5">
            {products.map((product) => (
              <article key={product.name} className="group">
                <Link href="/catalogo" className="relative block aspect-square overflow-hidden bg-[#e8e8e6]">
                  <Image src={product.image} alt={product.name} fill sizes="(max-width: 768px) 50vw, 25vw" className="object-cover transition duration-500 group-hover:scale-105" />
                  {product.badge && <span className="absolute left-3 top-3 bg-white px-3 py-1.5 text-[11px] font-bold uppercase">{product.badge}</span>}
                  <span className="absolute right-3 top-3 grid size-9 place-items-center rounded-full bg-white text-lg" aria-label="Agregar a favoritos">♡</span>
                  <span className="absolute inset-x-3 bottom-3 translate-y-16 bg-black py-3 text-center text-xs font-bold uppercase tracking-wider text-white transition-transform group-hover:translate-y-0">Vista rápida</span>
                </Link>
                <div className="pt-4"><p className="text-xs text-neutral-500">{product.category}</p><h3 className="mt-1 font-bold">{product.name}</h3><p className="mt-2 text-sm font-bold">${product.price} {product.oldPrice && <del className="ml-2 font-normal text-neutral-400">${product.oldPrice}</del>}</p></div>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="mx-auto grid max-w-[1440px] gap-5 px-4 py-20 sm:px-6 lg:grid-cols-2 lg:px-8">
        <div className="relative min-h-[480px] overflow-hidden bg-black">
          <Image src="https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=1200&q=90" alt="Entrenamiento de alto rendimiento" fill sizes="(max-width: 1024px) 100vw, 50vw" className="object-cover opacity-75" />
          <div className="absolute inset-0 flex flex-col justify-end p-8 text-white sm:p-12"><p className="section-kicker !text-[#d7ff3f]">Rendimiento</p><h2 className="max-w-md text-4xl font-black uppercase tracking-tight sm:text-5xl">Tu mejor versión empieza hoy.</h2><Link href="/catalogo?categoria=deportes" className="mt-6 w-fit bg-white px-6 py-3 text-sm font-bold text-black">Entrená mejor</Link></div>
        </div>
        <div className="flex min-h-[480px] flex-col justify-between bg-[#d7ff3f] p-8 sm:p-12">
          <span className="text-5xl">✦</span>
          <div><p className="section-kicker">Miembros Vanta+</p><h2 className="max-w-lg text-4xl font-black uppercase leading-none tracking-tight sm:text-6xl">Más beneficios. Cero costo.</h2><p className="mt-5 max-w-md leading-7">Acceso anticipado, envíos gratis y ofertas creadas para vos. Todo en un solo lugar.</p><Link href="/registro" className="mt-7 inline-block bg-black px-7 py-3.5 text-sm font-bold text-white">Unirme ahora</Link></div>
        </div>
      </section>

      <section className="border-t border-neutral-200 py-12">
        <div className="mx-auto grid max-w-[1440px] gap-8 px-6 sm:grid-cols-3 lg:px-8">
          {[['↗','Envíos a todo el país','Gratis desde $100'],['↺','Cambios simples','Tenés 30 días'],['◈','Pago protegido','Compra segura siempre']].map(([icon,title,text]) => <div key={title} className="flex items-center gap-4 sm:justify-center"><span className="text-2xl">{icon}</span><div><h3 className="text-sm font-bold">{title}</h3><p className="text-sm text-neutral-500">{text}</p></div></div>)}
        </div>
      </section>
    </main>
  );
}
