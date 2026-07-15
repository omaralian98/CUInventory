namespace CUInventory.AppHost;

public static class ExtensionMethods
{
    public static IResourceBuilder<T> WaitForCompletionIfNotNull<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource>? dependency, int exitCode = 0) where T : IResourceWithWaitSupport
    {
        return dependency is null ? builder : builder.WaitForCompletion(dependency, exitCode);
    }
    public static IResourceBuilder<T> WaitForIfNotNull<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource>? dependency, WaitBehavior waitBehavior = default) where T : IResourceWithWaitSupport
    {
        return dependency is null ? builder : builder.WaitFor(dependency, waitBehavior);
    }
}