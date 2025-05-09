import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AuthPayload {
  Id: string;
  Password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly BASE_URL = 'http://localhost:5000';

  constructor(private http: HttpClient) {}

  register(data: AuthPayload): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.BASE_URL}/register`, data);
  }

  login(data: AuthPayload): Observable<{ success: boolean; token?: string; message?: string }> {
    return this.http.post<{ success: boolean; token?: string; message?: string }>(`${this.BASE_URL}/login`, data);
  }
}