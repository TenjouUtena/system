export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  userId: string;
  email: string;
  displayName: string;
  roles: string[];
}

export interface RefreshTokenRequest {
  userId: string;
  refreshToken: string;
}

export interface User {
  userId: string;
  email: string;
  displayName: string;
  roles: string[];
}

