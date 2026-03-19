using System.Threading.Tasks;
// Avoid referencing ASP.NET Core types in BLL project; web-specific implementation lives in MicroLend.Web

namespace MicroLend.BLL.Services
{
    public interface IDocumentService
    {
        Task<string> SaveDocumentAsync(object file, int userId, int? loanId = null);
    }
}
