using System.Windows.Controls;

namespace Lab4.Contracts;

public interface IWidget
{
    string Name { get; }
    UserControl View { get; }
}
