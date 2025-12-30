import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SignalrService } from './signalr.service';
import { AuthService } from './auth.service';
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, JsonPipe],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected signalr = inject(SignalrService);
  protected auth = inject(AuthService);
}


