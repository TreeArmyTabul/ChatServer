import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';

export interface AuthPayload {
  Id: string;
  Password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly BASE_URL = 'http://localhost:5000';

  constructor(private http: HttpClient) {}

  register(data: AuthPayload): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.BASE_URL}/register`, data).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.message);
        }
        return response;
      }),
      catchError((error) => {
        console.error('회원가입 실패', error);
        return throwError(() => new Error(error.message));
      })
    );
  }

  login(data: AuthPayload): Observable<{ success: boolean; message: string; token?: string; }> {
    return this.http.post<{ success: boolean; message: string; token?: string; }>(`${this.BASE_URL}/login`, data).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.message);
        }
        return response;
      }),
      catchError((error) => {
        console.error('로그인 실패', error);
        return throwError(() => new Error(error.message));
      })
    );
  }
}