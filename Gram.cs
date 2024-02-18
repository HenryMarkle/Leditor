using System.Numerics;

namespace Leditor;

#nullable enable

public readonly record struct Coords(int X, int Y, int Z)
{
    public static implicit operator Coords((int x, int y, int z) tuple) => new(tuple.x, tuple.y, tuple.z);

    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }
        
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}

/// General Reversible Action Manager
public class GeoGram(int limit)
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

    public IAction? Current => _currentNode?.Value;

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
        
        //
        
        if (Actions.Count == limit) Actions.RemoveFirst();
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
        
        //
        
        if (Actions.Count == limit) Actions.RemoveFirst();
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
        
        //
        
        if (Actions.Count == limit) Actions.RemoveFirst();
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

public class TileGram(int limit)
{
    public interface IAction { }
    
    public interface IPlaceAction : IAction{ }
    public interface IRemoveAction : IAction { }
    public interface IGroupAction : IAction { }
    
    internal sealed record PlaceTileAction(Coords Position, (int category, int position) Tile, bool WithGeo) : IPlaceAction;
    internal sealed record RemoveTileAction(Coords Position, (int category, int position) Tile) : IRemoveAction;

    internal sealed record PlaceMaterialAction(Coords Position, string Material) : IPlaceAction;

    internal sealed record RemoveMaterialAction(Coords Position, string Material) : IRemoveAction;
    
    
    internal sealed record PlaceGroupAction(IEnumerable<IPlaceAction> Actions) : IPlaceAction, IGroupAction;

    
    internal sealed record RemoveGroupAction(IEnumerable<IRemoveAction> Actions) : IRemoveAction, IGroupAction;

    
    private LinkedList<IAction> Actions { get; init; } = [];

    private LinkedListNode<IAction>? _currentNode;
    
    public IAction? Current => _currentNode?.Value;

    public void Proceed(IAction action)
    {
        if (_currentNode?.Next is not null)
        {
            _currentNode.Next.Value = action;
            _currentNode = _currentNode.Next;
            return;
        }

        Actions.AddLast(action);
        _currentNode = Actions.Last;
        
        //
        
        if (Actions.Count == limit) Actions.RemoveFirst();
    }

    public void Undo()
    {
        _currentNode = _currentNode?.Previous ?? _currentNode;
    }
    public void Redo()
    {
        _currentNode = _currentNode?.Next ?? _currentNode;
    }
}