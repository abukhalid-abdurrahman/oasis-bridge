namespace Application.DTOs.File.Requests;

public record FileUploadRequest(
   [Required] IFormFile File,
   [Required] FileType Type
);