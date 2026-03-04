/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        // ─── Brand Palette — extracted from #907E80 button ─────────────────
        // Primary brand: dusty mauve / muted rose  hsl(345, 7%, 54%)
        // Hex: #907E80 = taupe-600 (THE brand action color)
        taupe: {
          50: '#FAF8F8',   // near-white — lightest surfaces
          100: '#F0ECEC',   // very light — section fills, hover overlays
          200: '#E1DADB',   // light — card borders, dividers, input borders
          300: '#CFc6C7',   // medium-light — subtle highlights
          400: '#BEB4B6',   // ← brand mid (link color, secondary icons)
          500: '#A89799',   // medium — secondary buttons, badges
          600: '#907E80',   // ← PRIMARY BRAND — buttons, accents (from screenshot)
          700: '#6B5E60',   // dark — headings, strong elements
          800: '#45393A',   // very dark — body text, nav
          900: '#251C1D',   // near-black — max contrast, dark surfaces
        },
        // Dark mode surface palette
        surface: {
          DEFAULT: '#FAF8F8',     // light bg
          dark: '#140E0F',        // darkest bg (html root)
          card: '#1C1517',        // card bg in dark
          elevated: '#221A1B',    // elevated surface in dark
          border: '#2E2122',      // border in dark
        }
      },
      fontFamily: {
        // ─── Display / headings — elegant serif ──────────────────────────
        display: ['"DM Serif Display"', 'Georgia', 'serif'],
        // ─── UI / body — clean modern sans-serif ─────────────────────────
        sans: ['"DM Sans"', 'system-ui', '-apple-system', 'BlinkMacSystemFont', '"Segoe UI"', 'sans-serif'],
        // ─── Monospace — for IDs, codes, numbers ─────────────────────────
        mono: ['"JetBrains Mono"', '"Fira Code"', 'monospace'],
      },
      fontSize: {
        'micro': ['0.625rem', { lineHeight: '1rem', letterSpacing: '0.1em' }],    // 10px
        'tiny': ['0.6875rem', { lineHeight: '1rem', letterSpacing: '0.08em' }],  // 11px
      },
      borderRadius: {
        '4xl': '2rem',
        '5xl': '2.5rem',
        '6xl': '3rem',
      },
      boxShadow: {
        'brand-sm': '0 4px 16px -2px rgba(144, 126, 128, 0.20)',
        'brand-md': '0 8px 32px -4px rgba(144, 126, 128, 0.28)',
        'brand-lg': '0 16px 48px -8px rgba(144, 126, 128, 0.32)',
        'brand-xl': '0 24px 64px -12px rgba(144, 126, 128, 0.38)',
        'dark-sm': '0 4px 16px -2px rgba(0, 0, 0, 0.30)',
        'dark-md': '0 8px 32px -4px rgba(0, 0, 0, 0.40)',
        'dark-lg': '0 16px 48px -8px rgba(0, 0, 0, 0.50)',
        'card': '0 1px 3px rgba(0,0,0,0.04), 0 8px 24px -4px rgba(144, 126, 128, 0.10)',
        'card-hover': '0 4px 6px rgba(0,0,0,0.04), 0 16px 40px -6px rgba(144, 126, 128, 0.18)',
      },
      animation: {
        'fade-in': 'fadeIn 0.4s ease-out',
        'slide-up': 'slideUp 0.5s cubic-bezier(0.16, 1, 0.3, 1)',
        'slide-down': 'slideDown 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
        'scale-in': 'scaleIn 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
        'shimmer': 'shimmer 2s linear infinite',
      },
      keyframes: {
        fadeIn: { from: { opacity: '0' }, to: { opacity: '1' } },
        slideUp: { from: { opacity: '0', transform: 'translateY(16px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        slideDown: { from: { opacity: '0', transform: 'translateY(-8px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        scaleIn: { from: { opacity: '0', transform: 'scale(0.95)' }, to: { opacity: '1', transform: 'scale(1)' } },
        shimmer: { from: { backgroundPosition: '-200% 0' }, to: { backgroundPosition: '200% 0' } },
      },
      transitionTimingFunction: {
        'spring': 'cubic-bezier(0.16, 1, 0.3, 1)',
      },
    },
  },
  plugins: [],
}
