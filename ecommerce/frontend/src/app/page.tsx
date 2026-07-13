import Link from "next/link";

const stats = [
  { value: "JWT", label: "Auth segura" },
  { value: "Docker", label: "Deploy reproducible" },
  { value: "Metrics", label: "Observabilidad" },
];

const features = [
  {
    title: "Experiencia de compra",
    description: "Catálogo con filtros, carrito persistente y checkout con validaciones de stock.",
    icon: "🛍️",
  },
  {
    title: "Backend profesional",
    description: "API REST con ASP.NET Core, Entity Framework, PostgreSQL, JWT y rate limiting.",
    icon: "⚙️",
  },
  {
    title: "Listo para demo",
    description: "Docker Compose, health checks, métricas Prometheus y documentación operativa.",
    icon: "🚀",
  },
];

export default function HomePage() {
  return (
    <main className="overflow-hidden">
      <section className="relative mx-auto grid min-h-[calc(100vh-73px)] w-full max-w-7xl items-center gap-12 px-6 py-16 lg:grid-cols-[1.05fr_0.95fr] lg:py-24">
        <div className="absolute left-1/2 top-12 -z-10 size-80 -translate-x-1/2 rounded-full bg-orange-200/30 blur-3xl" />

        <div className="space-y-8">
          <div className="inline-flex items-center gap-2 rounded-full border border-orange-200 bg-white/75 px-4 py-2 text-sm font-semibold text-orange-700 shadow-sm backdrop-blur">
            <span className="size-2 rounded-full bg-emerald-500" />
            Full-stack ecommerce portfolio
          </div>

          <div className="space-y-5">
            <h1 className="max-w-4xl text-5xl font-black tracking-tight text-slate-950 sm:text-6xl lg:text-7xl">
              Una tienda online moderna, elegante y lista para presentar.
            </h1>

            <p className="max-w-2xl text-lg leading-8 text-slate-600">
              Proyecto ecommerce con catálogo, carrito, checkout, autenticación JWT, panel admin y observabilidad;
              construido con Next.js, ASP.NET Core y PostgreSQL.
            </p>
          </div>

          <div className="flex flex-col gap-3 sm:flex-row">
            <Link
              href="/catalogo"
              className="rounded-full bg-slate-950 px-6 py-4 text-center font-semibold text-white shadow-xl shadow-slate-300 hover:bg-slate-800"
            >
              Explorar catálogo
            </Link>

            <Link
              href="/admin"
              className="rounded-full border border-slate-300 bg-white/80 px-6 py-4 text-center font-semibold text-slate-900 shadow-sm backdrop-blur hover:border-orange-300 hover:text-orange-700"
            >
              Ver panel admin
            </Link>
          </div>

          <dl className="grid max-w-2xl grid-cols-3 gap-3 pt-4">
            {stats.map((stat) => (
              <div
                key={stat.label}
                className="rounded-3xl border border-white/70 bg-white/70 p-4 shadow-sm backdrop-blur"
              >
                <dt className="text-xl font-black text-slate-950">{stat.value}</dt>
                <dd className="mt-1 text-sm text-slate-500">{stat.label}</dd>
              </div>
            ))}
          </dl>
        </div>

        <div className="relative">
          <div className="absolute -inset-6 -z-10 rounded-[3rem] bg-gradient-to-br from-orange-300/50 via-white/60 to-sky-300/50 blur-2xl" />

          <div className="rounded-[2rem] border border-white/70 bg-white/85 p-5 shadow-2xl shadow-slate-300/60 backdrop-blur">
            <div className="overflow-hidden rounded-[1.5rem] bg-slate-950 text-white">
              <div className="flex items-center justify-between border-b border-white/10 px-5 py-4">
                <div>
                  <p className="text-sm text-slate-400">Dashboard</p>
                  <h2 className="text-lg font-bold">Resumen de tienda</h2>
                </div>

                <span className="rounded-full bg-emerald-400/15 px-3 py-1 text-sm font-semibold text-emerald-300">
                  Online
                </span>
              </div>

              <div className="grid gap-4 p-5 sm:grid-cols-2">
                <div className="rounded-2xl bg-white/10 p-4">
                  <p className="text-sm text-slate-400">Ventas demo</p>
                  <p className="mt-2 text-3xl font-black">$12.8k</p>
                  <p className="mt-3 text-sm text-emerald-300">+18% esta semana</p>
                </div>

                <div className="rounded-2xl bg-orange-400 p-4 text-slate-950">
                  <p className="text-sm font-semibold text-orange-950/70">Órdenes</p>
                  <p className="mt-2 text-3xl font-black">248</p>
                  <p className="mt-3 text-sm font-semibold">Checkout validado</p>
                </div>

                <div className="rounded-2xl bg-white p-4 text-slate-950 sm:col-span-2">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-slate-500">Producto destacado</p>
                      <h3 className="mt-1 text-xl font-black">Zapatillas Pro Runner</h3>
                    </div>

                    <p className="text-2xl font-black">$120</p>
                  </div>

                  <div className="mt-5 h-3 overflow-hidden rounded-full bg-slate-100">
                    <div className="h-full w-3/4 rounded-full bg-gradient-to-r from-orange-400 to-sky-400" />
                  </div>

                  <p className="mt-3 text-sm text-slate-500">Stock, carrito y orden conectados al backend.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="mx-auto w-full max-w-7xl px-6 pb-20">
        <div className="grid gap-5 md:grid-cols-3">
          {features.map((feature) => (
            <article
              key={feature.title}
              className="rounded-[2rem] border border-white/70 bg-white/80 p-6 shadow-xl shadow-slate-200/70 backdrop-blur"
            >
              <div className="mb-5 grid size-12 place-items-center rounded-2xl bg-orange-100 text-2xl">
                {feature.icon}
              </div>

              <h2 className="text-xl font-black text-slate-950">{feature.title}</h2>
              <p className="mt-3 leading-7 text-slate-600">{feature.description}</p>
            </article>
          ))}
        </div>
      </section>
    </main>
  );
}