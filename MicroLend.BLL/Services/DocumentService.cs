using System.IO;
using System.IO;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class DocumentService : IDocumentService
    {
        public Task<string> SaveDocumentAsync(object file, int userId, int? loanId = null)
        {
            // Web implementation lives in MicroLend.Web; this placeholder avoids ASP.NET type references in BLL.
            return Task.FromResult(string.Empty);
        }
    }
}
