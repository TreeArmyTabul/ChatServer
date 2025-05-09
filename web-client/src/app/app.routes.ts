import { Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { ChatComponent } from './chat/chat.component';

export const routes: Routes = [
    { path: '', redirectTo: 'auth', pathMatch: 'full' },
    { path: 'auth', component: AuthComponent },
    { path: 'chat', component: ChatComponent }
  ];
