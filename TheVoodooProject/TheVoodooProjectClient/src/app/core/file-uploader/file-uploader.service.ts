import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})

export class FileUploaderService {
    constructor(private httpClient: HttpClient) {
    }

    postFile(fileToUpload: File): Observable<File> {
        const endpoint = 'endpointUrl';
          return this.httpClient.post<File>(endpoint, fileToUpload);
    }
}