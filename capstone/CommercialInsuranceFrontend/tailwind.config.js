/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: "#f8fafc", /* clean slate-50 for page backgrounds */
          100: "#f1f5f9", /* slate-100 for subtle shades/borders */
          200: "#e2e8f0", /* slate-200 for stronger borders */
          300: "#cbd5e1", /* slate-300 */
          400: "oklch(65% 0.04 35.7 / <alpha-value>)", /* Vibrant Burgundy for Dark Mode accents */
          500: "oklch(36.7% 0.016 35.7 / <alpha-value>)", /* The Charcoal Burgundy requested for buttons/accents */
          600: "oklch(30.0% 0.016 35.7 / <alpha-value>)", /* Darker charcoal for hover states */
          700: "#334155", /* slate-700 for secondary text */
          800: "#1e293b", /* slate-800 for dark mode cards */
          900: "#0f172a", /* slate-900 for dark mode page background */
          950: "#020617", /* slate-950 for deep contrast */
        }
      },
      fontFamily: {
        sans: ['"Inter"', 'system-ui', '-apple-system', 'sans-serif'],
        display: ['"Inter"', 'sans-serif'],
        mono: ['"JetBrains Mono"', '"Fira Code"', 'monospace'],
      },
      borderRadius: {
        '4xl': '2rem',
        '5xl': '2.5rem',
        '6xl': '3rem',
      },
      boxShadow: {
        'premium': '0 10px 40px -10px rgba(0, 0, 0, 0.08)',
        'premium-hover': '0 20px 60px -15px rgba(0, 0, 0, 0.12)',
        'glass': '0 8px 32px 0 rgba(0, 0, 0, 0.05)',
      },
      animation: {
        'fade-in': 'fadeIn 0.6s ease-out forwards',
        'slide-up': 'slideUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'slide-down': 'slideDown 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'scale-in': 'scaleIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'shimmer': 'shimmer 2s linear infinite',
        'float': 'float 6s ease-in-out infinite',
        'reveal': 'reveal 1.2s cubic-bezier(0.77, 0, 0.175, 1) forwards',
      },
      keyframes: {
        fadeIn: { from: { opacity: '0' }, to: { opacity: '1' } },
        slideUp: { from: { opacity: '0', transform: 'translateY(24px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        slideDown: { from: { opacity: '0', transform: 'translateY(-12px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        scaleIn: { from: { opacity: '0', transform: 'scale(0.9)' }, to: { opacity: '1', transform: 'scale(1)' } },
        shimmer: { from: { backgroundPosition: '-200% 0' }, to: { backgroundPosition: '200% 0' } },
        float: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        reveal: {
          '0%': { transform: 'translateY(100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
      },
    },
  },
  plugins: [],
}