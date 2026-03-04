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
    isDark = computed(() => this.themeService.isDarkMode());

    constructor(public themeService: ThemeService) { }

    toggle() {
        this.themeService.toggleTheme();
    }
}
