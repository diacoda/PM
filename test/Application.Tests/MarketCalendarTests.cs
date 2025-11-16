using System;
using System.Collections.Generic;
using Xunit;
using PM.Application.Services;
using PM.Application.Interfaces;
using FluentAssertions;
using PM.SharedKernel;

namespace PM.Application.Services.Tests;

public class MarketCalendarTests
{
    private readonly Dictionary<string, List<DateOnly>> _holidays =
        new()
        {
            { "TSX", new List<DateOnly> {
                new (2025, 1, 1),
                new (2025, 2, 17),
                new (2025, 4, 18),
                new (2025, 5, 19),
                new (2025, 7, 1),
                new (2025, 8, 4),
                new (2025, 9, 1),
                new (2025, 10, 13),
                new(2025, 12, 25),
                new (2025, 12, 26),

                new (2026, 1, 1),
                new (2026, 2, 16),
                new (2026, 4, 3),
                new (2026, 5, 18),
                new (2026, 7, 1),
                new (2026, 8, 3),
                new (2026, 9, 7),
                new (2026, 10, 12),
                new (2026, 12, 25)
            } },
            { "NYSE", new List<DateOnly> {
                new (2025, 1, 1),
                new (2025, 1, 20),
                new (2025, 2, 17),
                new (2025, 4, 18),
                new (2025, 5, 26),
                new (2025, 6, 19),
                new (2025, 7, 4),
                new (2025, 9, 1),
                new (2025, 11, 27),
                new (2025, 12, 25),

                new (2026, 1, 1),
                new (2026, 1, 19),
                new (2026, 2, 16),
                new (2026, 4, 3),
                new (2026, 5, 25),
                new (2026, 6, 19),
                new (2026, 7, 3),
                new (2026, 9, 7),
                new (2026, 11, 26),
                new (2026, 12, 25)

            } }
        };

    // Fake clock for deterministic testing
    private class FakeClock : ISystemClock
    {
        public DateTime Now { get; set; }
    }

    // Helper to create calendar with an optional fake clock
    private MarketCalendar Create(FakeClock? fakeClock = null)
        => new(_holidays, fakeClock ?? new FakeClock { Now = DateTime.Now });

