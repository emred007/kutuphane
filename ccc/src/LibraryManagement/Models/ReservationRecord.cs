namespace LibraryManagement.Models;

public sealed class ReservationRecord
{
    public required Reservation Reservation { get; init; }
    public required Book Book { get; init; }
    public int QueuePosition { get; init; }
}
