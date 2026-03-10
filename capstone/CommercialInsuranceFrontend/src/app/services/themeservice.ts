// Persists and applies the user's dark/light mode preference using localStorage and the Tailwind 'dark' class on the html element.
import { Injectable, signal, effect } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    // We use an Angular signal to reactive hold the current theme state
    public isDarkMode = signal<boolean>(false);

    constructor() {
        this.initializeTheme();

        // Effect automatically runs whenever isDarkMode changes
        effect(() => {
            const isDark = this.isDarkMode();

            // Update the DOM
            if (isDark) {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }

            // Persist to localStorage
            localStorage.setItem('theme', isDark ? 'dark' : 'light');
        });
    }

    private initializeTheme(): void {
        const savedTheme = localStorage.getItem('theme');

        if (savedTheme) {
            this.isDarkMode.set(savedTheme === 'dark');
        } else {
            // Check OS preference
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            this.isDarkMode.set(prefersDark);
        }
    }

    // Flips the stored theme between 'dark' and 'light', updates localStorage, and adds/removes
    // the Tailwind 'dark' class on the <html> element so all dark: variants take effect instantly.
    public toggleTheme(): void {
        this.isDarkMode.update((dark: boolean) => !dark);
    }
}
