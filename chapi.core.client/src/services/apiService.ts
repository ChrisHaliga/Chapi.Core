import { Injectable } from "@angular/core";
import { environment } from "../environments/environment";
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";


@Injectable({
  providedIn: "root"
})
export class ApiService {
  private defaultHeaders = {
    
  };

  constructor(private httpClient: HttpClient) { }

  public Get<T>(endpoint: string, headers?: { [header: string]: string }, params?: { [param: string]: string | number }): Observable<T> {
    console.log("getting")
    return this.httpClient.get<T>(`${environment.apiRoot}${endpoint}`, { headers: this.buildHeaders(headers), params: this.buildParams(params) });
  }

  public Post<T>(endpoint: string, body: any, headers ?: { [header: string]: string }, params ?: { [param: string]: string | number }): Observable < T > {
    return this.httpClient.post<T>(`${environment.apiRoot}${endpoint}`, body, { headers: this.buildHeaders(headers), params: this.buildParams(params) });
  }

  public Put<T>(endpoint: string, body: any, headers ?: { [header: string]: string }, params ?: { [param: string]: string | number }): Observable < T > {
    return this.httpClient.put<T>(`${environment.apiRoot}${endpoint}`, body, { headers: this.buildHeaders(headers), params: this.buildParams(params) });
  }

  public Patch<T>(endpoint: string, body: any, headers ?: { [header: string]: string }, params ?: { [param: string]: string | number }): Observable < T > {
    return this.httpClient.patch<T>(`${environment.apiRoot}${endpoint}`, body, { headers: this.buildHeaders(headers), params: this.buildParams(params) });
  }

  public Delete<T>(endpoint: string, headers ?: { [header: string]: string }, params ?: { [param: string]: string | number }): Observable < T > {
    return this.httpClient.delete<T>(`${environment.apiRoot}${endpoint}`, { headers: this.buildHeaders(headers), params: this.buildParams(params) });
  }

  private buildHeaders(headers?: { [header: string]: string }): HttpHeaders {
    return new HttpHeaders({ ...this.defaultHeaders, ...headers });
  }

  private buildParams(params?: { [param: string]: string | number }): HttpParams {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        httpParams = httpParams.set(key, params[key].toString());
      });
    }
    return httpParams;
  }
}
