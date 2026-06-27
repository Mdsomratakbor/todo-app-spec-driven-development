import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ApiResponse, AuthResponse, ApiConfig } from '../shared/models/api-response';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  login(username: string, password: string): Observable<ApiResponse<AuthResponse>> {
    console.log(`AuthService: login attempt for ${username}`);
    return this.http.post<ApiResponse<AuthResponse>>(`${ApiConfig.baseUrl}/auth/login`, { username, password })
      .pipe(tap(res => this.handleAuth(res)));
  }

  register(username: string, password: string): Observable<ApiResponse<AuthResponse>> {
    console.log(`AuthService: register attempt for ${username}`);
    return this.http.post<ApiResponse<AuthResponse>>(`${ApiConfig.baseUrl}/auth/register`, { username, password })
      .pipe(tap(res => this.handleAuth(res)));
  }

  private handleAuth(res: ApiResponse<AuthResponse>): void {
    if (res.success && res.data) {
      localStorage.setItem('token', res.data.token);
      localStorage.setItem('username', res.data.username);
    }
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
  }
}
