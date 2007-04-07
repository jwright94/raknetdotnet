namespace EventSystem
{
    interface IProtocolProcessorsLocator
    {
        /// <summary>
        /// Why Processors is array type? Because one communicator can handle many protocols.
        /// </summary>
        IProtocolProcessor[] Processors { get; }
    }
}