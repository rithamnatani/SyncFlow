import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

@Injectable({
    providedIn: 'root'
})
export class SignalrService {
    private hubConnection: HubConnection | null = null;

    // Signals to expose state to components
    public messages = signal<string[]>([]);
    public connectionStatus = signal<string>('Disconnected');

    constructor() {
        this.startConnection();
    }

    private startConnection() {
        this.connectionStatus.set('Connecting...');

        // In SWA, /api is proxied to the Functions backend.
        // The SignalR client will automatically append /negotiate to this URL.
        this.hubConnection = new HubConnectionBuilder()
            .withUrl('/api')
            .configureLogging(LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        this.hubConnection
            .start()
            .then(() => {
                console.log('SignalR Connected!');
                this.connectionStatus.set('Connected');
                this.addListeners();
            })
            .catch(err => {
                console.error('Error while starting connection: ' + err);
                this.connectionStatus.set('Error');
                setTimeout(() => this.startConnection(), 5000); // Retry
            });
    }

    private addListeners() {
        if (!this.hubConnection) return;

        this.hubConnection.on('newMessage', (message: string) => {
            console.log('New Message received:', message);
            this.messages.update(msgs => [...msgs, message]);
        });
    }
}
