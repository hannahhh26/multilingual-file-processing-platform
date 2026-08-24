import { Component } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-job-list',
  styleUrl: './job-list.css',
  templateUrl: './job-list.html',
})
export class JobList {
  jobs = [
    { id: 1, name: 'Example Job 1' },
    { id: 2, name: 'Example Job 2' },
  ];
}
