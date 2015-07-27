namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFilterableQueryExecutor : IFirebaseQueryExecutor
    {
        IFirebaseQueryExecutorAny StartAt(string startingValue);
        IFirebaseQueryExecutorAny StartAt(long startingValue);
        IOrderableQueryExecutor EndAt(string endingValue);
        IOrderableQueryExecutor EndAt(long endingValue);
        IFirebaseQueryExecutorAny EqualTo(string value);
        IFirebaseQueryExecutorAny EqualTo(long value);
        IOrderableQueryExecutor LimitToFirst(int limit);
        IOrderableQueryExecutor LimitToLast(int limit);
    }
}