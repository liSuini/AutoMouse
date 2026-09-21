using AutoMouse.Models;
using AutoMouse.Services;
using AutoMouse.Utils.Json;
using System.Text.Json;
using FluentAssertions;

namespace AutoMouse.Tests.Services;

public class InputEventConverterTests
{
    private readonly JsonSerializerOptions _options;

    public InputEventConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new InputEventConverter() }
        };
    }

    [Fact]
    public void Serialize_MouseEvent_ProducesCorrectJson()
    {
        var evt = new MouseEvent
        {
            Timestamp = 100,
            Action = MouseAction.Down,
            Button = MouseButton.Left,
            X = 500,
            Y = 300
        };

        string json = JsonSerializer.Serialize<InputEvent>(evt, _options);

        json.Should().Contain("\"type\": \"mouse\"");
        json.Should().Contain("\"timestamp\": 100");
        json.Should().Contain("\"action\": \"down\"");
        json.Should().Contain("\"button\": \"left\"");
        json.Should().Contain("\"x\": 500");
        json.Should().Contain("\"y\": 300");
    }

    [Fact]
    public void Serialize_ScrollEvent_IncludesDelta()
    {
        var evt = new MouseEvent
        {
            Timestamp = 200,
            Action = MouseAction.Scroll,
            X = 100,
            Y = 200,
            Delta = -120
        };

        string json = JsonSerializer.Serialize<InputEvent>(evt, _options);

        json.Should().Contain("\"action\": \"scroll\"");
        json.Should().Contain("\"delta\": -120");
    }

    [Fact]
    public void Serialize_MoveEvent_OmitsDelta()
    {
        var evt = new MouseEvent
        {
            Timestamp = 0,
            Action = MouseAction.Move,
            X = 10,
            Y = 20
        };

        string json = JsonSerializer.Serialize<InputEvent>(evt, _options);

        json.Should().NotContain("delta");
    }

    [Fact]
    public void Serialize_KeyboardEvent_ProducesCorrectJson()
    {
        var evt = new KeyboardEvent
        {
            Timestamp = 500,
            Key = 0x41,
            Action = KeyAction.Down,
            Ctrl = true
        };

        string json = JsonSerializer.Serialize<InputEvent>(evt, _options);

        json.Should().Contain("\"type\": \"keyboard\"");
        json.Should().Contain("\"key\": 65");
        json.Should().Contain("\"ctrl\": true");
    }

    [Fact]
    public void RoundTrip_MouseEvent_PreservesAllProperties()
    {
        var original = new MouseEvent
        {
            Timestamp = 1234,
            Action = MouseAction.Scroll,
            Button = MouseButton.None,
            X = 800,
            Y = 600,
            Delta = 240
        };

        string json = JsonSerializer.Serialize<InputEvent>(original, _options);
        var deserialized = JsonSerializer.Deserialize<InputEvent>(json, _options);

        var mouse = deserialized.Should().BeOfType<MouseEvent>().Subject;
        mouse.Timestamp.Should().Be(1234);
        mouse.Action.Should().Be(MouseAction.Scroll);
        mouse.Button.Should().Be(MouseButton.None);
        mouse.X.Should().Be(800);
        mouse.Y.Should().Be(600);
        mouse.Delta.Should().Be(240);
    }

    [Fact]
    public void RoundTrip_KeyboardEvent_PreservesAllProperties()
    {
        var original = new KeyboardEvent
        {
            Timestamp = 999,
            Key = 0x43,
            Action = KeyAction.Up,
            Ctrl = true,
            Alt = false,
            Shift = true,
            Win = false
        };

        string json = JsonSerializer.Serialize<InputEvent>(original, _options);
        var deserialized = JsonSerializer.Deserialize<InputEvent>(json, _options);

        var key = deserialized.Should().BeOfType<KeyboardEvent>().Subject;
        key.Timestamp.Should().Be(999);
        key.Key.Should().Be(0x43);
        key.Action.Should().Be(KeyAction.Up);
        key.Ctrl.Should().BeTrue();
        key.Shift.Should().BeTrue();
        key.Alt.Should().BeFalse();
        key.Win.Should().BeFalse();
    }

    [Fact]
    public void RoundTrip_MixedList_PreservesOrderAndTypes()
    {
        var events = new List<InputEvent>
        {
            new MouseEvent { Timestamp = 0, Action = MouseAction.Move, X = 100, Y = 200 },
            new MouseEvent { Timestamp = 120, Action = MouseAction.Down, Button = MouseButton.Left, X = 100, Y = 200 },
            new KeyboardEvent { Timestamp = 500, Key = 0x41, Action = KeyAction.Down, Ctrl = true },
            new KeyboardEvent { Timestamp = 560, Key = 0x41, Action = KeyAction.Up, Ctrl = true }
        };

        string json = JsonSerializer.Serialize(events, _options);
        var deserialized = JsonSerializer.Deserialize<List<InputEvent>>(json, _options);

        deserialized.Should().HaveCount(4);
        deserialized[0].Should().BeOfType<MouseEvent>();
        deserialized[1].Should().BeOfType<MouseEvent>();
        deserialized[2].Should().BeOfType<KeyboardEvent>();
        deserialized[3].Should().BeOfType<KeyboardEvent>();

        ((MouseEvent)deserialized[1]).Button.Should().Be(MouseButton.Left);
        ((KeyboardEvent)deserialized[2]).Ctrl.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_UnknownType_ThrowsJsonException()
    {
        var json = """{"type":"unknown","timestamp":0}""";

        var act = () => JsonSerializer.Deserialize<InputEvent>(json, _options);
        act.Should().Throw<JsonException>();
    }
}
