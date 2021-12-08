namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     An input position refers to a specific character in a query.
    public interface IInputPosition
    {
        //
        // Summary:
        //     Gets the character offset referred to by this position; offset numbers start
        //     at 0.
        int Offset { get; }

        //
        // Summary:
        //     Gets the line number referred to by the position; line numbers start at 1.
        int Line { get; }

        //
        // Summary:
        //     Gets the column number referred to by the position; column numbers start at 1.
        int Column { get; }
    }
}
