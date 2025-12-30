import { Routes } from '@angular/router';
import { KanbanViewComponent } from './kanban/kanban.component';

export const routes: Routes = [
    { path: 'kanban', component: KanbanViewComponent },
    { path: '', redirectTo: 'kanban', pathMatch: 'full' }
];
