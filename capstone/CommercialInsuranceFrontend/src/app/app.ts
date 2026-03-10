// Root application component. Bootstraps the Angular app, applies the saved theme (dark/light) on load, and hosts the top-level router outlet.
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbarcomponent';
import { FooterComponent } from './components/footer/footercomponent';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('InsureDesk');
}

