namespace Receiver_linux_wayland.Core.DirectoryHelper;

public interface IDirectoryDescendantBuilder
{
    public IDirectoryDescendantBuilder WithSubdirectory(string name);
    public IDirectoryDescendantBuilder ThenSubdirectory(
        Func<IDirectoryDescendantBuilder, IDirectoryDescendantBuilder> builder);
    
}