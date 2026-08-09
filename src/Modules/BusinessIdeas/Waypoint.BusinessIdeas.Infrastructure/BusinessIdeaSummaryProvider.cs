using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Infrastructure;

/// <summary>Implements the cross-module IBusinessIdeaSummaryProvider read contract — see docs/03-domain-model.md.</summary>
public sealed class BusinessIdeaSummaryProvider(BusinessIdeasDbContext db, IDreamSummaryProvider dreamSummaryProvider)
    : IBusinessIdeaSummaryProvider
{
    public async Task<BusinessIdeaSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var idea = await db.BusinessIdeas.SingleOrDefaultAsync(i => i.DreamId == dream.DreamId, cancellationToken);
        if (idea is null)
        {
            return null;
        }

        return new BusinessIdeaSummary(
            idea.Id, dream.DreamId, idea.Problem, idea.Customer, idea.ValueProposition, idea.Solution,
            idea.BusinessModel, idea.Market, idea.Competitors, idea.Pricing, idea.Marketing, idea.Sales,
            idea.Operations, idea.Technology, idea.FinancialAssumptions, idea.Risks);
    }
}
