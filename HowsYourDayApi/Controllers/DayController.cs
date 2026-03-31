using HowsYourDayApi.DTOs.Day;
using HowsYourDayApi.Extensions;
using HowsYourDayApi.Models;
using HowsYourDayApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDayApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DayController : ControllerBase
    {
        private readonly IDayEntryService _dayService;
        private readonly IDaySummaryService _daySummaryService;

        public DayController(IDayEntryService dayService, IDaySummaryService daySummaryService)
        {
            _dayService = dayService;
            _daySummaryService = daySummaryService;
        }

        #region Day Entry
        [HttpGet("me/entry/today")]
        public async Task<ActionResult<DayEntryDto>> GetEntryOfUserToday()
        {
            var userId = User.GetUserId();

            var result = await _dayService.GetDayEntryOfUserTodayAsync(userId);

            return Ok(result);
        }

        [HttpGet("me/entry")]
        public async Task<ActionResult<IEnumerable<DayEntryDto>>> GetEntries([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest("The 'from' date must be earlier than the 'to' date.");

            // Convert to UTC if not already
            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            var userId = User.GetUserId();

            var result = await _dayService.GetDayEntriesOfUserAsync(userId, from, to);
            
            return Ok(result);
        }

        [HttpPost("me/entry/today")]
        public async Task<ActionResult<DayEntryDto>> CreateEntryOfUserToday([FromBody] CreateDayEntryDto day)
        {
            var userId = User.GetUserId();

            try
            {
                // Map DTO to the DayEntry entity
                var dayEntry = new DayEntry
                {
                    Rating = day.Rating,
                    Note = day.Note,
                };
                
                await _dayService.InsertDayEntryOfUserAsync(userId, dayEntry);
            }
            catch(Exception ex)
            {
                return BadRequest($"An error occurred while posting your day: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetEntryOfUserToday), new {}, null);
        }

        [HttpPut("me/entry/today")]
        public async Task<ActionResult<DayEntryDto>> EditEntryOfUserToday([FromBody] CreateDayEntryDto day)
        {
            var userId = User.GetUserId();

            try
            {
                // Get entry of user today
                var dayEntryToday = await _dayService.GetDayEntryOfUserTodayAsync(userId);
                
                if (dayEntryToday == null)
                    return NotFound();
                
                // Map DTO to the DayEntry entity
                dayEntryToday.Rating = day.Rating;
                dayEntryToday.Note = day.Note;
                
                await _dayService.UpdateDayEntryOfUserAsync(userId, dayEntryToday);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while updating your day: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetEntryOfUserToday), new {}, null);
        }
        #endregion

        #region Day Summary
        [HttpGet("summary/today")]
        public async Task<ActionResult<DaySummaryDto>> GetSummaryToday()
        {
            var result = await _daySummaryService.GetDaySummaryOfDateAsync(DateTime.UtcNow);
            
            if (result == null)
                return NotFound();
            
            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<DaySummaryDto>>> GetSummaries([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest("The 'from' date must be earlier than the 'to' date.");

            // Convert to UTC if not already
            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            var result = await _daySummaryService.GetDaySummariesAsync(from, to);

            return Ok(result);
        }
        #endregion
    }
}