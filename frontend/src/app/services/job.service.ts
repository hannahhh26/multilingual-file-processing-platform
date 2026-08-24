import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Job } from '../models/job';

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly apiUrl = 'https://localhost:7271/api/jobs';

  constructor(private http: HttpClient) {}

  getJobs() {
    return this.http.get<Job[]>(this.apiUrl);
  }
}