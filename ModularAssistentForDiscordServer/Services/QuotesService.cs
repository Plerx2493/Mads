using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Services;

public class QuotesService
{
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;

    public QuotesService(IDbContextFactory<MadsContext> factory)
    {
        _dbContextFactory = factory;
    }
}