using Server.Cache;
using Server.Data;
using Server.TaskLocker;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace Server
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            UnityContainer container = new UnityContainer();

            container.RegisterType<ICache, CacheInMemory>();
            container.RegisterType<ITaskLocker, TaskLockerCache>();
            container.RegisterType<TaskDbContext>();
            container.RegisterType<ITaskRepository, TaskRepositoryEf>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
