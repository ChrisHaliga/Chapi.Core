import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-delete-confirmation-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delete-confirmation-modal.component.html',
  styleUrls: ['./delete-confirmation-modal.component.scss']
})
export class DeleteConfirmationModalComponent {
  @Input() modalId: string = 'deleteConfirmationModal';
  @Input() confirmationText: string = '';
  @Input() title: string = 'Confirm Deletion';
  @Input() message: string = 'Are you sure you want to delete this item?';
  @Output() confirmDelete = new EventEmitter<void>();

  userConfirmationText: string = '';

  isDeleteConfirmed(): boolean {
    return this.userConfirmationText !== this.confirmationText;
  }

  confirm(): void {
    this.confirmDelete.emit();
    this.resetConfirmation();
  }

  resetConfirmation(): void {
    this.userConfirmationText = '';
  }
}
