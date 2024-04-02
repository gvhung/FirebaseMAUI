namespace FirebaseMAUI
{

    public static class ServiceHelper
    {
        public static IServiceProvider Current => IPlatformApplication.Current.Services;

        public static T GetService<T>() => (T)Current.GetService(typeof(T));
        public static TService GetService<TService>(Type type) where TService : class => GetService(type) as TService;
        public static object GetService(Type type) => Current.GetService(type);

        public static TService Resolve<TService>() => Current.GetRequiredService<TService>();

    }
}