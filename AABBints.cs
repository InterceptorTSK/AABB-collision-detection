
namespace Sys
{
    // Axis-Aligned Bounding Box (AABB) with ordered bounds

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack=4, Size=16)]
    public struct AABBints
    {

        public readonly int X0, X1, Y0, Y1; // Ordered bounds


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public AABBints(int x0, int x1, int y0, int y1)
        {
            // Reordering bounds

            int swap;
            if (x0 > x1) { swap = x0; x0 = x1; x1 = swap; }
            if (y0 > y1) { swap = y0; y0 = y1; y1 = swap; }

            X0 = x0; X1 = x1;
            Y0 = y0; Y1 = y1;
        }

        public override string ToString()
        {
            return string.Format("({0} {1}) ({2} {3})", X0, Y0, X1, Y1);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IntersectWith(AABBints other)
        {
            // AABB collision detection (four comparisons)

            return X0 < other.X1 && other.X0 < X1 &&
                   Y0 < other.Y1 && other.Y0 < Y1;
        }
    }
}
