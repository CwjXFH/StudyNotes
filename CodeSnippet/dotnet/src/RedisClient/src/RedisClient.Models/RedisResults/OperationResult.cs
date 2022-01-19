namespace RedisClient.Models.RedisResults
{
    public readonly struct OperationResult
    {
        public OperationResult(bool successed)
        {
            Successed = successed;
        }

        public bool Successed { get; init; }

        public static implicit operator bool(OperationResult result) => result.Successed;

        public static implicit operator OperationResult(bool successed) => new OperationResult(successed);
    }


    public readonly struct OperationResult<T> where T : class
    {
        public OperationResult(bool successed, T data)
        {
            Successed = successed;
            Data = data;
        }


        public bool Successed { get; init; }
        public T Data { get; init; }

        // implicit convert will lose data
        //public static implicit operator bool(OperationResult<T> result) => result.Successed;

        public static implicit operator OperationResult<T>(bool successed) => new OperationResult<T>(successed, default!);
    }

}
