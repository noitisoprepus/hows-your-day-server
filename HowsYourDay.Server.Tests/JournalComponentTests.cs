using System.Security.Claims;
using Bunit;
using HowsYourDay.Server.Components;
using HowsYourDay.Server.Components.Pages;
using HowsYourDay.Server.DTOs.Day;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HowsYourDay.Server.Tests;

public sealed class JournalComponentTests : BunitContext
{
    private static readonly Guid UserId = Guid.Parse("c047cbeb-9586-477c-aefe-9e7b6a0c45fa");
    private static readonly DateTimeOffset Now = new(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void WeatherPicker_RendersAccessibleChoicesAndReportsSelection()
    {
        int? selected = null;
        var picker = Render<WeatherPicker>(parameters => parameters
            .Add(component => component.Name, "rating")
            .Add(component => component.Required, true)
            .Add(component => component.Value, 7)
            .Add(component => component.ValueChanged, (int? rating) => selected = rating));

        var choices = picker.FindAll("input[type=radio]");
        Assert.Equal(10, choices.Count);
        for (var index = 0; index < choices.Count; index++)
        {
            var choice = choices[index];
            Assert.Equal("rating", choice.GetAttribute("name"));
            Assert.True(choice.HasAttribute("required"));
            Assert.Equal((index + 1).ToString(), choice.GetAttribute("value"));
            Assert.StartsWith($"{index + 1} out of 10, ", choice.GetAttribute("aria-label"));
        }

        var icons = picker.FindAll(".weather-tile img");
        Assert.Equal(10, icons.Count);
        Assert.Equal(10, icons.Select(icon => icon.GetAttribute("src")).Distinct().Count());
        Assert.All(icons, icon =>
        {
            Assert.StartsWith("icons/weather/", icon.GetAttribute("src"));
            Assert.EndsWith(".svg", icon.GetAttribute("src"));
            Assert.Equal("", icon.GetAttribute("alt"));
        });
        Assert.Equal("7", picker.Find("input[checked]").GetAttribute("value"));

        picker.Find("input[value='4']").Change("4");

        Assert.Equal(4, selected);
    }

    [Fact]
    public void Today_EmptyEntryRendersSaveFormAndEmptyInsight()
    {
        SetUpJournal();
        var today = Render<Today>();

        var form = today.Find("form[data-journal-save]");
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("2026-11-22", form.QuerySelector("input[name='Draft.Date']")?.GetAttribute("value"));
        Assert.Equal("false", form.QuerySelector("input[name='Draft.IsUpdate']")?.GetAttribute("value"));
        Assert.Equal(10, form.QuerySelectorAll("input[name='Draft.Rating']").Length);
        Assert.Empty(form.QuerySelector("textarea[name='Draft.Note']")!.TextContent);
        Assert.Equal("Save today’s entry", today.Find("form button").TextContent);
        Assert.Contains("Nothing to read yet.", today.Find(".insight-panel").TextContent);
    }

    [Fact]
    public void Today_SavedEntryPrefillsTheUpdateFormAndShowsInsight()
    {
        SetUpJournal(CreateEntry(0));
        var today = Render<Today>();

        Assert.Equal("true", today.Find("input[name='Draft.IsUpdate']").GetAttribute("value"));
        Assert.Equal("7", today.Find("input[name='Draft.Rating'][checked]").GetAttribute("value"));
        Assert.Equal("Journal note 0", today.Find("textarea[name='Draft.Note']").TextContent);
        Assert.Equal("Update today’s entry", today.Find("form button").TextContent);
        Assert.Contains("A calm day.", today.Find(".insight-panel").TextContent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InsightPanel_ShowsTheReadingOrItsEmptyMessage(bool hasInsight)
    {
        var insight = hasInsight
            ? new DayEntryInsightDto { Label = "Positive", Summary = "A calm day." }
            : null;
        var panel = Render<InsightPanel>(parameters => parameters.Add(component => component.Insight, insight));

        if (hasInsight)
        {
            var reading = panel.Find(".reading-label").TextContent;
            Assert.Contains("How it read", reading);
            Assert.Contains("Positive", reading);
            Assert.Contains("A calm day.", panel.Find("section").TextContent);
        }
        else
        {
            Assert.Equal("Nothing to read yet.", panel.Find("p").TextContent);
            Assert.Empty(panel.FindAll(".reading-label"));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JournalHistory_ShowsTheRightCardContentAndAppendsOlderEntries(bool insights)
    {
        var entries = Enumerable.Range(0, 11).Select(CreateEntry).ToArray();
        const string opening = "This is a long journal entry. ";
        entries[0].Note = opening + new string('a', 180) + " end of note";
        SetUpJournal(entries);
        var history = Render<JournalHistory>(parameters => parameters.Add(component => component.Insights, insights));

        var cards = history.FindAll(".history-list article");
        Assert.Equal(10, cards.Count);
        var first = cards[0];
        var reading = first.QuerySelector(".reading-label")!.TextContent;
        Assert.Contains("How it read", reading);
        Assert.Contains("Positive", reading);
        Assert.Equal("/days/2026-11-22", first.QuerySelector("h3 a")?.GetAttribute("href"));

        if (insights)
        {
            Assert.Contains("A calm day.", first.TextContent);
            Assert.Contains(first.QuerySelectorAll("a"), link =>
                link.TextContent == "View day" && link.GetAttribute("href") == "/days/2026-11-22");
        }
        else
        {
            Assert.Equal("Your weather 7/10", first.QuerySelector(".weather-rating")?.TextContent);
            var excerpt = first.QuerySelector(".note-text")!.TextContent;
            Assert.StartsWith(opening, excerpt);
            Assert.EndsWith("…", excerpt);
            Assert.DoesNotContain("end of note", excerpt);
        }

        var label = insights ? "Load earlier insights" : "Load earlier days";
        Assert.Single(history.FindAll("button"), button => button.TextContent == label).Click();

        var dates = history.FindAll(".history-list article h3 a")
            .Select(link => link.GetAttribute("href"))
            .ToArray();
        Assert.Equal(11, dates.Length);
        Assert.Equal(11, dates.Distinct().Count());
        Assert.Equal("/days/2026-11-22", dates[0]);
        Assert.Equal("/days/2026-11-12", dates[^1]);
        Assert.DoesNotContain(history.FindAll("button"), button => button.TextContent == label);
    }

    private void SetUpJournal(params DayEntry[] entries)
    {
        var auth = AddAuthorization();
        auth.SetAuthorized("Journal user");
        auth.SetClaims(new Claim("sub", UserId.ToString()));
        var clock = new FixedTimeProvider(Now);
        var zones = new FakeUserTimeZoneService(TimeZoneInfo.Utc);

        Services.AddSingleton<TimeProvider>(clock);
        Services.AddSingleton<IUserTimeZoneService>(zones);
        Services.AddSingleton<IDayEntryService>(new DayEntryService(
            new FakeDayEntryRepository(entries),
            new FakeDayEntryAnalysisRepository(),
            new FakeNlpService(),
            zones,
            clock));
    }

    private static DayEntry CreateEntry(int daysAgo)
    {
        var date = Now.UtcDateTime.AddDays(-daysAgo);
        return new DayEntry
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            EntryDate = DateOnly.FromDateTime(date),
            LoggedAtUtc = date,
            Rating = 7,
            Note = $"Journal note {daysAgo}",
            Analysis = new DayEntryAnalysis { Label = "Positive", Summary = "A calm day." }
        };
    }
}
