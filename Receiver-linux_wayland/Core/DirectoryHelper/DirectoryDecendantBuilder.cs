namespace Receiver_linux_wayland.Core.DirectoryHelper;

public class DirectoryDescendantBuilder : IDirectoryDescendantBuilder
{
    private List<DirectoryDescendantBuilder> _children = new List<DirectoryDescendantBuilder>();
    private DirectoryDescendantBuilder _lastChild;
    
    private string _name;
    public DirectoryDescendantBuilder(string name)
    {
       _name = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;
    }
    
    public IDirectoryDescendantBuilder WithSubdirectory(string name)
    {
        _children.Add(new DirectoryDescendantBuilder(name));
        return this;
    }

    public IDirectoryDescendantBuilder ThenSubdirectory(
        Func<IDirectoryDescendantBuilder, IDirectoryDescendantBuilder> builder)
    {
        if (_lastChild == null) throw new InvalidOperationException("No subdirectory to reference"); 
        _lastChild = (DirectoryDescendantBuilder) builder(_lastChild);
        return this;
    }

}