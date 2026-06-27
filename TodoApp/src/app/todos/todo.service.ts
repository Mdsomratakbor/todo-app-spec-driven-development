import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, ApiConfig } from '../shared/models/api-response';
import { Todo, TodoFilter, TodoRequest, TodoUpdateRequest } from './todo.models';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private http = inject(HttpClient);

  getAll(filter?: TodoFilter): Observable<Todo[]> {
    let params = new HttpParams();
    if (filter) {
      if (filter.isCompleted !== undefined) params = params.set('isCompleted', filter.isCompleted);
      if (filter.categoryId) params = params.set('categoryId', filter.categoryId);
      if (filter.dueBefore) params = params.set('dueBefore', filter.dueBefore);
      if (filter.dueAfter) params = params.set('dueAfter', filter.dueAfter);
      if (filter.search) params = params.set('search', filter.search);
      if (filter.priority) params = params.set('priority', filter.priority);
      if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
      if (filter.sortDirection) params = params.set('sortDirection', filter.sortDirection);
    }
    return this.http.get<ApiResponse<Todo[]>>(`${ApiConfig.baseUrl}/todos`, { params })
      .pipe(map(res => res.data ?? []));
  }

  getById(id: string): Observable<Todo> {
    return this.http.get<ApiResponse<Todo>>(`${ApiConfig.baseUrl}/todos/${id}`)
      .pipe(map(res => res.data!));
  }

  create(request: TodoRequest): Observable<Todo> {
    return this.http.post<ApiResponse<Todo>>(`${ApiConfig.baseUrl}/todos`, request)
      .pipe(map(res => res.data!));
  }

  update(id: string, request: TodoUpdateRequest): Observable<Todo> {
    return this.http.put<ApiResponse<Todo>>(`${ApiConfig.baseUrl}/todos/${id}`, request)
      .pipe(map(res => res.data!));
  }

  delete(id: string): Observable<void> {
    return this.http.delete(`${ApiConfig.baseUrl}/todos/${id}`, { responseType: 'text' })
      .pipe(map(() => void 0));
  }
}
