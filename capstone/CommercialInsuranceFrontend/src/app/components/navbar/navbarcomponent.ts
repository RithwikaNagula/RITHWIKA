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

    showNotifications = false;

    @HostListener('document:click', ['$event'])
    onDocumentClick(event: Event) {
        const target = event.target as HTMLElement;
        if (!target.closest('.relative')) {
            this.showNotifications = false;
        }
    }

    toggleNotifications() {
        this.showNotifications = !this.showNotifications;
        if (this.showNotifications) {
            this.notifService.load();
        }
    }

    markRead(n: NotificationDto) {
        if (!n.isRead) this.notifService.markRead(n.id);
    }

    markAllRead() {
        this.notifService.markAllRead();
    }

    logout() {
        this.auth.logout();
        this.router.navigate(['/']);
    }
}

