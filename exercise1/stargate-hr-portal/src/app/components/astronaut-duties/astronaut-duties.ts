import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StargateApiService } from '../../services/stargate-api.service';
import { Person, AstronautDuty, UpdateAstronautDutyRequest } from '../../models/person.model';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import { Inject } from '@angular/core';

@Component({
  selector: 'app-astronaut-duties',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatExpansionModule,
    MatChipsModule,
    MatDividerModule,
    MatTooltipModule,
    MatDialogModule
  ],
  templateUrl: './astronaut-duties.html',
  styleUrl: './astronaut-duties.scss'
})
export class AstronautDuties implements OnInit {
  people: Person[] = [];
  filteredPeople: Person[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  selectedPerson: Person | null = null;
  astronautDuties: AstronautDuty[] = [];
  
  dutyForm = new FormGroup({
    name: new FormControl('', Validators.required),
    rank: new FormControl('', Validators.required),
    dutyTitle: new FormControl('', Validators.required),
    dutyStartDate: new FormControl(new Date(), Validators.required)
  });
  
  rankOptions = [
    'Trainee',
    'Lieutenant',
    'Captain',
    'Commander',
    'Major',
    'Colonel',
    'General'
  ];
  
  dutyTitleOptions = [
    'Pilot',
    'Mission Specialist',
    'Flight Engineer',
    'Commander',
    'Science Officer',
    'RETIRED'
  ];

  constructor(
    private apiService: StargateApiService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadPeople();
  }

  loadPeople(): void {
    this.loading = true;
    this.error = null;
    
    this.apiService.getAllPeople()
      .pipe(
        catchError(err => {
          this.error = 'Failed to load people. Please try again.';
          console.error('Error loading people:', err);
          return of([]);
        }),
        finalize(() => this.loading = false)
      )
      .subscribe(data => {
        this.people = data;
        this.filteredPeople = [...this.people];
      });
  }

  loadAstronautDuties(personName: string): void {
    this.loading = true;
    this.error = null;
    this.astronautDuties = [];
    
    this.apiService.getAstronautDutiesByName(personName)
      .pipe(
        catchError(err => {
          this.error = 'Failed to load astronaut duties. Please try again.';
          console.error('Error loading astronaut duties:', err);
          return of([]);
        }),
        finalize(() => this.loading = false)
      )
      .subscribe(data => {
        this.astronautDuties = data;
        // Sort duties by start date (most recent first)
        this.astronautDuties.sort((a, b) => 
          new Date(b.dutyStartDate).getTime() - new Date(a.dutyStartDate).getTime()
        );
        // Pre-populate form with the person name
        this.dutyForm.get('name')?.setValue(personName);
        
        // Set default values for rank and title if the person has existing duties
        if (data.length > 0) {
          const activeDuty = data.find(duty => !duty.dutyEndDate);
          if (activeDuty) {
            this.dutyForm.get('rank')?.setValue(activeDuty.rank);
            this.dutyForm.get('dutyTitle')?.setValue(activeDuty.dutyTitle);
          } else {
            // Use the most recent duty
            const mostRecentDuty = data[0];
            this.dutyForm.get('rank')?.setValue(mostRecentDuty.rank);
            this.dutyForm.get('dutyTitle')?.setValue(mostRecentDuty.dutyTitle);
          }
        }
      });
  }

  selectPerson(person: Person): void {
    this.selectedPerson = person;
    this.loadAstronautDuties(person.name);
  }

  applyFilter(): void {
    const filterValue = this.searchQuery.toLowerCase();
    this.filteredPeople = this.people.filter(person => 
      person.name.toLowerCase().includes(filterValue)
    );
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.filteredPeople = [...this.people];
  }

  submitDuty(): void {
    if (this.dutyForm.invalid) {
      this.snackBar.open('Please fill out all required fields', 'Dismiss', { duration: 3000 });
      return;
    }

    const formValue = this.dutyForm.value;
    
    const newDuty = {
      name: formValue.name || '',
      rank: formValue.rank || '',
      dutyTitle: formValue.dutyTitle || '',
      dutyStartDate: formValue.dutyStartDate ? new Date(formValue.dutyStartDate).toISOString() : new Date().toISOString()
    };

    this.loading = true;
    
    this.apiService.createAstronautDuty(newDuty)
      .pipe(
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open('Astronaut duty added successfully', 'Dismiss', { 
              duration: 3000,
              panelClass: ['success-snackbar']
            });
            
            // Reset form except for name
            const currentName = this.dutyForm.get('name')?.value;
            this.dutyForm.reset({
              name: currentName,
              dutyStartDate: new Date()
            });
            
            // Reload the duties
            if (this.selectedPerson) {
              this.loadAstronautDuties(this.selectedPerson.name);
            }
          } else {
            this.snackBar.open(`Failed to add duty: ${response.message}`, 'Dismiss', { 
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (err) => {
          this.snackBar.open('An error occurred while adding the duty', 'Dismiss', { 
            duration: 5000,
            panelClass: ['error-snackbar']
          });
          console.error('Error adding duty:', err);
        }
      });
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString();
  }
  
  formatDateLong(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric'
    });
  }

