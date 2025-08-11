namespace Receiver_linux_wayland.Core.DirectoryHelper;

public class DirectoryHierarchyBuilder : IDirectoryDescendantBuilder
{
    public IDirectoryDescendantBuilder WithSubdirectory(string name)
    {
        throw new NotImplementedException();
    }

    public IDirectoryDescendantBuilder ThenSubdirectory(
        Func<IDirectoryDescendantBuilder, IDirectoryDescendantBuilder> builder)
    {
        throw new NotImplementedException();
    }
}