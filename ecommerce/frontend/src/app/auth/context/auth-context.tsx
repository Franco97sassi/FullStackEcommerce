"use client";

import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { AuthUser } from "@/modules/auth/types";

type AuthState = {
  token: string;
  expiresAtUtc: string;
  user: AuthUser;
};

type AuthContextValue = {
  isAuthenticated: boolean;
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  setSession: (state: AuthState) => void;
  logout: () => void;
};

const AUTH_STORAGE_KEY = "ecommerce_auth_session";
const AUTH_SESSION_CHANGED_EVENT = "auth-session-changed";
const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<AuthState | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(AUTH_STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as AuthState;
        if (new Date(parsed.expiresAtUtc).getTime() > Date.now()) {
          setSessionState(parsed);
        } else {
          window.localStorage.removeItem(AUTH_STORAGE_KEY);
        }
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  function setSession(state: AuthState) {
    setSessionState(state);
    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(state));
    window.dispatchEvent(new Event(AUTH_SESSION_CHANGED_EVENT));  }

  function logout() {
    setSessionState(null);
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    window.dispatchEvent(new Event(AUTH_SESSION_CHANGED_EVENT));  }

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: Boolean(session?.token),
      user: session?.user ?? null,
      token: session?.token ?? null,
      isLoading,
      setSession,
      logout,
    }),
    [session, isLoading],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth debe usarse dentro de AuthProvider");
  }
  return context;
}

export const authStorageKey = AUTH_STORAGE_KEY;
