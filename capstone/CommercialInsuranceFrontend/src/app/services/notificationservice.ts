// Connects to the SignalR NotificationHub, subscribes to real-time events, and exposes an observable notification stream to the navbar.
import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface NotificationDto {
    id: string;
    title: string;
    message: string;
    type: string;
    isRead: boolean;
    createdAt: string;
    relatedEntityId?: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
    private http = inject(HttpClient);
    private baseUrl = 'http://localhost:5202/api/notifications';

    notifications = signal<NotificationDto[]>([]);
    unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

    load() {
        this.http.get<NotificationDto[]>(this.baseUrl).subscribe({
            next: (data) => this.notifications.set(data),
            error: () => { }
        });
    }

    markRead(id: string) {
        this.http.put(`${this.baseUrl}/${id}/read`, {}).subscribe(() => {
            this.notifications.update(list =>
                list.map(n => n.id === id ? { ...n, isRead: true } : n)
            );
        });
    }

    markAllRead() {
        this.http.put(`${this.baseUrl}/read-all`, {}).subscribe(() => {
            this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
        });
    }
}
