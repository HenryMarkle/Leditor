using System.Numerics;
using Coords = (int X, int Y, int Z);

namespace Leditor;

#nullable enable

/// <summary>
/// General Reversible Action Manager
/// </summary>
public class GeoGram
{
    public interface IAction { Coords Position { get; } }

    public record CellAction(Coords Position, RunCell Previous, RunCell Next) : IAction;

    public record RectAction(Coords Position, RunCell[,] Previous, RunCell[,] Next) : IAction;

    public record GroupAction(CellAction[] CellActions) : IAction
    {
        public Coords Position => CellActions[0].Position;
    }
    
    private LinkedList<IAction> Actions { get; init; } = [];

    private LinkedListNode<IAction>? _currentNode;

    public IAction? Current=> _currentNode?.Value;

    public void Proceed(Coords position, RunCell previous, RunCell current)
    {
        if (_currentNode?.Next is not null)
        {
            _currentNode.Next.Value = new CellAction(position, previous, current);
            _currentNode = _currentNode.Next;
            return;
        }

        Actions.AddLast(new CellAction(position, previous, current));
        _currentNode = Actions.Last;
    }
    
    public void Proceed(Coords position, RunCell[,] previous, RunCell[,] current)
    {
        if (_currentNode?.Next is not null)
        {
            _currentNode.Next.Value = new RectAction(position, previous, current);
            _currentNode = _currentNode.Next;
            return;
        }
        
        Actions.AddLast(new RectAction(position, previous, current));
        _currentNode = Actions.Last;
    }

    public void Proceed(CellAction[] groupedActions)
    {
        if (_currentNode?.Next is not null)
        {
            _currentNode.Next.Value = new GroupAction(groupedActions);
            _currentNode = _currentNode.Next;
            return;
        }

        Actions.AddLast(new GroupAction(groupedActions));
        _currentNode = Actions.Last;
    }
    
    public void Redo()
    {
        _currentNode = _currentNode?.Next ?? _currentNode;
    }

    public void Undo()
    {
        _currentNode = _currentNode?.Previous ?? _currentNode;
    }
}