  resetForm(): void {
    // Reset form but keep the person name
    const currentName = this.dutyForm.get('name')?.value;
    this.dutyForm.reset({
      name: currentName,
      dutyStartDate: new Date()
    });
  }

  isDutyActive(duty: AstronautDuty): boolean {
    return !duty.dutyEndDate;
  }
  
  getDutyDuration(duty: AstronautDuty): string {
    const start = new Date(duty.dutyStartDate);
    const end = duty.dutyEndDate ? new Date(duty.dutyEndDate) : new Date();
    
    const diffMs = end.getTime() - start.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    
    if (diffDays < 30) {
      return `${diffDays} days`;
    }
    
    const diffMonths = Math.floor(diffDays / 30);
    if (diffMonths < 12) {
      return `${diffMonths} ${diffMonths === 1 ? 'month' : 'months'}`;
    }
    
    const years = Math.floor(diffMonths / 12);
    const remainingMonths = diffMonths % 12;
    return `${years} ${years === 1 ? 'year' : 'years'}${remainingMonths > 0 ? `, ${remainingMonths} ${remainingMonths === 1 ? 'month' : 'months'}` : ''}`;
  }
  
  getTotalServiceYears(): number {
    if (!this.astronautDuties || this.astronautDuties.length === 0) return 0;
    
    // Find the earliest start date among all duties
    const earliestStartDate = new Date(Math.min(
      ...this.astronautDuties.map(duty => new Date(duty.dutyStartDate).getTime())
    ));
    
    const now = new Date();
    const diffMs = now.getTime() - earliestStartDate.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    return +(diffDays / 365.25).toFixed(1);
  }
  
  getDutyColor(duty: AstronautDuty): string {
    if (!duty.dutyEndDate) {
      return 'primary'; // Active duty
    }
    
    if (duty.dutyTitle === 'RETIRED') {
      return 'warn';
    }
    
    return 'accent'; // Past duty
  }

  isAstronaut(person: Person): boolean {
    // Check if person exists in AstronautDetail table (via currentRank/currentDutyTitle from API)
    // or has any AstronautDuty records (via astronautDuties array if available)
    return !!(person.currentRank || person.currentDutyTitle || 
              person.astronautDetail || 
              (person.astronautDuties && person.astronautDuties.length > 0));
  }

  showEndDateDialog(duty: AstronautDuty): void {
    const dialogRef = this.dialog.open(EndDateDialogComponent, {
      width: '400px',
      data: { duty: duty }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.endDate) {
        this.updateDutyEndDate(duty.id, result.endDate);
      }
    });
  }

  private updateDutyEndDate(dutyId: number, endDate: Date): void {
    this.loading = true;
    
    const request: UpdateAstronautDutyRequest = {
      dutyEndDate: endDate.toISOString().split('T')[0] // Format as YYYY-MM-DD
    };

    this.apiService.updateAstronautDuty(dutyId, request)
      .pipe(
        catchError(err => {
          console.error('Error updating duty end date:', err);
          this.snackBar.open('Failed to update duty end date', 'Close', { duration: 3000 });
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe((response: any) => {
        if (response && response.success) {
          this.snackBar.open(
            response.retirementDutyCreated 
              ? 'Duty ended and retirement created successfully!' 
              : 'Duty end date updated successfully!', 
            'Close', 
            { duration: 5000 }
          );
          
          // Reload the duties to show the updated information
          if (this.selectedPerson) {
            this.loadAstronautDuties(this.selectedPerson.name);
          }
        }
      });
  }
}

// End Date Dialog Component
@Component({
  selector: 'app-end-date-dialog',
  template: `
    <h2 mat-dialog-title>Add End Date</h2>
    <mat-dialog-content>
      <p>Add an end date for this duty assignment:</p>
      <p><strong>{{ data.duty.dutyTitle }}</strong> - {{ data.duty.rank }}</p>
      <p>Started: {{ formatDateLong(data.duty.dutyStartDate) }}</p>
      
      <mat-form-field appearance="outline" style="width: 100%; margin-top: 16px;">
        <mat-label>End Date</mat-label>
        <input matInput [matDatepicker]="picker" [(ngModel)]="endDate" [min]="minDate">
        <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
        <mat-datepicker #picker></mat-datepicker>
        <mat-hint>Date when this duty assignment ends</mat-hint>
      </mat-form-field>
      
      <mat-dialog-actions>
        <button mat-button (click)="onCancel()">Cancel</button>
        <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="!endDate">
          Add End Date
        </button>
      </mat-dialog-actions>
    </mat-dialog-content>
  `,
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule
  ]
})
export class EndDateDialogComponent {
  endDate: Date | null = null;
  minDate: Date;

  constructor(
    public dialogRef: MatDialogRef<EndDateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { duty: AstronautDuty }
  ) {
    // Set minimum date to the duty start date
    this.minDate = new Date(data.duty.dutyStartDate);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.endDate) {
      this.dialogRef.close({ endDate: this.endDate });
    }
  }

  formatDateLong(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
