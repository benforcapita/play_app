import { Component, signal, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { JobIndicatorComponent } from './core/components/job-indicator.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgIf, JobIndicatorComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('play-app');

  // Back button visibility (hide on character list page)
  showBack = signal(true);

  constructor(private router: Router) {}

  ngOnInit() {
    this.updateBackVisibility(this.router.url);
    this.router.events.subscribe((e) => {
      if (e instanceof NavigationEnd) {
        this.updateBackVisibility(e.urlAfterRedirects || e.url);
      }
    });
  }

  goBack() {
    // Try history back; fallback to characters
    if (typeof window !== 'undefined' && window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/characters']);
    }
  }

  private updateBackVisibility(url: string) {
    const path = url.split('?')[0];
    const noBackPaths = ['/characters', '/login'];
    this.showBack.set(!noBackPaths.includes(path));
  }
}
