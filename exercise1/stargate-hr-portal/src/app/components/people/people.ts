import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTable } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { StargateApiService } from '../../services/stargate-api.service';
import { Person } from '../../models/person.model';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatTableModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatPaginatorModule
  ],
  templateUrl: './people.html',
  styleUrl: './people.scss'
})
export class People implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'isAstronaut', 'currentRank', 'status', 'actions'];
  people: Person[] = [];
  filteredPeople: Person[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  newPersonName = '';

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatTable) table!: MatTable<Person>;

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
    
    this.apiService.getAllPeople().subscribe({
      next: (data) => {
        this.people = data;
        this.filteredPeople = [...this.people];
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load people. Please try again.';
        this.loading = false;
        console.error('Error loading people:', err);
      }
    });
  }

  applyFilter(): void {
    const filterValue = this.searchQuery.toLowerCase();
    this.filteredPeople = this.people.filter(person => 
      person.name.toLowerCase().includes(filterValue)
    );
    
    if (this.table) {
      this.table.renderRows();
    }
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.filteredPeople = [...this.people];
    if (this.table) {
      this.table.renderRows();
    }
  }

  addPerson(): void {
    if (!this.newPersonName.trim()) {
      this.snackBar.open('Please enter a name', 'Dismiss', { duration: 3000 });
      return;
    }

    this.loading = true;
    this.apiService.createPerson({ name: this.newPersonName }).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Person added successfully', 'Dismiss', { duration: 3000 });
          this.newPersonName = '';
          this.loadPeople();
        } else {
          this.snackBar.open(`Failed to add person: ${response.message}`, 'Dismiss', { duration: 5000 });
          this.loading = false;
        }
      },
      error: (err) => {
        this.snackBar.open('An error occurred while adding the person', 'Dismiss', { duration: 5000 });
        console.error('Error adding person:', err);
        this.loading = false;
      }
    });
  }

  getPersonStatus(person: Person): string {
    if (!person.currentRank && !person.currentDutyTitle) {
      return 'Not an astronaut';
    }
    
    return person.careerEndDate ? 'Retired' : 'Active';
  }

  getCurrentRank(person: Person): string {
    return person.currentRank || 'N/A';
  }

  isAstronaut(person: Person): boolean {
    return !!(person.currentRank || person.currentDutyTitle);
  }
}
