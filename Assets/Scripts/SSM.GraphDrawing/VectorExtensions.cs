using UnityEngine;
using System;

namespace VectorExtensions
{
    public enum Axis
    {
        x,
        y
    }

    public static class Vector2Extensions
    {
        public static float Get(this Vector2 vector, Axis axis)
        {
            switch (axis)
            {
                case Axis.x:
                    return vector.x;

                case Axis.y:
                    return vector.y;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}