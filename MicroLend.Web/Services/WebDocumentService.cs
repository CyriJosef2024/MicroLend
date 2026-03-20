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
            if (file.Length <= 0) return string.Empty;

            // validate allowed extensions and size
            var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return string.Empty;
            const long maxBytes = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxBytes) return string.Empty;

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploads);
            ext = Path.GetExtension(file.FileName);
            var safeName = Path.GetFileNameWithoutExtension(file.FileName);
            var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Path.GetRandomFileName()}{ext}";
            var path = Path.Combine(uploads, fileName);
            try
            {
                // Create file and copy content
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(fs);
                await fs.FlushAsync();
            }
            catch
            {
                // In case of any IO error, attempt to delete partial file and return empty
                try { if (File.Exists(path)) File.Delete(path); } catch { }
                return string.Empty;
            }

            return "/uploads/" + fileName; // Updated return statement
        }
    }
}
