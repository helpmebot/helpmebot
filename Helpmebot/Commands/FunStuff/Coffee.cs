namespace helpmebot6.Commands
{
    /// <summary>
    /// The coffee.
    /// </summary>
    class Coffee : FunStuff.FunCommand
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string name;
            name = args.Length == 0 ? source.nickname : string.Join(" ", args);
            
            string[] messageparams = { name };
            string message = new Message().get("cmdCoffee", messageparams);
            
            return new CommandResponseHandler(message);
        }
    }
}
