using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public interface ICreditScoreService
    {
        List<QuizQuestion> GetQuestions();
        Task<int> CalculateScoreAsync(Dictionary<int,int> answers);
        Task<int> ScoreAndSaveAsync(int userId, Dictionary<int,int> answers, decimal monthlyIncome);
    }
}
