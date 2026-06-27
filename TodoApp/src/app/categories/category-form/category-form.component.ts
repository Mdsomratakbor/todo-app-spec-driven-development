import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CategoryService } from '../category.service';
import { Category } from '../category.models';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  templateUrl: './category-form.component.html',
  styleUrls: ['./category-form.component.scss']
})
export class CategoryFormComponent {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private dialogRef = inject(MatDialogRef<CategoryFormComponent>);
  private snackBar = inject(MatSnackBar);
  readonly category: Category | null = inject(MAT_DIALOG_DATA);

  isEdit = !!this.category;

  form = this.fb.nonNullable.group({
    name: [
      this.category?.name ?? '',
      [Validators.required, Validators.maxLength(50)]
    ]
  });

  loading = false;
  serverError = '';

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    this.serverError = '';
    const name = this.form.getRawValue().name;

    const request = this.isEdit
      ? this.categoryService.update(this.category!.id, name)
      : this.categoryService.create(name);

    request.subscribe({
      next: (result) => {
        this.snackBar.open(
          this.isEdit ? 'Category updated' : 'Category created',
          'Close', { duration: 3000 }
        );
        this.dialogRef.close(result);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 409) {
          this.serverError = 'A category with this name already exists';
        } else if (err.status === 400 && err.error?.errors) {
          const errors = err.error.errors;
          if (errors.name) {
            this.serverError = errors.name.join(', ');
          } else {
            this.serverError = 'Validation failed';
          }
        } else {
          this.snackBar.open('Failed to save category', 'Close', { duration: 3000 });
        }
      }
    });
  }
}
