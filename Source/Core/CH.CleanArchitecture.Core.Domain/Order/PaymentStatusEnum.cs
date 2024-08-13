namespace CH.CleanArchitecture.Core.Domain.Order
{
    public enum PaymentStatusEnum
    {
        New,
        Processing,
        Confirmed,
        Rejected,
        Aborted
    }
}
