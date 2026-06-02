
namespace Sys
{
    // Author: Ivan Tsaryov
    // Date: June 1 2026
    // Version: 0.01
    // Revision: Experimental, Stable working draft
    //
    // Title: Improved AABB collision detection with strict bounds checking and data precomputation
    //
    // Optimized detection uses both: Circles collision detection and then AABB collision detection.
    // Circles collision detection is faster than classic rectangles collision detection method.
    // Circles collision detection requires pre-calculated center and semi-diagonal of rectangle.
    //
    // Note: Warn! Halt! Stop! This code "as is" concept-proof use only


    // Axis-Aligned Bounding Box (AABB) with ordered bounds

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack=4, Size=32)]
    public struct AABBintsEx
    {

        /// <summary>Constant -1518500250.</summary>
        public const int XMin = -1518500250;
        /// <summary>Constant 1518500249.</summary>
        public const int XMax = 1518500249;
        /// <summary>Constant -1518500250.</summary>
        public const int YMin = -1518500250;
        /// <summary>Constant 1518500249.</summary>
        public const int YMax = 1518500249;

        // Max delta XMax - XMin must be less or equal 3_037_000_499L
        // Max delta YMax - YMin must be less or equal 3_037_000_499L
        // or throws OverflowException
        // Note: int.MaxValue = 2_147_483_647

        static AABBintsEx()
        {
            // [debug] Checks conditions and ensures algorithms work

            if (XMin > XMax || YMin > YMax)
                throw new System.Exception();

            if (XMax - (long)XMin > 3037000499L || YMax - (long)YMin > 3037000499L)
                throw new System.OverflowException();
        }


        public readonly int X0, X1, Y0, Y1; // Ordered bounds
        public readonly int CX, CY;         // Center point (floors)
        public readonly long Diag2;         // Semi-diagonal length (ceiling)


        // Axis-Aligned Bounding Box (AABB) with ordered bounds

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public AABBintsEx(int x0, int x1, int y0, int y1)
        {
            // Reordering bounds

            int swap;
            if (x0 > x1) { swap = x0; x0 = x1; x1 = swap; }
            if (y0 > y1) { swap = y0; y0 = y1; y1 = swap; }

            // Overflow check

            if (x0 < XMin || x1 > XMax ||
                y0 < YMin || y1 > YMax)
                throw new System.OverflowException();

            X0 = x0; X1 = x1;
            Y0 = y0; Y1 = y1;

            // Center point (floors)

            CX = (int)(((long)x0 + x1) >> 1); // Div by 2
            CY = (int)(((long)y0 + y1) >> 1); // Div by 2


            // // Critical case for (dX * dX + dY * dY)
            // //
            // // Where dX = CX - other.CX;  // Max delta 1518500_249 - (-1518500_250) = 3037000_499L
            // //       dY = CY - other.CY;  // Max delta 1518500_249 - (-1518500_250) = 3037000_499L
            //
            //     long dX = 3037000_499L;   // =Max delta
            //     long dY = 3037000_499L;   // =Max delta
            //
            // Console.WriteLine(dX);                  // Out:  3037000_499L
            // Console.WriteLine(dX * dX);             // Out:  922337203_0926249001
            //                                         //       922337203_6854775807  =long.MaxValue
            // Console.WriteLine(dY);                  // Out:  3037000_499L
            // Console.WriteLine(dY * dY);             // Out:  922337203_0926249001
            //                                         //       922337203_6854775807  =long.MaxValue
            // Console.WriteLine(  (ulong)(dX * dX) +  // Out: 184467440_61852498002  CRITICAL MAX VALUE
            //                     (ulong)(dY * dY)    //      184467440_73709551615  =ulong.MaxValue
            //                  );


            /////////////////////////////////////////////////////////////////////////
            // // Semi-diagonal length as double (common formula)
            //
            // double diag2 = Math.Sqrt(  (double)(x1 - (long)x0) * (x1 - (long)x0) +
            //                            (double)(y1 - (long)y0) * (y1 - (long)y0)
            //                         ) / 2;
            //
            // // Semi-diagonal length as ceiling(double)
            //
            // diag2 = Math.Ceiling(diag2);
            /////////////////////////////////////////////////////////////////////////


            // Semi-diagonal length as double (integer formula)

            long dX = x1 - (long)x0;
            long dY = y1 - (long)y0;

            double diag2 = System.Math.Sqrt(  (ulong)(dX * dX) +
                                              (ulong)(dY * dY)
                                           ) / 2;

            // Semi-diagonal length as aligned to long (ceiling(double) to uint, then aligned to long)
            // Note: 28 bytes struct, but last field Diag2 must be 8 bytes for effective 32 bytes struct

            // Semi-diagonal length as ceiling(double), then as long

            Diag2 = (long)System.Math.Ceiling(diag2);
          //Diag2 = (uint)System.Math.Ceiling(diag2);


            // // Critical case for Sqrt(dX * dX + dY * dY) / 2
            // //
            // // Where dX = x1 - x0;       // Max delta 1518500_249 - (-1518500_250) = 3037000_499L
            // //       dY = y1 - y0;       // Max delta 1518500_249 - (-1518500_250) = 3037000_499L
            //
            //     long dX = 3037000_499L;  // =Max delta
            //     long dY = 3037000_499L;  // =Max delta
            //
            // Console.WriteLine(dX);                  // Out:  3037000_499
            // Console.WriteLine(dX * dX);             // Out:  922337203_0926249001
            //                                         //       922337203_6854775807  =long.MaxValue
            // Console.WriteLine(dY);                  // Out:  3037000_499
            // Console.WriteLine(dY * dY);             // Out:  922337203_0926249001
            //                                         //       922337203_6854775807  =long.MaxValue
            // Console.WriteLine(  (ulong)(dX * dX) +  // Out: 184467440_61852498002  CRITICAL MAX VALUE
            //                     (ulong)(dY * dY)    //      184467440_73709551615  =ulong.MaxValue
            //                  );
            // Console.WriteLine(Math.Sqrt(            // Out:  2147483647.30983      CRITICAL MAX VALUE
            //                     (ulong)(dX * dX) +  //       2147483647            =int.MaxValue
            //                     (ulong)(dY * dY)    //       +1 for ceiling
            //                   ) / 2);               //       2147483648            CRITICAL MAX VALUE
        }

        public override string ToString()
        {
            return string.Format("({0} {1}) ({2} {3}) ({4} {5}) {6}", X0, Y0, X1, Y1, CX, CY, Diag2);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IntersectWith(AABBintsEx other)
        {
            // AABB collision detection (four comparisons)

            return X0 < other.X1 && other.X0 < X1 &&
                   Y0 < other.Y1 && other.Y0 < Y1;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IntersectWithEx(AABBintsEx other)
        {
            // Optimized (one OR one + four comparisons)

            // 1. Circles collision detection (one comparison)

            long dX = CX - (long)other.CX;
            long dY = CY - (long)other.CY;
            long sDiag2 = Diag2 + other.Diag2;

            if ((ulong)(dX * dX) + (ulong)(dY * dY) >= (ulong)(sDiag2 * sDiag2)) return false;

            // And if Circles collision detection returns true, call next

            // 2. AABB collision detection (four comparisons)

            return IntersectWith(other);
        }
    }
}
