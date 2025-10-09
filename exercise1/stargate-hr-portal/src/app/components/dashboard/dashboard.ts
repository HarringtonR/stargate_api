import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { StargateApiService } from '../../services/stargate-api.service';
import { Person } from '../../models/person.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  people: Person[] = [];
  loading = false;
  error: string | null = null;
  currentDate = new Date();

  constructor(private apiService: StargateApiService) {}

  ngOnInit(): void {
    this.loadPeople();
  }

  loadPeople(): void {
    this.loading = true;
    this.error = null;
    
    this.apiService.getAllPeople().subscribe({
      next: (data) => {
        this.people = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load people. Please try again.';
        this.loading = false;
        console.error('Error loading people:', err);
      }
    });
  }

  getAstronautCount(): number {
    return this.people.filter(p => p.astronautDetail).length;
  }

  getActiveAstronautCount(): number {
    return this.people.filter(p => p.astronautDetail && !p.astronautDetail.careerEndDate).length;
  }

  getRetiredAstronautCount(): number {
    return this.people.filter(p => p.astronautDetail && p.astronautDetail.careerEndDate).length;
  }
}
