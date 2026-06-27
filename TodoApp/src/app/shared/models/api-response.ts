export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: Record<string, string[]> | null;
}

export interface AuthResponse {
  token: string;
  username: string;
  expiresAt: string;
}

export const ApiConfig = {
  baseUrl: 'http://localhost:5000/api/v1'
};
