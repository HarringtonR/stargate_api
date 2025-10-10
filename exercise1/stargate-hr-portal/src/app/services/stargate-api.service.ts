import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { 
  Person, 
  AstronautDuty, 
  CreatePersonRequest, 
  CreateAstronautDutyRequest,
  UpdateAstronautDutyRequest, 
  BaseResponse,
  PersonAstronaut 
} from '../models/person.model';

@Injectable({
  providedIn: 'root'
})
export class StargateApiService {
  // private readonly baseUrl = 'https://localhost:7204'; // Updated to match your API URL
  // private readonly baseUrl = 'https://stargate-api-prod.eba-spvrrfv5.us-east-1.elasticbeanstalk.com';
    private readonly baseUrl = 'https://api.rossludeke.com';

  private readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  // Loading state management
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) { }

  private setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }

  // Person endpoints
  getAllPeople(): Observable<Person[]> {
    this.setLoading(true);
    return this.http.get<any>(`${this.baseUrl}/Person`, this.httpOptions)
      .pipe(
        map(response => response.people || response.data || []),
        tap(() => this.setLoading(false))
      );
  }

  getPersonByName(name: string): Observable<Person> {
    this.setLoading(true);
    return this.http.get<BaseResponse<Person>>(`${this.baseUrl}/Person/${encodeURIComponent(name)}`, this.httpOptions)
      .pipe(
        map(response => response.data!),
        tap(() => this.setLoading(false))
      );
  }

  createPerson(request: CreatePersonRequest): Observable<BaseResponse> {
    this.setLoading(true);
    return this.http.post<BaseResponse>(`${this.baseUrl}/Person`, JSON.stringify(request.name), this.httpOptions)
      .pipe(
        tap(() => this.setLoading(false))
      );
  }

  // Astronaut Duty endpoints
  getAstronautDutiesByName(name: string): Observable<AstronautDuty[]> {
    this.setLoading(true);
    return this.http.get<any>(`${this.baseUrl}/AstronautDuty/${encodeURIComponent(name)}`, this.httpOptions)
      .pipe(
        map(response => {
          // Handle the actual API response structure
          if (response.astronautDuties) {
            return response.astronautDuties;
          } else if (response.data) {
            return response.data;
          } else {
            return [];
          }
        }),
        tap(() => this.setLoading(false))
      );
  }

  createAstronautDuty(request: CreateAstronautDutyRequest): Observable<BaseResponse> {
    this.setLoading(true);
    return this.http.post<BaseResponse>(`${this.baseUrl}/AstronautDuty`, request, this.httpOptions)
      .pipe(
        tap(() => this.setLoading(false))
      );
  }

  updateAstronautDuty(dutyId: number, request: UpdateAstronautDutyRequest): Observable<any> {
    this.setLoading(true);
    return this.http.put<any>(`${this.baseUrl}/AstronautDuty/${dutyId}`, request, this.httpOptions)
      .pipe(
        tap(() => this.setLoading(false))
      );
  }

  // Combined operations for better UX
  getPersonWithDuties(name: string): Observable<PersonAstronaut> {
    this.setLoading(true);
    return new Observable(observer => {
      // Get person and duties in parallel
      const person$ = this.getPersonByName(name);
      const duties$ = this.getAstronautDutiesByName(name);

      Promise.all([
        person$.toPromise(),
        duties$.toPromise()
      ]).then(([person, duties]) => {
        observer.next({
          person: person!,
          duties: duties || []
        });
        observer.complete();
        this.setLoading(false);
      }).catch(error => {
        observer.error(error);
        this.setLoading(false);
      });
    });
  }
}