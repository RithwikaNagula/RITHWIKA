// Icon button that switches between dark and light mode by toggling the ThemeService; persists the preference across sessions.
import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../services/themeservice';

@Component({
    selector: 'app-theme-toggle',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './theme-toggle.component.html',
    styleUrl: './theme-toggle.component.css'
})
export class ThemeToggleComponent {
// State and data property: isDark
    isDark = computed(() => this.themeService.isDarkMode());

    constructor(public themeService: ThemeService) { }

// Toggles visibility or state flag for toggle
    toggle() {
        this.themeService.toggleTheme();
    }
}