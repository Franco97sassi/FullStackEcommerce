"use client";

import Link from "next/link";
import { FormEvent, Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { login } from "@/modules/auth/services/auth-service";
import { useAuth } from "@/app/auth/context/auth-context";

export default function LoginPage() {
  return (
    <Suspense fallback={<main className="mx-auto min-h-screen w-full max-w-md px-6 py-10"><p>Cargando...</p></main>}>
      <LoginContent />
    </Suspense>
  );
}

function LoginContent() {
  const { setSession, isAuthenticated } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();

  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (isSubmitting) return;

    const formData = new FormData(event.currentTarget);
    const email = String(formData.get("email") ?? "").trim();
    const password = String(formData.get("password") ?? "");

    try {
      setIsSubmitting(true);
      setError(null);

      const response = await login(email, password);
      setSession({
        token: response.accessToken,
        expiresAtUtc: response.expiresAtUtc,
        user: response.user,
      });

      const next = searchParams.get("next") || "/catalogo";
      router.replace(next);
    } catch {
      setError("No se pudo iniciar sesión. Verificá tus credenciales.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isAuthenticated) {
    router.replace("/catalogo");
    return null;
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-md space-y-6 px-6 py-10">
      <h1 className="text-3xl font-bold">Iniciar sesión</h1>

      <form className="space-y-4 rounded-xl border p-4" onSubmit={onSubmit}>
        <div className="space-y-1">
          <label htmlFor="email" className="text-sm font-medium">Email</label>
          <input id="email" name="email" type="email" required className="w-full rounded-md border px-3 py-2" />
        </div>

        <div className="space-y-1">
          <label htmlFor="password" className="text-sm font-medium">Contraseña</label>
          <input id="password" name="password" type="password" required className="w-full rounded-md border px-3 py-2" />
        </div>

        {error && <p className="text-sm text-red-600">{error}</p>}

        <button type="submit" disabled={isSubmitting} className="w-full rounded-md bg-black px-4 py-2 text-white disabled:opacity-60">
          {isSubmitting ? "Ingresando..." : "Ingresar"}
        </button>
      </form>

      <p className="text-sm text-gray-600">
        ¿No tenés cuenta? <Link href="/registro" className="text-blue-600 underline">Crear cuenta</Link>
      </p>
    </main>
  );
}
