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

  createJob(name: string) {
  return this.http.post<Job>(this.apiUrl, { name });
}

uploadSourceFile(jobId: string, file: File) {
  const formData = new FormData();
  formData.append('file', file);

  return this.http.post(
    `${this.apiUrl}/${jobId}/source`,
    formData
  );
}

preprocessJob(jobId: string) {
  return this.http.post(
    `${this.apiUrl}/${jobId}/preprocess`,
    null
  );
}

downloadPreparedSource(jobId: string) {
  return this.http.get(
    `${this.apiUrl}/${jobId}/prepared-source`,
    {
      responseType: 'blob',
      observe: 'response'
    }
  );
}

}