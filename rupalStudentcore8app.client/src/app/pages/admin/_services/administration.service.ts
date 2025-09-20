import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdministrationService {
  ApiURL = environment.ApiURL;

  constructor(private http: HttpClient) { }

  getUserList(filterData: any): Observable<any> {
    return this.http.post(this.ApiURL + "User/GetList", filterData);
  }

  activeInactiveStatus(userId: number): Observable<any> {
    return this.http.put(this.ApiURL + "User/ActiveInactiveStatus/" + userId, null);
  }

  user2FA(userId: number): Observable<any> {
    return this.http.put(this.ApiURL + "User/User2FA/" + userId, null);
  }

  changePassword(data: any): Observable<any> {
    return this.http.post(this.ApiURL + 'User/ChangePassword', data, {
    });
  }

  addUser(userData: any): Observable<any> {
    return this.http.post(this.ApiURL + "User", userData);
  }

  editUser(userData: any): Observable<any> {
    return this.http.put(this.ApiURL + "User", userData);
  }

  getUserById(id: number): Observable<any> {
    return this.http.get(this.ApiURL + "User/" + id);
  }

  getRoleList(): Observable<any> {
    return this.http.get(this.ApiURL + "Role");
  }

  getRoleById(roleId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Role/" + roleId);
  }

  addRole(data: any): Observable<any> {
    return this.http.post(this.ApiURL + "Role", data);
  }

  editRole(data: any): Observable<any> {
    return this.http.put(this.ApiURL + "Role", data);
  }

  getPermissions(roleId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Permission/MenuList/" + roleId);
  }

  postPermissions(permissionData): Observable<any> {
    return this.http.post(this.ApiURL + "Permission", permissionData);
  }

  getFilteredMenuList(parentId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Menu/GetList?parentId=" + parentId);
  }

  getParentMenus(): Observable<any> {
    return this.http.get(this.ApiURL + "Menu/Name");
  }

  changeOrder(formData: any): Observable<any> {
    return this.http.post(this.ApiURL + "Menu/ChangeOrder", formData);
  }

  getMenuByMenuId(menuId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Menu/" + menuId);
  }

  addEditMenu(menuData: any): Observable<any> {
    return this.http.post(this.ApiURL + "Menu", menuData);
  }

  deleteMenu(menuId: number): Observable<any> {
    return this.http.delete(this.ApiURL + "Menu/" + menuId);
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete(this.ApiURL + "User/" + id);
  }

  deleteRole(roleId: number): Observable<any> {
    return this.http.delete(this.ApiURL + "Role/" + roleId);
  }

  deletePermission(permissionId: number): Observable<any> {
    return this.http.delete(this.ApiURL + "Menu/Permission/" + permissionId);
  }

  getPermissionByMenuId(menuId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Menu/PermissionByMenu/" + menuId);
  }

  getPermissionById(permissionId: number): Observable<any> {
    return this.http.get(this.ApiURL + "Menu/Permission/" + permissionId);
  }

  addEditPermission(permissionData: any): Observable<any> {
    return this.http.post(this.ApiURL + "Menu/Permission", permissionData);
  }

}
