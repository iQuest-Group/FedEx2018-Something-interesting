import { Component } from "@angular/core";
import { HttpEventType, HttpRequest, HttpClient } from "@angular/common/http";

@Component({
  selector: "app-file-uploader",
  templateUrl: "./file-uploader.component.html",
  styleUrls: ["./file-uploader.component.css"]
})
export class FileUploaderComponent {
  public progress: number;
  public message: string;

  constructor(private http: HttpClient) { }

  upload(files) {
    if (files.length === 0)
      return;

    const formData = new FormData();

    let file = files[0];
    formData.append('file', file, file.name);

    const uploadReq = new HttpRequest('POST', `http://localhost:56141/api/FileUpload`, formData, {
      reportProgress: true,
    });

    this.http.request(uploadReq).subscribe(event => {
      if (event.type === HttpEventType.Response)
        this.message = file.Name + " uploaded succesfully.";
    });
  }
}
