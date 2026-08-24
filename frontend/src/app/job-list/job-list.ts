import { Component, OnInit, signal } from '@angular/core';
import { JobService } from '../services/job.service';
import { Job } from '../models/job';

@Component({
  imports: [],
  selector: 'app-job-list',
  styleUrl: './job-list.css',
  templateUrl: './job-list.html',
})
export class JobList implements OnInit {
  jobs = signal<Job[]>([]);

  constructor(private jobService: JobService) {}

  ngOnInit(): void {
    this.jobService.getJobs().subscribe((jobs) => {
      this.jobs.set(jobs);
    });
  }
}
