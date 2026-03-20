using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicroLend.BLL.Services;

namespace MicroLend.Web.Services
{
    public class WebDocumentService : IDocumentService
    {
        public async Task<string> SaveDocumentAsync(object fileObj, int userId, int? loanId = null)
        {
            if (fileObj is not IFormFile file) return string.Empty;
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{userId}_{Path.GetRandomFileName()}{ext}";
            var path = Path.Combine(uploads, fileName);
            using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);
            return "/uploads/" + fileName; // Updated return statement
        }
    }
}
