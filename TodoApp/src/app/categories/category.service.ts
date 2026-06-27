import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, ApiConfig } from '../shared/models/api-response';
import { Category } from './category.models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(HttpClient);

  getAll(): Observable<Category[]> {
    return this.http.get<ApiResponse<Category[]>>(`${ApiConfig.baseUrl}/categories`)
      .pipe(map(res => res.data ?? []));
  }

  create(name: string): Observable<Category> {
    return this.http.post<ApiResponse<Category>>(`${ApiConfig.baseUrl}/categories`, { name })
      .pipe(map(res => res.data!));
  }

  update(id: string, name: string): Observable<Category> {
    return this.http.put<ApiResponse<Category>>(`${ApiConfig.baseUrl}/categories/${id}`, { name })
      .pipe(map(res => res.data!));
  }

  delete(id: string): Observable<void> {
    return this.http.delete(`${ApiConfig.baseUrl}/categories/${id}`, { responseType: 'text' })
      .pipe(map(() => void 0));
  }
}
