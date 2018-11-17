import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})

export class FileUploaderService {
    private baseUrl: string;

    constructor(private httpClient: HttpClient) {
        this.baseUrl = "https://websitel6kz46tu7tui6.azurewebsites.net/";
    }

    getTextFromSpeech(): Observable<string> {
        return this.httpClient.get<string>(this.baseUrl + "api/FileUpload/SpeechToText");
    }

    getSentimentScore(): Observable<number> {
        return this.httpClient.get<number>(this.baseUrl + "api/FileUpload/TextToSentiment");
    }
}