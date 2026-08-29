namespace Plugin.Maui.Printing;

/// <summary>
/// Business document the job represents. Used for defaults and sample layouts.
/// </summary>
public enum PrintJobKind
{
    /// <summary>Unspecified document.</summary>
    Generic = 0,

    /// <summary>Tax / sales invoice.</summary>
    Invoice = 1,

    /// <summary>POS or shop receipt.</summary>
    Receipt = 2,

    /// <summary>Shipping, inventory, or product label.</summary>
    Label = 3,

    /// <summary>Event or boarding ticket.</summary>
    Ticket = 4,

    /// <summary>Goods delivery challan.</summary>
    DeliveryChallan = 5,

    /// <summary>Vehicle inspection report.</summary>
    InspectionReport = 6
}
