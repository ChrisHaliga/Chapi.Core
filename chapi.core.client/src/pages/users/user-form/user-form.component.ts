// shared/user-form.component.ts
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { User } from '../../../models/user';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgSelectModule]
})
export class UserFormComponent implements OnInit, OnChanges {
  @Input() user: User = new User();
  @Input() isEditMode: boolean = false;
  @Output() save = new EventEmitter<User>();
  @Output() cancel = new EventEmitter<void>();

  userForm!: FormGroup; // Using ! to assure TypeScript this will be initialized before use
  submitted = false;
  previewUrl: string | ArrayBuffer | null = null;
  profilePictureValid = true;

  // Sample data for dropdowns - these could be fetched from an API in a real application
  organizations: string[] = ['Acme Corp', 'Globex', 'Initech', 'Umbrella Corp', 'Stark Industries'];

  availableApplications = [
    { id: 'app1', name: 'Dashboard' },
    { id: 'app2', name: 'Analytics' },
    { id: 'app3', name: 'Reporting' },
    { id: 'app4', name: 'Admin Panel' },
    { id: 'app5', name: 'Messaging' }
  ];

  availableGroups = [
    { id: 'group1', name: 'Administrators' },
    { id: 'group2', name: 'Managers' },
    { id: 'group3', name: 'Developers' },
    { id: 'group4', name: 'Marketing' },
    { id: 'group5', name: 'Support' }
  ];

  constructor(private formBuilder: FormBuilder) {
    // Form will be initialized in ngOnInit, not here
  }

  initForm(): FormGroup {
    console.log('Initializing form');
    const form = this.formBuilder.group({
      organization: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      name: ['', Validators.required],
      profilePicture: [''],
      applications: [[]],
      groups: [[]]
    });
    console.log('Form initialized:', form);
    return form;
  }

  ngOnInit(): void {
    console.log('UserFormComponent initialized');
    console.log('Initial user input:', this.user);
    this.userForm = this.initForm();
    this.populateForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('ngOnChanges called, changes:', changes);
    if (changes['user'] && this.userForm) {
      console.log('User changed:', changes['user'].currentValue);
      this.populateForm();
    }
  }

  populateForm(): void {
    console.log('Populating form with user:', this.user);
    if (this.user && this.user.email && this.userForm) {
      // Parse comma-separated values back to arrays
      const applications = this.user.applications ? this.user.applications.split(',') : [];
      const groups = this.user.groups ? this.user.groups.split(',') : [];

      console.log('Setting form values:', {
        organization: this.user.organization,
        email: this.user.email,
        name: this.user.name,
        applications: applications,
        groups: groups
      });

      this.userForm.patchValue({
        organization: this.user.organization,
        email: this.user.email,
        name: this.user.name,
        applications: applications,
        groups: groups
      });

      // Email field should be readonly in edit mode
      if (this.isEditMode) {
        console.log('Edit mode - disabling email field');
        this.userForm.get('email')?.disable();
      } else {
        this.userForm.get('email')?.enable();
      }

      this.previewUrl = this.user.profilePicture || null;

      console.log('Form after population:', this.userForm.value);
    } else {
      console.log('User data is empty or missing email, or form not initialized, skipping form population');
    }
  }

  // Convenience getter for easy access to form fields
  get f() { return this.userForm.controls; }

  onFileChange(event: any): void {
    const file = event.target.files[0];

    if (file) {
      this.profilePictureValid = file.type.startsWith('image/');

      if (this.profilePictureValid) {
        // Generate preview
        const reader = new FileReader();
        reader.onload = () => {
          this.previewUrl = reader.result;
        };
        reader.readAsDataURL(file);
      } else {
        this.previewUrl = null;
      }
    }
  }

  onSubmit(): void {
    this.submitted = true;

    // Stop here if form is invalid
    if (this.userForm.invalid || !this.profilePictureValid) {
      return;
    }

    // Process form data
    const formData = this.userForm.getRawValue(); // Use getRawValue to include disabled fields
    const updatedUser = new User();

    updatedUser.organization = formData.organization;
    updatedUser.email = formData.email;
    updatedUser.name = formData.name;
    updatedUser.profilePicture = this.previewUrl as string; // In real app, you'd upload the file
    updatedUser.applications = formData.applications.join(',');
    updatedUser.groups = formData.groups.join(',');

    this.save.emit(updatedUser);
  }

  resetForm(): void {
    this.cancel.emit();
  }
}
