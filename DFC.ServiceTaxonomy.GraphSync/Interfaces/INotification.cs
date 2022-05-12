namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     Representation for notifications found when executing a query. A notification
    //     can be visualized in a client pinpointing problems or other information about
    //     the query.
    public interface INotification
    {
        //
        // Summary:
        //     Gets a notification code for the discovered issue.
        string Code { get; }

        //
        // Summary:
        //     Gets a short summary of the notification.
        string Title { get; }

        //
        // Summary:
        //     Gets a longer description of the notification.
        string Description { get; }

        //
        // Summary:
        //     Gets the position in the query where this notification points to. Not all notifications
        //     have a unique position to point to and in that case the position would be set
        //     to all 0s.
        IInputPosition Position { get; }

        //
        // Summary:
        //     Gets The severity level of the notification.
        string Severity { get; }
    }
}
