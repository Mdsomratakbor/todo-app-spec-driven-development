import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { TodoService } from '../todo.service';
import { CategoryService } from '../../categories/category.service';
import { Todo, TodoFilter } from '../todo.models';
import { Category } from '../../categories/category.models';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DatePipe,
    MatToolbarModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatDialogModule,
    MatButtonToggleModule
  ],
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.scss']
})
export class TodoListComponent implements OnInit, OnDestroy {
  private todoService = inject(TodoService);
  private categoryService = inject(CategoryService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  todos: Todo[] = [];
  categories: Category[] = [];
  displayedColumns = ['select', 'title', 'category', 'dueDate', 'createdAt', 'actions'];
  loading = false;

  searchTerm = '';
  statusFilter: '' | 'pending' | 'completed' = '';
  categoryFilter = '';
  dueAfter: string | null = null;
  dueBefore: string | null = null;

  private searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.loadCategories();
    this.loadTodos();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => this.loadTodos());
  }

  ngOnDestroy(): void {
    this.searchSubject.complete();
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe(cats => this.categories = cats);
  }

  loadTodos(): void {
    this.loading = true;
    const filter: TodoFilter = {};

    if (this.statusFilter === 'completed') filter.isCompleted = true;
    else if (this.statusFilter === 'pending') filter.isCompleted = false;

    if (this.categoryFilter) filter.categoryId = this.categoryFilter;
    if (this.dueAfter) filter.dueAfter = this.dueAfter;
    if (this.dueBefore) filter.dueBefore = this.dueBefore;
    if (this.searchTerm.trim()) filter.search = this.searchTerm.trim();

    this.todoService.getAll(filter)
      .pipe(finalize(() => this.loading = false))
      .subscribe(todos => this.todos = todos);
  }

  isOverdue(todo: Todo): boolean {
    if (todo.isCompleted || !todo.dueDate) return false;
    return new Date(todo.dueDate) < new Date();
  }

  toggleCompletion(todo: Todo): void {
    this.todoService.update(todo.id, { isCompleted: !todo.isCompleted }).subscribe({
      next: () => this.loadTodos(),
      error: () => this.snackBar.open('Failed to update todo', 'Close', { duration: 3000 })
    });
  }

  openAddDialog(): void {
    import('../todo-form/todo-form.component').then(m => {
      const ref = this.dialog.open(m.TodoFormComponent, {
        width: '500px',
        data: null
      });
      ref.afterClosed().subscribe(result => {
        if (result) this.loadTodos();
      });
    });
  }

  openEditDialog(todo: Todo): void {
    import('../todo-form/todo-form.component').then(m => {
      const ref = this.dialog.open(m.TodoFormComponent, {
        width: '500px',
        data: todo
      });
      ref.afterClosed().subscribe(result => {
        if (result) this.loadTodos();
      });
    });
  }

  deleteTodo(todo: Todo): void {
    import('../../categories/confirm-dialog/confirm-dialog.component').then(m => {
      const ref = this.dialog.open(m.ConfirmDialogComponent, {
        width: '400px',
        data: {
          title: 'Delete Todo',
          message: `Delete todo "${todo.title}"? This action is permanent.`
        }
      });
      ref.afterClosed().subscribe(confirmed => {
        if (!confirmed) return;
        this.todoService.delete(todo.id).subscribe({
          next: () => {
            this.snackBar.open('Todo deleted', 'Close', { duration: 3000 });
            this.loadTodos();
          },
          error: (err: HttpErrorResponse) => {
            if (err.status === 404) {
              this.snackBar.open('Todo not found', 'Close', { duration: 3000 });
            } else {
              this.snackBar.open('Failed to delete todo', 'Close', { duration: 3000 });
            }
          }
        });
      });
    });
  }
}
