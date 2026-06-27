import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CategoryService } from '../category.service';
import { Category } from '../category.models';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatSnackBarModule,
    DatePipe
  ],
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss']
})
export class CategoryListComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  categories: Category[] = [];
  displayedColumns = ['name', 'todoCount', 'createdAt', 'actions'];
  loading = false;

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getAll()
      .pipe(finalize(() => this.loading = false))
      .subscribe(cats => this.categories = cats);
  }

  openAddDialog(): void {
    import('../category-form/category-form.component').then(m => {
      const ref = this.dialog.open(m.CategoryFormComponent, {
        width: '400px',
        data: null
      });
      ref.afterClosed().subscribe(result => {
        if (result) this.loadCategories();
      });
    });
  }

  openEditDialog(cat: Category): void {
    import('../category-form/category-form.component').then(m => {
      const ref = this.dialog.open(m.CategoryFormComponent, {
        width: '400px',
        data: cat
      });
      ref.afterClosed().subscribe(result => {
        if (result) this.loadCategories();
      });
    });
  }

  deleteCategory(cat: Category): void {
    const message = cat.todoCount > 0
      ? `This category has ${cat.todoCount} todo(s). Deleting it will remove the category from those todos.`
      : `Delete category "${cat.name}"?`;

    import('../confirm-dialog/confirm-dialog.component').then(m => {
      const ref = this.dialog.open(m.ConfirmDialogComponent, {
        width: '400px',
        data: { title: 'Delete Category', message }
      });

      ref.afterClosed().subscribe(confirmed => {
        if (!confirmed) return;

        this.categoryService.delete(cat.id).subscribe({
          next: () => {
            this.snackBar.open('Category deleted', 'Close', { duration: 3000 });
            this.loadCategories();
          },
          error: (err: HttpErrorResponse) => {
            if (err.status === 409) {
              this.snackBar.open('Cannot delete: category still has todos', 'Close', { duration: 5000 });
            } else {
              this.snackBar.open('Failed to delete category', 'Close', { duration: 3000 });
            }
          }
        });
      });
    });
  }
}
