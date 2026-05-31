namespace Folium.Core.Exceptions;

/// <summary>Base exception for all Folium domain errors.</summary>
public class FoliumException : Exception
{
    public FoliumException(string message) : base(message) { }

    public FoliumException(string message, Exception innerException)
        : base(message, innerException) { }
}
