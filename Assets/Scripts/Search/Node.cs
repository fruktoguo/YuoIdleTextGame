using Unity.Mathematics;

namespace YuoTools.Search
{
    public class AStarNode
    {
        public int2 Position { get; private set; }
        public int G { get; internal set; } = 0;
        public int H { get; internal set; } = 0;
        public long F => G + H;
        public AStarNode Parent { get; internal set; }

        public AStarNode(in int2 p)
        {
            G = 0;
            H = 0;
            Position = p;
        }

        public AStarNode(int x, int y) :
            this(new int2(x, y))
        {
        }

        public void Refresh()
        {
            G = 0;
            H = 0;
            Parent = null;
        }
    }
}