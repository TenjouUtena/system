'use client';

import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { authApi } from '@/lib/api/auth';
import type { User, AuthResponse } from '@/lib/types/auth';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadUserFromStorage = useCallback(() => {
    if (typeof window === 'undefined') return;

    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');
    const email = localStorage.getItem('email');
    const displayName = localStorage.getItem('displayName');
    const roles = localStorage.getItem('roles');

    if (token && userId && email && displayName) {
      setUser({
        userId,
        email,
        displayName,
        roles: roles ? JSON.parse(roles) : [],
      });
    }

    setIsLoading(false);
  }, []);

  useEffect(() => {
    loadUserFromStorage();
  }, [loadUserFromStorage]);

  const login = async (email: string, password: string) => {
    const response: AuthResponse = await authApi.login({ email, password });
    
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('userId', response.userId);
      localStorage.setItem('email', response.email);
      localStorage.setItem('displayName', response.displayName);
      localStorage.setItem('roles', JSON.stringify(response.roles));
    }

    setUser({
      userId: response.userId,
      email: response.email,
      displayName: response.displayName,
      roles: response.roles,
    });
  };

  const register = async (email: string, password: string, displayName: string) => {
    const response: AuthResponse = await authApi.register({ email, password, displayName });
    
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('userId', response.userId);
      localStorage.setItem('email', response.email);
      localStorage.setItem('displayName', response.displayName);
      localStorage.setItem('roles', JSON.stringify(response.roles));
    }

    setUser({
      userId: response.userId,
      email: response.email,
      displayName: response.displayName,
      roles: response.roles,
    });
  };

  const logout = async () => {
    await authApi.logout();
    
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userId');
      localStorage.removeItem('email');
      localStorage.removeItem('displayName');
      localStorage.removeItem('roles');
    }

    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

