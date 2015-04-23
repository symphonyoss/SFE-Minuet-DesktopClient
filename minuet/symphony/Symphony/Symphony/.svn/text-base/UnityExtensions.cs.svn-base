using Microsoft.Practices.Unity;

namespace Symphony
{
    public static class UnityExtensions
    {
        public static IUnityContainer RegisterSingleton<TFrom, TTo>(this IUnityContainer container) where TTo : TFrom
        {
            return container.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer RegisterSingleton<TInterface>(this IUnityContainer container, TInterface instance)
        {
            return container.RegisterInstance(instance, new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer RegisterSingleton<TInterface>(this IUnityContainer container)
        {
            return container.RegisterInstance(container.Resolve<TInterface>(), new ContainerControlledLifetimeManager());
        }
    }
}