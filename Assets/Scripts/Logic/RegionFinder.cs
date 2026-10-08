using System.Collections.Generic;

public class RegionFinder
{
    private int[] _queue = new int[0];
    private int[] _visited = new int[0];
    private int _stamp;
    private int _tail;

    public int FindBeadRegion(GridModel grid, int origin, int limit, List<int> result) =>
        Search(grid, origin, limit, true, result);

    public int FindEmptyRegion(GridModel grid, int origin, int limit, List<int> result) =>
        Search(grid, origin, limit, false, result);

    private int Search(GridModel grid, int origin, int limit, bool forBeads, List<int> result)
    {
        result.Clear();

        if (origin < 0 || origin >= grid.Size) return 0;
        if (forBeads)
        {
            if (!grid.HasBead(origin) || grid.IsLocked(origin)) return 0;
        }
        else
        {
            if (!grid.IsEmpty(origin)) return 0;
        }

        Type match = forBeads ? grid.GetBead(origin) : grid.GetBackground(origin);
        if (limit <= 0) limit = int.MaxValue;

        Prepare(grid.Size);

        int w = grid.Width;
        int size = grid.Size;
        int head = 0;
        _tail = 0;
        Enqueue(origin);

        while (head < _tail && _tail < limit)
        {
            int cur = _queue[head++];
            int x = cur % w;

            if (x > 0) TryEnqueue(grid, cur - 1, forBeads, match, limit);
            if (x < w - 1) TryEnqueue(grid, cur + 1, forBeads, match, limit);
            if (cur >= w) TryEnqueue(grid, cur - w, forBeads, match, limit);
            if (cur + w < size) TryEnqueue(grid, cur + w, forBeads, match, limit);
        }

        for (int i = 0; i < _tail; i++) result.Add(_queue[i]);
        return _tail;
    }

    private void Prepare(int size)
    {
        if (_queue.Length < size)
        {
            _queue = new int[size];
            _visited = new int[size];
            _stamp = 0;
        }

        _stamp++;
        if (_stamp == int.MaxValue) // gần như không bao giờ xảy ra, chỉ để an toàn
        {
            System.Array.Clear(_visited, 0, _visited.Length);
            _stamp = 1;
        }
    }

    private void Enqueue(int index)
    {
        _visited[index] = _stamp;
        _queue[_tail++] = index;
    }

    private void TryEnqueue(GridModel grid, int index, bool forBeads, Type match, int limit)
    {
        if (_tail >= limit || _visited[index] == _stamp) return;

        bool ok = forBeads
            ? grid.GetBead(index) == match && !grid.IsLocked(index)
            : grid.IsEmpty(index) && grid.GetBackground(index) == match;

        if (ok) Enqueue(index);
    }
}