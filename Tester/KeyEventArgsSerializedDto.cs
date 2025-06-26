using System.Windows.Input;

namespace Tester;

public struct KeyEventArgsSerializedDto
{
    public string Key { get; set; }
    public bool IsToggled { get; set; }
    public bool IsRepeat { get; set; }
}
