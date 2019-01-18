using System;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Resolvers;

namespace VbsKisTaskFramesetExtractor
{
    /// <summary>
    /// Resolver, ktery zajistuje propagovani parametru pri resolvovani (CW defaultne posila parametry jen do rootu).
    /// http://stackoverflow.com/questions/3904951/castle-windsor-ioc-passing-constructor-parameters-to-child-components
    /// </summary>
    public class ArgumentPassingDependencyResolver : DefaultDependencyResolver
    {
        protected override CreationContext RebuildContextForParameter(CreationContext current, Type parameterType)
        {
            if (parameterType.ContainsGenericParameters)
            {
                // this behaviour copied from base class
                return current;
            }

            // the difference in the following line is that "true" is passed
            // instead of "false" as the third parameter
            return new CreationContext(parameterType, current, true);
        }
    }
}
