using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMouse.Models;

namespace AutoMouse.Utils.Json;

/// <summary>
/// InputEvent 多态 JSON 序列化器。
/// 通过 "type" 鉴别字段区分 MouseEvent 和 KeyboardEvent。
/// </summary>
public class InputEventConverter : JsonConverter<InputEvent>
{
    private const string TypeMouse = "mouse";
    private const string TypeKeyboard = "keyboard";

    public override InputEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var typeStr = root.GetProperty("type").GetString();
        return typeStr switch
        {
            TypeMouse => ReadMouseEvent(root),
            TypeKeyboard => ReadKeyboardEvent(root),
            _ => throw new JsonException($"Unknown event type: {typeStr}")
        };
    }

    private static MouseEvent ReadMouseEvent(JsonElement root)
    {
        var evt = new MouseEvent
        {
            Timestamp = root.GetProperty("timestamp").GetInt64(),
            Action = ParseEnum<MouseAction>(root.GetProperty("action").GetString()!),
            Button = root.GetProperty("button").GetString() == "none"
                ? MouseButton.None
                : ParseEnum<MouseButton>(root.GetProperty("button").GetString()!),
            X = root.GetProperty("x").GetInt32(),
            Y = root.GetProperty("y").GetInt32()
        };
        if (root.TryGetProperty("delta", out var delta))
            evt.Delta = delta.GetInt32();
        return evt;
    }

    private static KeyboardEvent ReadKeyboardEvent(JsonElement root)
    {
        var evt = new KeyboardEvent
        {
            Timestamp = root.GetProperty("timestamp").GetInt64(),
            Key = root.GetProperty("key").GetUInt32(),
            Action = ParseEnum<KeyAction>(root.GetProperty("action").GetString()!)
        };
        if (root.TryGetProperty("ctrl", out var ctrl))
            evt.Ctrl = ctrl.GetBoolean();
        if (root.TryGetProperty("alt", out var alt))
            evt.Alt = alt.GetBoolean();
        if (root.TryGetProperty("shift", out var shift))
            evt.Shift = shift.GetBoolean();
        if (root.TryGetProperty("win", out var win))
            evt.Win = win.GetBoolean();
        return evt;
    }

    public override void Write(Utf8JsonWriter writer, InputEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", value.Type == InputEventType.Mouse ? TypeMouse : TypeKeyboard);
        writer.WriteNumber("timestamp", value.Timestamp);

        if (value is MouseEvent m)
        {
            writer.WriteString("action", m.Action.ToString().ToLowerInvariant());
            writer.WriteString("button", m.Button.ToString().ToLowerInvariant());
            writer.WriteNumber("x", m.X);
            writer.WriteNumber("y", m.Y);
            if (m.Action == MouseAction.Scroll)
                writer.WriteNumber("delta", m.Delta);
        }
        else if (value is KeyboardEvent k)
        {
            writer.WriteString("action", k.Action.ToString().ToLowerInvariant());
            writer.WriteNumber("key", k.Key);
            if (k.Ctrl) writer.WriteBoolean("ctrl", true);
            if (k.Alt) writer.WriteBoolean("alt", true);
            if (k.Shift) writer.WriteBoolean("shift", true);
            if (k.Win) writer.WriteBoolean("win", true);
        }

        writer.WriteEndObject();
    }

    private static T ParseEnum<T>(string value) where T : struct, Enum
    {
        // 忽略大小写解析枚举
        return Enum.TryParse<T>(value, ignoreCase: true, out var result)
            ? result
            : throw new JsonException($"Cannot parse {typeof(T).Name} from '{value}'");
    }
}
