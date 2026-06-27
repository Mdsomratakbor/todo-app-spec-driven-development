export interface Todo {
  id: string;
  title: string;
  description: string | null;
  isCompleted: boolean;
  dueDate: string | null;
  categoryId: string | null;
  categoryName: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TodoFilter {
  isCompleted?: boolean;
  categoryId?: string;
  dueBefore?: string;
  dueAfter?: string;
  search?: string;
}

export interface TodoRequest {
  title: string;
  description?: string | null;
  dueDate?: string | null;
  categoryId?: string | null;
}

export interface TodoUpdateRequest {
  title?: string;
  description?: string | null;
  dueDate?: string | null;
  isCompleted?: boolean;
  categoryId?: string | null;
}
