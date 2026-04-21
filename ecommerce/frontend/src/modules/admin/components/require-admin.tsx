"use client";

import { useAuth } from "@/app/auth/context/auth-context";

export function RequireAdmin({ children }: { children: React.ReactNode }) {
  const { isLoading, isAuthenticated, user } = useAuth();

  if (isLoading) {
    return <p>Cargando permisos...</p>;
  }

  if (!isAuthenticated || user?.role !== "Admin") {
    return <p className="text-red-600">No tenés permisos de administrador.</p>;
  }

  return <>{children}</>;
}
