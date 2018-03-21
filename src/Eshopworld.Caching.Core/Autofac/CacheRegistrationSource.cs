#if AUTOFAC
using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace Eshopworld.Caching.Core.Autofac
{
    // dear sonar - i hate having to do this to stop you whining
    public class CacheRegistrationSource
    {
        protected CacheRegistrationSource() { } // dear sonar - i hate having to do this to stop you whining
        protected static readonly MethodInfo OpenGenericFactoryMethod = typeof(ICacheFactory).GetRuntimeMethod(nameof(ICacheFactory.CreateDefault),new Type[0]);
    }

    public class CacheRegistrationSource<TCFactory> : CacheRegistrationSource, IRegistrationSource where TCFactory : ICacheFactory
    {
        private readonly Type cacheOpenGenericType;

        public bool IsAdapterForIndividualComponents => false;

        public CacheRegistrationSource(Type cacheType)
        {
            cacheOpenGenericType = cacheType;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null || !swt.ServiceType.IsConstructedGenericType || swt.ServiceType.GetGenericTypeDefinition() != cacheOpenGenericType) yield break;

            var registration = new ComponentRegistration(Guid.NewGuid(),
                new DelegateActivator(swt.ServiceType, (c, p) =>
                {
                    // factory method is generic, so we have to use reflection to make the call.
                    return OpenGenericFactoryMethod.MakeGenericMethod(swt.ServiceType.GenericTypeArguments)
                        .Invoke(c.Resolve<TCFactory>(), null);

                }), new CurrentScopeLifetime(), InstanceSharing.None, InstanceOwnership.OwnedByLifetimeScope,
                new[] { service }, new Dictionary<string, object>());

            yield return registration;
        }
    }
}

#endif