using HowsYourDay.Server.DTOs.Day;
using HowsYourDay.Server.Exceptions;
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
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date range.",
                    Detail = "The 'from' date must be earlier than the 'to' date.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

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
            catch (DayEntryAlreadyExistsException)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "An entry already exists for today.",
                    Detail = "You can edit today's entry instead of creating another one.",
                    Status = StatusCodes.Status409Conflict
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid entry.",
                    Detail = "The entry could not be saved because one or more values are invalid.",
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }

        [HttpPut("me/entry/today")]
        public async Task<ActionResult<DayEntryDto>> EditEntryOfUserToday([FromBody] CreateDayEntryDto day)
        {
            var userId = User.GetUserId();

            // Get entry of user today
            var dayEntryToday = await _dayService.GetDayEntryOfUserTodayAsync(userId);

            if (dayEntryToday == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "No entry found for today.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            try
            {
                day.ApplyTo(dayEntryToday);

                await _dayService.UpdateDayEntryOfUserAsync(userId, dayEntryToday);

                return Ok(dayEntryToday.ToDto());
            }
            catch (DayEntryNotFoundException)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "No entry found for today.",
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid entry.",
                    Detail = "The entry could not be updated because one or more values are invalid.",
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}
