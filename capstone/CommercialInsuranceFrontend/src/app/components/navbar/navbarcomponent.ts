// Top navigation bar: shows logo, nav links, theme toggle, notification bell, and the authenticated user's name and role with a logout button.
import { Component, inject, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/authservice';
import { NotificationService, NotificationDto } from '../../services/notificationservice';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';

@Component({
    selector: 'app-navbar',
    standalone: true,
    imports: [RouterLink, RouterLinkActive, CommonModule, ThemeToggleComponent],
    templateUrl: './navbarcomponent.html',
    styleUrl: './navbarcomponent.css'
})
export class NavbarComponent {
    auth = inject(AuthService);
    notifService = inject(NotificationService);
    private router = inject(Router);

// State and data property: showNotifications
    showNotifications = false;

    @HostListener('document:click', ['$event'])
// Event listener hook triggered by onDocumentClick
    onDocumentClick(event: Event) {
        const target = event.target as HTMLElement;
        if (!target.closest('.relative')) {
            this.showNotifications = false;
        }
    }

// Toggles visibility or state flag for toggleNotifications
    toggleNotifications() {
        this.showNotifications = !this.showNotifications;
        if (this.showNotifications) {
            this.notifService.load();
        }
    }

// Mutates local state tracking for markRead
    markRead(n: NotificationDto) {
        if (!n.isRead) this.notifService.markRead(n.id);
    }

// Mutates local state tracking for markAllRead
    markAllRead() {
        this.notifService.markAllRead();
    }
// Invokes the AuthService to safely purge the local JWT session token and securely navigate to the login route

    logout() {
        this.auth.logout();
        this.router.navigate(['/']);
    }
}
