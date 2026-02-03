using HowsYourDayApi.DTOs.Day;
using HowsYourDayApi.Models;
using HowsYourDayApi.Repositories;

namespace HowsYourDayApi.Services
{
    public class DaySummaryService : IDaySummaryService
    {
        private readonly IDaySummaryRepository _daySummaryRepository;
        public DaySummaryService(IDaySummaryRepository daySummaryRepository)
        {
            _daySummaryRepository = daySummaryRepository;
        }

        public async Task<IEnumerable<DaySummaryDto>> GetDaySummariesAsync(DateTime fromUtc, DateTime toUtc)
        {
            var daySummaries = await _daySummaryRepository.SearchAsync(fromUtc, toUtc);

            var daySummaryDtos = daySummaries.Select(summary => new DaySummaryDto
            {
                DateStampUtc = summary.DateStampUtc,
                AverageRating = summary.AverageRating
            });

            return daySummaryDtos;
        }

        public async Task<DaySummaryDto?> GetDaySummaryOfDateAsync(DateTime dateUtc)
        {
            var daySummary = await _daySummaryRepository.GetByDateAsync(dateUtc);

            var daySummaryDto = new DaySummaryDto
            {
                DateStampUtc = dateUtc,
            };

            if (daySummary != null)
            {
                daySummaryDto.AverageRating = daySummary.AverageRating;
            }

            return daySummaryDto;
        }

        public async Task InsertDaySummaryAsync(DaySummaryDto daySummary)
        {
            if (daySummary == null)
                throw new ArgumentNullException(nameof(daySummary), "Day summary cannot be null.");

            var newDaySummary = new DaySummary
            {
                Id = Guid.NewGuid(),
                DateStampUtc = daySummary.DateStampUtc,
                AverageRating = daySummary.AverageRating
            };

            await _daySummaryRepository.InsertAsync(newDaySummary);
        }

        public async Task UpdateDaySummaryAsync(DaySummaryDto daySummary)
        {
            if (daySummary == null)
                throw new ArgumentNullException(nameof(daySummary), "Day summary cannot be null.");

            var existingSummary = await _daySummaryRepository.GetByDateAsync(daySummary.DateStampUtc);

            existingSummary.AverageRating = daySummary.AverageRating;

            await _daySummaryRepository.UpdateAsync(existingSummary);
        }
    }
}
