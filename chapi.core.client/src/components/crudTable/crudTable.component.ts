import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-crud-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crudTable.component.html',
  styleUrls: ['./crudTable.component.css']
})

export class CrudTable<T> {
  @Input() items$!: Observable<T[]>;
  @Input() fields!: { [key: string]: (item: T) => string };
  @Input() onEdit!: (item: T) => void;
  @Input() onView!: (item: T) => void;
  @Input() onCreate!: () => void;
  @Input() onDelete!: (item: T) => void;
  @Input() onRefresh!: () => void;
  @Input() getDisplayName!: (item: T) => string;


  public selectedItems: Map<string, boolean> = new Map();
  public allSelected: boolean = false;
  public hasSelectedItems: boolean = false;
  public userConfirmationText: string = "";
  public confirmationText: string = "";

  toggleItem(key: string) {
    this.selectedItems.set(key, !this.selectedItems.get(key));
    this.updateAllSelected();
  }

  toggleAll(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    this.allSelected = checked;
    this.items$.subscribe(items => {
      items.forEach(item => this.selectedItems.set(this.getItemKey(item), checked));
    });
  }

  selectedItemsCount(): number {
    let count = 0;
    this.selectedItems.forEach((value: boolean, key: string) => count += value ? 1 : 0);
    return count;
  }

  updateConfirmationText(): void {
    if (this.selectedItemsCount() > 1) {
      this.confirmationText = "I have verified the items marked for deletion";
      return;
    }

    this.items$.subscribe(items => {
      for(let item of items){
        if (this.selectedItems.get(this.getItemKey(item))) {
          this.confirmationText = this.getDisplayName(item);
          break;
        }
      }
    });
  }

  updateAllSelected() {
    this.items$.subscribe(items => {
      this.allSelected = items.every(item => this.selectedItems.get(this.getItemKey(item)));
    });

    this.hasSelectedItems = this.selectedItemsCount() > 0
  }

  preventRowClick(e: Event) {
    e.stopPropagation();
  }

  getItemKey(item: T): string {
    return JSON.stringify(item); // Adjust key selection logic as needed
  }

  getFieldKeys(): string[] {
    return Object.keys(this.fields);
  }

  isDeleteConfirmed() {
    return this.userConfirmationText != this.confirmationText
  }

  confirmDelete(): void {
    this.items$.subscribe(items => {
      items.forEach(item => {
        if (this.selectedItems.get(this.getItemKey(item))) {
          this.onDelete(item);
        }
      });
      this.onRefresh();
    });
    this.resetDelete()
  }

  resetDelete(): void {
    this.userConfirmationText = "";
    this.confirmationText= "";
  }
}
