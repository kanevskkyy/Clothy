using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Clients.Interfaces
{
    public interface IReviewGrpcClient
    {
        Task<ReviewsListGrpcResponse> GetReviewsByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default);
        Task<QuestionsListGrpcResponse> GetQuestionsAndAnswersByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default);
        Task<ReviewStatisticGrpcResponse> GetStatisticsByClotheIdAsync(Guid clotheId, CancellationToken cancellationToken = default);
    }
}
