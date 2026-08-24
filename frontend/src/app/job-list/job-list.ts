import { Component, ElementRef, OnInit, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { JobService } from '../services/job.service';
import { Job } from '../models/job';

@Component({
  imports: [FormsModule],
  selector: 'app-job-list',
  styleUrl: './job-list.css',
  templateUrl: './job-list.html',
})
export class JobList implements OnInit {

  @ViewChild('sourceFileInput')
  sourceFileInput!: ElementRef<HTMLInputElement>;

  @ViewChild('translationFileInput')
 translationFileInput!: ElementRef<HTMLInputElement>;

  jobs = signal<Job[]>([]);
  newJobName = signal('');
  selectedJob = signal<Job | null>(null);
  selectedSourceFile = signal<File | null>(null);
  selectedTranslationFile = signal<File | null>(null);

  constructor(private jobService: JobService) {}

  ngOnInit(): void {
    this.jobService.getJobs().subscribe((jobs) => {
      this.jobs.set(jobs);
    });
  }

  createJob(): void {
  const name = this.newJobName();

  if (!name) {
    return;
  }

  this.jobService.createJob(name).subscribe((job) => {
    this.jobs.update((jobs) => [...jobs, job]);
    this.newJobName.set('');
  });
}

selectJob(job: Job): void {
  this.selectedJob.set(job);
}

onSourceFileSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];

  if (!file) {
    return;
  }

  this.selectedSourceFile.set(file);
}

uploadSourceFile(): void {
  const job = this.selectedJob();
  const file = this.selectedSourceFile();

  if (!job || !file) {
    return;
  }

this.jobService.uploadSourceFile(job.id, file).subscribe(() => {
  this.jobService.preprocessJob(job.id).subscribe(() => {
    this.selectedSourceFile.set(null);
    this.sourceFileInput.nativeElement.value = '';
  });
});
}

downloadPreparedSource(): void {
  const job = this.selectedJob();

  if (!job) {
    return;
  }

  this.jobService.downloadPreparedSource(job.id).subscribe((response) => {
    const blob = response.body;

    if (!blob) {
      return;
    }

    const contentDisposition = response.headers.get('Content-Disposition');

    let fileName = 'prepared-source.json';

    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename=([^;]+)/);

      if (fileNameMatch) {
        fileName = fileNameMatch[1];
      }
    }

    const url = window.URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();

    window.URL.revokeObjectURL(url);
  });
}

onTranslationFileSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];

  if (!file) {
    return;
  }

  this.selectedTranslationFile.set(file);
}

uploadTranslation(): void {
  const job = this.selectedJob();
  const file = this.selectedTranslationFile();

  if (!job || !file) {
    return;
  }

this.jobService.uploadTranslation(job.id, file).subscribe(() => {
  this.jobService.postprocessJob(job.id).subscribe(() => {
    this.selectedTranslationFile.set(null);
    this.translationFileInput.nativeElement.value = '';
  });
});
}

downloadDelivery(): void {
  const job = this.selectedJob();

  if (!job) {
    return;
  }

  this.jobService.downloadDelivery(job.id).subscribe((response) => {
    const blob = response.body;

    if (!blob) {
      return;
    }

    const contentDisposition = response.headers.get('Content-Disposition');

    let fileName = 'delivery.json';

    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename=([^;]+)/);

      if (fileNameMatch) {
        fileName = fileNameMatch[1];
      }
    }

    const url = window.URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();

    window.URL.revokeObjectURL(url);
  });
}

}
