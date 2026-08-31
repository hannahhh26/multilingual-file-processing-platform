import { Component, ElementRef, OnInit, signal, ViewChild, AfterViewChecked } from '@angular/core';
import { createIcons, Download, Pencil, Trash2 } from 'lucide';
import { FormsModule } from '@angular/forms';
import { JobService } from '../services/job.service';
import { Job, TranslationValidation } from '../models/job';

@Component({
  imports: [FormsModule],
  selector: 'app-job-list',
  styleUrl: './job-list.css',
  templateUrl: './job-list.html',
})
export class JobList implements OnInit, AfterViewChecked {

  @ViewChild('sourceFileInput')
  sourceFileInput!: ElementRef<HTMLInputElement>;

  @ViewChild('translationFileInput')
 translationFileInput!: ElementRef<HTMLInputElement>;

  jobs = signal<Job[]>([]);
  newJobName = signal('');
  updatedJobName = signal('');
  selectedJob = signal<Job | null>(null);
  selectedSourceFile = signal<File | null>(null);
  selectedTranslationFile = signal<File | null>(null);
  errorMessage = signal<string | null>(null);
  validationErrors = signal<TranslationValidation | null>(null);

  constructor(private jobService: JobService) {}

ngOnInit(): void {
  this.jobService.getJobs().subscribe((jobs) => {
    this.jobs.set(jobs);
  });
}

ngAfterViewChecked(): void {
  createIcons({
    icons: {
      Download,
      Pencil,
      Trash2
    }
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
  this.updatedJobName.set(job.name);
  this.errorMessage.set(null);
  this.validationErrors.set(null);
}

updateJob(): void {
  const job = this.selectedJob();
  const name = this.updatedJobName();

  if (!job || !name) {
    return;
  }

  this.jobService.updateJob(job.id, name).subscribe((updatedJob) => {
    this.selectedJob.set(updatedJob);

    this.jobs.update((jobs) =>
      jobs.map((job) =>
        job.id === updatedJob.id ? updatedJob : job
      )
    );
  });
}

deleteJob(): void {
  const job = this.selectedJob();

  if (!job) {
    return;
  }

  this.jobService.deleteJob(job.id).subscribe(() => {
    this.jobs.update((jobs) =>
      jobs.filter((existingJob) => existingJob.id !== job.id)
    );

    this.selectedJob.set(null);
    this.updatedJobName.set('');
  });
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

  this.errorMessage.set(null);

  this.jobService.uploadSourceFile(job.id, file).subscribe({
    next: () => {
      this.jobService.preprocessJob(job.id).subscribe({
        next: () => {
          this.selectedSourceFile.set(null);
          this.sourceFileInput.nativeElement.value = '';
        },
        error: (error) => {
          this.errorMessage.set(
            error.error || 'Failed to preprocess source file.'
          );
        }
      });
    },
    error: (error) => {
      this.errorMessage.set(
        error.error || 'Failed to upload source file.'
      );
    }
  });
}

downloadPreparedSource(): void {
  const job = this.selectedJob();

  if (!job) {
    return;
  }

  this.errorMessage.set(null);

  this.jobService.downloadPreparedSource(job.id).subscribe({
  next: (response) => {
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
    },
  error: (error) => {
    this.errorMessage.set('Failed to download prepared source.'
    );
  }
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

  this.errorMessage.set(null);
  this.validationErrors.set(null);

  this.jobService.uploadTranslation(job.id, file).subscribe({
    next: () => {
      this.jobService.postprocessJob(job.id).subscribe({
        next: () => {
          this.selectedTranslationFile.set(null);
          this.translationFileInput.nativeElement.value = '';
        },
        error: (error) => {
          try {
            const errorResponse = JSON.parse(error.error);

            this.errorMessage.set(
              errorResponse.message || 'Failed to post-process translation.'
            );

            this.validationErrors.set(
              errorResponse.validation || null
            );
          } catch {
            this.errorMessage.set(
              error.error || 'Failed to post-process translation.'
            );

            this.validationErrors.set(null);
          }
        }
      });
    },
    error: (error) => {
      this.errorMessage.set(
        error.error || 'Failed to upload translation.'
      );
    }
  });
}



downloadDelivery(): void {
  const job = this.selectedJob();

  if (!job) {
    return;
  }

  this.errorMessage.set(null);

  this.jobService.downloadDelivery(job.id).subscribe({
    next: (response) => {
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
    },
    error: () => {
      this.errorMessage.set('Delivery file is not available.');
    }
  });
}

}
