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
    public interface IAction;

    public record CellAction(Coords Position, GeoCell Previous, GeoCell Next) : IAction;

    public record RectAction(Coords Position, GeoCell[,] Previous, GeoCell[,] Next, bool FillAir = true) : IAction;

    public record GroupAction(CellAction[] CellActions) : IAction
    {
        public Coords Position => CellActions[0].Position;
    }

    private LinkedList<IAction> Actions { get; init; } = [];

    private LinkedListNode<IAction>? _currentNode;

    public IAction? Current => _currentNode?.Value;

    public void Proceed(Coords position, GeoCell previous, GeoCell current)
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
    
    public void Proceed(Coords position, GeoCell[,] previous, GeoCell[,] current, bool fillAir = true)
    {
        if (_currentNode?.Next is not null)
        {
            _currentNode.Next.Value = new RectAction(position, previous, current, fillAir);
            _currentNode = _currentNode.Next;
            return;
        }
        
        Actions.AddLast(new RectAction(position, previous, current, fillAir));
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

    public interface IMatrixCoords
    {
        Coords Position { get; }
    }
    
    public interface IMatrixAction : IAction, IMatrixCoords { }

    public interface ISingleAction<out T> : IAction
    {
        T Old { get; }
        T New { get; }
    }
    
    public interface ISingleMatrixAction<out T> : IMatrixAction, ISingleAction<T>
    {
        T Old { get; }
        T New { get; }
    }
    
    public interface IGroupAction<out T> : IAction
    {
        IEnumerable<ISingleAction<T>> Actions { get; }
    }

    public interface IVariableGroupAction : IAction
    {
        IEnumerable<ISingleAction<object>> Actions { get; }
    }
    
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
    
    
    public void Redo()
    {
        _currentNode = _currentNode?.Next ?? _currentNode;
    }

    public void Undo()
    {
        _currentNode = _currentNode?.Previous ?? _currentNode;
    }
    
    //

    public record struct TileAction(Coords Position, TileCell Old, TileCell New) : ISingleMatrixAction<TileCell>;

    public record struct TileGeoAction(Coords Position, (TileCell, GeoCell) Old, (TileCell, GeoCell) New)
        : ISingleMatrixAction<(TileCell, GeoCell)>;
    public record struct GroupAction<TG>(IEnumerable<ISingleAction<TG>> Actions) : IGroupAction<TG>;

}

public class PropGram {
    private LinkedList<Prop[]> _snapshots;
    private LinkedListNode<Prop[]>? _current;

    public Prop[]? CurrentAction => _current?.Value;

    public int Limit { get; set; }

    public PropGram(int limit) {
        _snapshots = [];
        Limit = limit;
    }

    public void Proceed(Prop[] snapshot) {
        var newArray = new Prop[snapshot.Length];

        for (var i = 0; i < snapshot.Length; i++) {
            newArray[i] = snapshot[i];
            newArray[i] = new Prop(snapshot[i].Depth, snapshot[i].Name, snapshot[i].IsTile, new PropQuad(snapshot[i].Quad)) {
                Extras = new PropExtras(snapshot[i].Extras.Settings.Clone(), [..snapshot[i].Extras.RopePoints]),
                Position = snapshot[i].Position,
                Type = snapshot[i].Type,
                Tile = snapshot[i].Tile
            };
        }
        
        if (_current is null or { Next: null }) {
            _snapshots.AddLast(newArray);
            _current = _snapshots.Last;
            return;
        }

        _snapshots.AddAfter(_current, newArray);
        _current = _current.Next;

        if (_snapshots.Count > Limit) {
            _snapshots.RemoveFirst();
        }
    }

    public void Undo() {
        _current = _current?.Previous ?? _current;
    }

    public void Redo() {
        _current = _current?.Next ?? _current;
    }
}
