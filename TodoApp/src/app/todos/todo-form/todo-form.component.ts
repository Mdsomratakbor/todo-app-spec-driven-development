import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TodoService } from '../todo.service';
import { CategoryService } from '../../categories/category.service';
import { Todo } from '../todo.models';
import { Category } from '../../categories/category.models';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-todo-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule
  ],
  templateUrl: './todo-form.component.html',
  styleUrls: ['./todo-form.component.scss']
})
export class TodoFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private todoService = inject(TodoService);
  private categoryService = inject(CategoryService);
  private dialogRef = inject(MatDialogRef<TodoFormComponent>);
  private snackBar = inject(MatSnackBar);
  readonly todo: Todo | null = inject(MAT_DIALOG_DATA);

  isEdit = !!this.todo;
  categories: Category[] = [];
  loading = false;
  serverError = '';

  form = this.fb.nonNullable.group({
    title: [
      this.todo?.title ?? '',
      [Validators.required, Validators.maxLength(200)]
    ],
    description: [this.todo?.description ?? ''],
    dueDate: [this.todo?.dueDate ?? null as string | null],
    categoryId: [this.todo?.categoryId ?? ''],
    priority: [this.todo?.priority ?? 'medium']
  });

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(cats => this.categories = cats);
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    this.serverError = '';
    const raw = this.form.getRawValue();

    const request = {
      title: raw.title,
      description: raw.description || null,
      dueDate: raw.dueDate || null,
      categoryId: raw.categoryId || null,
      priority: raw.priority || 'medium'
    };

    const obs = this.isEdit
      ? this.todoService.update(this.todo!.id, request)
      : this.todoService.create(request);

    obs.subscribe({
      next: (result) => {
        this.snackBar.open(
          this.isEdit ? 'Todo updated' : 'Todo created',
          'Close', { duration: 3000 }
        );
        this.dialogRef.close(result);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 400 && err.error?.errors) {
          this.serverError = Object.values(err.error.errors).flat().join('. ');
        } else {
          this.snackBar.open('Failed to save todo', 'Close', { duration: 3000 });
        }
      }
    });
  }
}
