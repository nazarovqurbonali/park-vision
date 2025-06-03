namespace Domain.Resources;

/// <summary>
/// Centralized accessor for localized resource strings used across the application.
/// 
/// The <c>Messages</c> class provides strongly-typed, localized access to application messages
/// (error messages, notifications, and email templates), simplifying the use of <see cref="ResourceManager"/>.
/// 
/// Each property name maps to a key in a .resx file, and this design:
/// - Reduces hardcoded string usage
/// - Promotes reusability
/// - Enables clean localization and globalization
/// 
/// NOTE: <c>_resources.Get().AsString()</c> is expected to be a custom extension method that resolves the resource
/// string based on the calling property name using reflection or caller info.
/// </summary>
public static class Messages
{
    private static readonly ResourceManager Resources = new(typeof(Messages).FullName!, typeof(Messages).Assembly);

    public static string ConfigurationValueRequired(string key) => Resources.Get().Format(key);

    public static string ConfigurationValueMustBeInteger(string key, string? value) =>
        Resources.Get().Format(key, value);

    public static string ConfigurationValueMustBeBoolean(string key, string? value) =>
        Resources.Get().Format(key, value);

    public static string ConnectionStringNotFound(string name) => Resources.Get().Format(name);
    public static string ModelLoaderLoadModelNotFound => Resources.Get().AsString();
    public static string ModelLoaderLoadClassNamesNotFound => Resources.Get().AsString();
    public static string ModelLoaderLoadModelNotLoaded => Resources.Get().AsString();
    public static string ModelLoaderLoadClassNamesNotLoaded => Resources.Get().AsString();
}