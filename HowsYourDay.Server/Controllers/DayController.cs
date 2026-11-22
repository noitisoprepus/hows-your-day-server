using HowsYourDay.Server.DTOs.Day;
using HowsYourDay.Server.Extensions;
using HowsYourDay.Server.Mappings;
using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDay.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DayController : ControllerBase
    {
        private readonly IDayEntryService _dayService;

        public DayController(IDayEntryService dayService)
        {
            _dayService = dayService;
        }

        [HttpGet("me/entry/today")]
        public async Task<ActionResult<DayEntryDto>> GetEntryOfUserToday()
        {
            var userId = User.GetUserId();

            var result = await _dayService.GetDayEntryOfUserTodayAsync(userId);

            if (result == null)
                return NotFound();

            return Ok(result.ToDto());
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

            return Ok(result.Select(entry => entry.ToDto()));
        }

        [HttpPost("me/entry/today")]
        public async Task<ActionResult<DayEntryDto>> CreateEntryOfUserToday([FromBody] CreateDayEntryDto day)
        {
            var userId = User.GetUserId();

            try
            {
                var dayEntry = day.ToEntity();

                await _dayService.InsertDayEntryOfUserAsync(userId, dayEntry);

                return CreatedAtAction(nameof(GetEntryOfUserToday), new { }, dayEntry.ToDto());
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while posting your day: {ex.Message}");
            }
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

                day.ApplyTo(dayEntryToday);

                await _dayService.UpdateDayEntryOfUserAsync(userId, dayEntryToday);

                return Ok(dayEntryToday.ToDto());
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while updating your day: {ex.Message}");
            }
        }
    }
}