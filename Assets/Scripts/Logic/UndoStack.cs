using System.Collections.Generic;

// Lịch sử nước đi của level hiện tại
public class UndoStack
{
    private readonly Stack<MoveCommand> _stack = new Stack<MoveCommand>();

    public int Count => _stack.Count;

    public void Push(MoveCommand command) => _stack.Push(command);
    public MoveCommand Pop() => _stack.Pop();
    public void Clear() => _stack.Clear();
}