    // ─────────────────────────────────────────────────────────────────────
    // 1. GetCloseTime
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void GetCloseTime_ReturnsCorrectEasternTime()
    {
        var sut = Create();
        var date = new DateOnly(2025, 1, 15);

        var result = sut.GetCloseTime(date, "TSX");

        result.Year.Should().Be(2025);
        result.Month.Should().Be(1);
        result.Day.Should().Be(15);
        result.Minute.Should().Be(0);
        result.Hour.Should().Be(16);
        result.Second.Should().Be(0);
        result.Offset.Should().NotBe(TimeSpan.Zero); // EST/EDT offset expected
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. IsToday
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void IsToday_ReturnsTrue_ForToday()
    {
        var sut = Create();
        var today = DateOnly.FromDateTime(DateTime.Today);

        sut.IsToday(today).Should().BeTrue();
    }

    [Fact]
    public void IsToday_ReturnsFalse_ForAnotherDay()
    {
        var sut = Create();
        var other = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        sut.IsToday(other).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3. IsMarketOpen
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void IsMarketOpen_ReturnsFalse_OnWeekend()
    {
        var sut = Create();

        sut.IsMarketOpen(new DateOnly(2025, 1, 4), "TSX") // Saturday
            .Should().BeFalse();
        sut.IsMarketOpen(new DateOnly(2025, 1, 5), "TSX") // Sunday
            .Should().BeFalse();
    }

    [Fact]
    public void IsMarketOpen_ReturnsFalse_OnHoliday()
    {
        var sut = Create();

        sut.IsMarketOpen(new DateOnly(2026, 1, 1), "TSX") // New Year
            .Should().BeFalse();

        sut.IsMarketOpen(new DateOnly(2026, 1, 1), "NYSE") // New Year
            .Should().BeFalse();

    }

    [Fact]
    public void IsMarketOpen_ReturnsTrue_OnNormalWeekday()
    {
        var sut = Create();

        sut.IsMarketOpen(new DateOnly(2025, 1, 2), "TSX")
            .Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────
    // 4. IsHoliday
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void IsHoliday_ReturnsTrue_WhenDateIsInAnyHolidayList()
    {
        var sut = Create();

        sut.IsHoliday(new DateOnly(2025, 7, 4)) // NYSE holiday
            .Should().BeTrue();
    }

    [Fact]
    public void IsHoliday_ReturnsFalse_WhenNotHoliday()
    {
        var sut = Create();

        sut.IsHoliday(new DateOnly(2025, 1, 3))
            .Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────
    // 5. IsAfterMarketClose
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void IsAfterMarketClose_ReturnsTrue_WhenTimePastClose()
    {
        var fakeClock = new FakeClock { Now = new DateTime(2025, 1, 2, 17, 0, 0) };
        var sut = Create(fakeClock);

        sut.IsAfterMarketClose("TSX").Should().BeTrue();
    }

    [Fact]
    public void IsAfterMarketClose_ReturnsFalse_WhenBeforeClose()
    {
        var fakeClock = new FakeClock { Now = new DateTime(2025, 1, 2, 15, 0, 0) };
        var sut = Create(fakeClock);

        sut.IsAfterMarketClose("TSX").Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────
    // 6. GetNextMarketDay
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void GetNextMarketDay_SkipsWeekend()
    {
        var sut = Create();
        var friday = new DateOnly(2025, 1, 3); // Friday

        // Next is Monday the 6th (Jan 4–5 are weekend)
        sut.GetNextMarketDay(friday, "TSX")
            .Should().Be(new DateOnly(2025, 1, 6));
    }

    [Fact]
    public void GetNextMarketDay_SkipsHoliday()
    {
        var sut = Create();

        // Before Jan 1 2025 → next market day is Jan 2
        sut.GetNextMarketDay(new DateOnly(2024, 12, 31), "TSX")
            .Should().Be(new DateOnly(2025, 1, 2));
    }

    // ─────────────────────────────────────────────────────────────────────
    // 7. GetNextMarketRunDateTime
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void GetNextMarketRunDateTime_UsesNextOpenDay()
    {
        // Arrange
        // Set the clock to a known holiday
        var fakeClock = new FakeClock { Now = new DateTime(2025, 1, 1, 10, 0, 0) };
        var sut = Create(fakeClock);

        var runTime = TimeSpan.FromHours(17); // 5 PM

        // Act
        var nextRun = sut.GetNextMarketRunDateTime(runTime);

        // Assert
        // Jan 1 = holiday → next open day = Jan 2
        nextRun.Should().Be(new DateTime(2025, 1, 2, 17, 0, 0));
    }

    // ─────────────────────────────────────────────────────────────────────
    // 8. GetNextValuationDate
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void GetNextValuationDate_AllowsToday_WhenNoMarketRequired()
    {
        var sut = Create();

        sut.GetNextValuationDate(new DateOnly(2025, 1, 1), requireMarketOpen: false)
            .Should().Be(new DateOnly(2025, 1, 1)); // even though holiday
    }

    [Fact]
    public void GetNextValuationDate_SkipsClosedMarket()
    {
        var sut = Create();

        sut.GetNextValuationDate(new DateOnly(2025, 1, 1), requireMarketOpen: true)
            .Should().Be(new DateOnly(2025, 1, 2)); // next open day
    }

    // ─────────────────────────────────────────────────────────────────────
    // 9. GetNextValuationRunDateTime
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void GetNextValuationRunDateTime_CombinesDateAndTime()
    {
        var sut = Create();

        var time = TimeSpan.FromHours(2); // 2 AM
        var result = sut.GetNextValuationRunDateTime(time, requireMarketOpen: true);

        // Today is assumed NON-holiday for this test
        result.Hour.Should().Be(2);
    }
}
