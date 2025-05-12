import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthPayload, AuthService } from './auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth',
  imports: [FormsModule],
  providers: [AuthService],
  styleUrl: './auth.component.scss',
  templateUrl: './auth.component.html',
})
export class AuthComponent {
  isLoginMode = signal<boolean>(true);

  authData: { [key in Lowercase<keyof AuthPayload>]: string } = {
    id: '',
    password: '',
  };
  dataFormat: 'json' | 'protobuf' = 'json';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    if (this.isLoginMode()) {
      this.authService.login({ Id: this.authData.id, Password: this.authData.password }).subscribe({
        next: (response) => {
          this.router.navigate(['/chat'], { queryParams: { format: this.dataFormat, token: response.token, }});
        },
        error: () => {
          this.authData = { id: '', password: '' };
        }
      });
    } else {
      this.authService.register({ Id: this.authData.id, Password: this.authData.password }).subscribe({
        next: () => {
          this.isLoginMode.set(true);
        },
        error: () => {
          this.authData = { id: '', password: '' };
        }
      });
    }
  }

  toggleMode(): void {
    this.isLoginMode.update((value) => !value);
  }
}