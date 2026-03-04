// Custom canvas-based CAPTCHA component — generates a 6-char alphanumeric code
// with background noise, wavy lines, and per-character rotation. No external deps.
import {
    Component, AfterViewInit, ViewChild, ElementRef,
    Output, EventEmitter, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-captcha',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './captchacomponent.html',
    styleUrl: './captchacomponent.css'
})
export class CaptchaComponent implements AfterViewInit {
    @ViewChild('captchaCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

    /** Emits true when the user's input matches the generated code. */
    @Output() captchaValid = new EventEmitter<boolean>();

    userInput = '';
    isVerified = signal(false);
    showError = signal(false);

    private code = '';

    // Alphanumeric set — excludes visually ambiguous chars (0, O, 1, I, l)
    private readonly CHARS = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';

    ngAfterViewInit(): void {
        this.generate();
    }

    /** Generates a new code and redraws the canvas. */
    generate(): void {
        this.code = Array.from(
            { length: 6 },
            () => this.CHARS[Math.floor(Math.random() * this.CHARS.length)]
        ).join('');

        this.userInput = '';
        this.isVerified.set(false);
        this.showError.set(false);
        this.captchaValid.emit(false);

        // Use rAF to ensure the canvas is in the DOM before drawing
        requestAnimationFrame(() => this.draw());
    }

    private draw(): void {
        const canvas = this.canvasRef?.nativeElement;
        if (!canvas) return;
        const ctx = canvas.getContext('2d')!;
        const W = canvas.width;   // 220
        const H = canvas.height;  // 64

        ctx.clearRect(0, 0, W, H);

        // ── Background gradient ──────────────────────────────────────────────────
        const bg = ctx.createLinearGradient(0, 0, W, H);
        bg.addColorStop(0, '#F4F1EC');
        bg.addColorStop(1, '#E8E3DA');
        ctx.fillStyle = bg;
        ctx.fillRect(0, 0, W, H);

        // ── Noise dots ───────────────────────────────────────────────────────────
        for (let i = 0; i < 90; i++) {
            ctx.beginPath();
            ctx.arc(
                Math.random() * W,
                Math.random() * H,
                Math.random() * 1.8,
                0, Math.PI * 2
            );
            ctx.fillStyle = `rgba(
        ${80 + ~~(Math.random() * 80)},
        ${60 + ~~(Math.random() * 60)},
        ${40 + ~~(Math.random() * 40)},
        ${0.25 + Math.random() * 0.45})`;
            ctx.fill();
        }

        // ── Noise bezier lines ───────────────────────────────────────────────────
        for (let i = 0; i < 5; i++) {
            ctx.beginPath();
            ctx.moveTo(Math.random() * W, Math.random() * H);
            ctx.bezierCurveTo(
                Math.random() * W, Math.random() * H,
                Math.random() * W, Math.random() * H,
                Math.random() * W, Math.random() * H
            );
            ctx.strokeStyle = `rgba(
        ${100 + ~~(Math.random() * 70)},
        ${70 + ~~(Math.random() * 50)},
        ${40 + ~~(Math.random() * 40)},
        0.35)`;
            ctx.lineWidth = 0.8 + Math.random() * 1.2;
            ctx.stroke();
        }

        // ── Characters ───────────────────────────────────────────────────────────
        const slotW = W / (this.code.length + 1);
        for (let i = 0; i < this.code.length; i++) {
            ctx.save();
            const x = slotW * (i + 1);
            const y = H / 2 + 9;
            ctx.translate(x, y);
            ctx.rotate((Math.random() - 0.5) * 0.45);          // ±~13°
            ctx.font = `bold ${21 + ~~(Math.random() * 7)}px 'Plus Jakarta Sans', sans-serif`;

            // Warm dark-brown palette per character
            const rShade = 50 + ~~(Math.random() * 60);
            const gShade = 35 + ~~(Math.random() * 45);
            const bShade = 18 + ~~(Math.random() * 30);
            ctx.fillStyle = `rgb(${rShade},${gShade},${bShade})`;
            ctx.fillText(this.code[i], 0, 0);
            ctx.restore();
        }
    }

    /** Called on every keystroke in the input field. */
    onInput(): void {
        const match = this.userInput.toUpperCase().trim() === this.code;
        this.isVerified.set(match);
        this.showError.set(this.userInput.length >= this.code.length && !match);
        this.captchaValid.emit(match);
    }

    refresh(): void {
        this.generate();
    }
}
