using AutoMouse.Models;
using AutoMouse.Services;
using AutoMouse.Services.Interfaces;
using FluentAssertions;

namespace AutoMouse.Tests.Services;

public class PlaybackServiceTests
{
    [Fact]
    public async Task PlayAsync_EmptyEvents_DoesNothing()
    {
        var service = new PlaybackService();
        var progressEvents = new List<PlaybackProgress>();
        service.ProgressChanged += (s, e) => progressEvents.Add(e);

        await service.PlayAsync(new List<InputEvent>(), 1, CancellationToken.None);

        progressEvents.Should().BeEmpty();
        service.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public async Task PlayAsync_SingleLoop_FiresProgressForEachEvent()
    {
        var service = new PlaybackService();
        var progressEvents = new List<PlaybackProgress>();
        service.ProgressChanged += (s, e) => progressEvents.Add(e);

        var events = new List<InputEvent>
        {
            new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 100, Y = 100 },
            new MouseEvent { Timestamp = 10, Action = MouseAction.Down, Button = MouseButton.Left, X = 100, Y = 100 },
            new MouseEvent { Timestamp = 20, Action = MouseAction.Up, Button = MouseButton.Left, X = 100, Y = 100 }
        };

        await service.PlayAsync(events, 1, CancellationToken.None);

        progressEvents.Should().HaveCount(3);
        progressEvents[0].CurrentStep.Should().Be(1);
        progressEvents[0].TotalSteps.Should().Be(3);
        progressEvents[0].CurrentLoop.Should().Be(1);
        progressEvents[0].TotalLoops.Should().Be(1);
        progressEvents[2].CurrentStep.Should().Be(3);
        progressEvents[2].CurrentLoop.Should().Be(1);
    }

    [Fact]
    public async Task PlayAsync_TwoLoops_FiresProgressForBothLoops()
    {
        var service = new PlaybackService();
        var progressEvents = new List<PlaybackProgress>();
        service.ProgressChanged += (s, e) => progressEvents.Add(e);

        var events = new List<InputEvent>
        {
            new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 50, Y = 50 },
            new MouseEvent { Timestamp = 5, Action = MouseAction.Down, Button = MouseButton.Left, X = 50, Y = 50 }
        };

        await service.PlayAsync(events, 2, CancellationToken.None);

        progressEvents.Should().HaveCount(4); // 2 events * 2 loops
        progressEvents[0].CurrentLoop.Should().Be(1);
        progressEvents[1].CurrentLoop.Should().Be(1);
        progressEvents[2].CurrentLoop.Should().Be(2);
        progressEvents[3].CurrentLoop.Should().Be(2);
    }

    [Fact]
    public async Task PlayAsync_Cancellation_StopsPlayback()
    {
        var service = new PlaybackService();
        var progressEvents = new List<PlaybackProgress>();
        service.ProgressChanged += (s, e) => progressEvents.Add(e);

        using var cts = new CancellationTokenSource();

        // 事件间隔较大，方便在中间取消
        var events = new List<InputEvent>
        {
            new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 10, Y = 10 },
            new MouseEvent { Timestamp = 2000, Action = MouseAction.Move, X = 20, Y = 20 },
            new MouseEvent { Timestamp = 4000, Action = MouseAction.Move, X = 30, Y = 30 }
        };

        // 在第一个事件后取消
        service.ProgressChanged += (s, e) =>
        {
            if (e.CurrentStep == 1)
                cts.Cancel();
        };

        await service.PlayAsync(events, 1, cts.Token);

        // 应该只完成了第一个事件
        progressEvents.Should().HaveCount(1);
        service.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public async Task PlayAsync_SetsIsPlayingDuringPlayback()
    {
        var service = new PlaybackService();
        var wasPlayingDuringExecution = false;

        var events = new List<InputEvent>
        {
            new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 10, Y = 10 }
        };

        service.ProgressChanged += (s, e) =>
        {
            wasPlayingDuringExecution = service.IsPlaying;
        };

        await service.PlayAsync(events, 1, CancellationToken.None);

        wasPlayingDuringExecution.Should().BeTrue();
        service.IsPlaying.Should().BeFalse();
    }
}
