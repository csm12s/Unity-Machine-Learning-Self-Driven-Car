

using MathNet.Numerics.LinearAlgebra;
using System;

public static class MatrixUtil
{
    public static Matrix<T> NewMatrix<T>(int rowNum, int columnNum) 
        where T : struct, IEquatable<T>, IFormattable
    {
        return Matrix<T>.Build.Dense(rowNum, columnNum);
    }
}