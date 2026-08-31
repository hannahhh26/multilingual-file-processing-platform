import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Job } from '../models/job';

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly apiUrl = 'http://localhost:8080/api/jobs';

  constructor(private http: HttpClient) {}

  getJobs() {
    return this.http.get<Job[]>(this.apiUrl);
  }

  createJob(name: string) {
  return this.http.post<Job>(this.apiUrl, { name });
}

updateJob(jobId: string, name: string) {
  return this.http.put<Job>(
    `${this.apiUrl}/${jobId}`,
    { name }
  );
}

deleteJob(jobId: string) {
  return this.http.delete(
    `${this.apiUrl}/${jobId}`
  );
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

uploadTranslation(jobId: string, file: File) {
  const formData = new FormData();
  formData.append('file', file);

  return this.http.post(
    `${this.apiUrl}/${jobId}/translation`,
    formData,
    {
      responseType: 'text'
    }
  );
}

postprocessJob(jobId: string) {
  return this.http.post(
    `${this.apiUrl}/${jobId}/postprocess`,
    null,
    {
      responseType: 'text'
    }
  );
}

downloadDelivery(jobId: string) {
  return this.http.get(
    `${this.apiUrl}/${jobId}/delivery`,
    {
      responseType: 'blob',
      observe: 'response'
    }
  );
}

}