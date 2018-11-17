import { Component } from "@angular/core";
import { HttpEventType, HttpRequest, HttpClient } from "@angular/common/http";
import { FileUploaderService } from "./file-uploader.service";

@Component({
  selector: "app-file-uploader",
  templateUrl: "./file-uploader.component.html",
  styleUrls: ["./file-uploader.component.css"]
})
export class FileUploaderComponent {
  private message: string;
  private textFromSpeech: string;
  private ourFile: File;
  private waiting: boolean
  private score: number;

  constructor(private http: HttpClient,
    private fileUploaderService: FileUploaderService) { }

  transformTextToScore() {
    this.fileUploaderService.getSentimentScore().subscribe(
      score => this.score = score * 100
    );
  }

  async transformAudioToText() {
    this.waiting = true;
    this.fileUploaderService.getTextFromSpeech().subscribe(
      textFromSpeech => this.textFromSpeech = textFromSpeech,
      () => this.waiting = false
    );
  }

  upload() {
    const formData = new FormData();
    formData.append('file', this.ourFile, this.ourFile.name);
    const uploadReq = new HttpRequest('POST', `https://websitel6kz46tu7tui6.azurewebsites.net/api/FileUpload`, formData, {
      reportProgress: true,
    });
    this.http.request(uploadReq).subscribe(event => {
      if (event.type === HttpEventType.Response)
        this.message = this.ourFile.name + " uploaded successfully.";
    });
  }

  openInput(){
    document.getElementById("fileInput").click();
  }

  fileChange(files: File[]) {
    if (files.length > 0) {
      this.ourFile = files[0];
    }
  }
}
