namespace Helpmebot.Configuration.Startup
{
    using System;
    using Castle.Core.Configuration;
    using Castle.MicroKernel.SubSystems.Conversion;
    using Helpmebot.Configuration;

    public class CommandOverrideMapEntryConverter : AbstractTypeConverter
    {
        public override bool CanHandleType(Type type)
        {
            return type == typeof(CommandOverrideConfiguration.OverrideMapEntry);
        }

        public override object PerformConversion(string value, Type targetType)
        {
            throw new NotImplementedException();
        }

        public override object PerformConversion(IConfiguration configuration, Type targetType)
        {
            var channel = configuration.Attributes.Get("channel");
            var keyword = configuration.Attributes.Get("keyword");
            var typeStr = configuration.Attributes.Get("type");

            var conversionManager = this.Context as IConversionManager;

            if (conversionManager == null)
            {
                throw new InvalidOperationException("Unable to instantiate CommandOverrideMap entry due to conversion context not being set as a conversion manager.");
            }
                
            var typeObj = conversionManager.PerformConversion<Type>(typeStr);

            return new CommandOverrideConfiguration.OverrideMapEntry{ Keyword = keyword, Channel = channel, CommandType = typeObj, Type = typeStr };
        }
    }
}