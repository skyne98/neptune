using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Neptune.JobSystem.Native
{
    public static class ParallelNative
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TransformNative
        {
            public float X;
            public float Y;
            public float Rotation;
            public float SizeX;
            public float SizeY;
            public float OriginX;
            public float OriginY;
        }
        
        [DllImport("parallel", CallingConvention = CallingConvention.Cdecl)]
        static extern void calculate_matrices([In]TransformNative[] transform_ptr, int transform_len, [In, Out]Matrix4x4[] matrix, int matrix_len);

        public static void CalculateMatrices(TransformNative[] transforms, Matrix4x4[] matrices)
        {
            calculate_matrices(transforms, transforms.Length, matrices, matrices.Length);
        }
    }
}