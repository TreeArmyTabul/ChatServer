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
    password: ''
  };

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    if (this.isLoginMode()) {
      this.authService.login({ Id: this.authData.id, Password: this.authData.password }).subscribe({
        next: (response) => {
          console.log("로그인 성공", response);
          this.router.navigate(['/chat'], { queryParams: { token: response.token }});
        },
        error: (err) => {
          console.error('로그인 실패', err);
        }
      });
    } else {
      this.authService.register({ Id: this.authData.id, Password: this.authData.password }).subscribe({
        next: (response) => {
          console.log("회원가입 성공", response);
          this.router.navigate(['/chat']);
        },
        error: (err) => {
          console.error('회원가입 실패', err);
        }
      });
    }
  }

  toggleMode(): void {
    this.isLoginMode.update((value) => !value);
  }
}