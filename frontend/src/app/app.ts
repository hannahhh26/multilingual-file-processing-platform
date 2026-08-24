import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { JobList } from './job-list/job-list';

@Component({
  imports: [RouterOutlet, JobList],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('frontend');
}
