import { Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import { People } from './components/people/people';
import { AstronautDuties } from './components/astronaut-duties/astronaut-duties';
import { PersonDetails } from './components/person-details/person-details';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'people', component: People },
  { path: 'person/:name', component: PersonDetails },
  { path: 'duties', component: AstronautDuties },
  { path: '**', redirectTo: '/dashboard' }
];
