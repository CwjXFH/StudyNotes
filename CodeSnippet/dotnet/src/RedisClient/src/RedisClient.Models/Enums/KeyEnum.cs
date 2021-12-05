namespace RedisClient.Models.Enums
{
    /// <summary>
    /// Indicates when this operation should be performed
    /// </summary>
    public enum KeyWriteBehavior
    {
        /// <summary>
        /// default
        /// </summary>
        None = 0,
        /// <summary>
        /// The operation should only occur when there is an existing value 
        /// </summary>
        Exists = 1,
        /// <summary>
        /// The operation should only occur when there is not an existing value 
        /// </summary>
        NotExists = 2
    }
}
