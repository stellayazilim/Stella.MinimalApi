using System.Text.Json.Serialization;

namespace Stella.MinimalApi.Sample.Data;

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);


[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

public static class SampleTodos
{
    public static Todo[] GetTodos =>
    [
        new(1, "Walk the dog"),
        new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
        new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
        new(4, "Clean the bathroom"),
        new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
    ];
}