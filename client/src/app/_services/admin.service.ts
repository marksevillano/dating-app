import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }

  getUserWithRoles() {
    return this.http.get<Partial<User[]>>(`${this.baseUrl}admin/users-with-roles`);
  }

  updateUserRoles(username: string, roles: string) {
    return this.http.post<Partial<string[]>>(`${this.baseUrl}admin/edit-roles/${username}?roles=${roles}`, {});
  }

  getPhotosToModerate(pageNumber, pageSize) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    return getPaginatedResult<Photo[]>(`${this.baseUrl}admin/photos-to-moderate`, params, this.http);
  }

  approveOrDisapprovePhotos(bodyParams: any) {
    return this.http.put<Partial<string>>(`${this.baseUrl}admin/photos-approve/`, bodyParams);
  }

}
