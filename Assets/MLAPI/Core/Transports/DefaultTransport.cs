namespace Alpaca.Transports
{
    /// <summary>
    /// Supported built in transport
    /// </summary>
    public enum DefaultTransport
    {
        /// <summary>
        /// Unity's UNET transport
        /// </summary>
        UNET,
        /// <summary>
        /// Alpaca.Relay transport (UNET internally)
        /// </summary>
        ALPACA_Relay,
        /// <summary>
        /// Custom transport
        /// </summary>
        Custom
    }
}
