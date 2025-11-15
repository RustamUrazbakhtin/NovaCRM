using Microsoft.AspNetCore.Http;

namespace NovaCRM.Server.Contracts;

public class UploadAvatarForm
{
    public IFormFile File { get; set; } = default!;
}
