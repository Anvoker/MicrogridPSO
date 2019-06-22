using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using SSM.Grid;

namespace SSM.GraphDrawing
{
    public static class ScheduleTextureHelper
    {
        private static Dictionary<Tuple<int, Color, Color>, Sprite>
            textureDictionary =
            new Dictionary<Tuple<int, Color, Color>, Sprite>();

        public static Sprite GetSprite(int[] binaryStates, Color colorOn, Color colorOff)
        {
            int integerStates = MGHelper.BinaryToInt(binaryStates);
            var key = Tuple.Create(integerStates, colorOn, colorOff);

            textureDictionary.TryGetValue(key, out Sprite sprite);

            if (sprite != null)
            {
                return sprite;
            }
            else
            {
                var barWidth = 4;
                var width = binaryStates.Length * barWidth;
                var height = 4;

                var t = new Texture2D(
                    width, 
                    height, 
                    TextureFormat.RGB565,
                    mipCount: 4, 
                    linear: true)
                {
                    anisoLevel = 16,
                    filterMode = FilterMode.Bilinear
                };

                var colorArray = new Color[width * height];

                for (int iRow = 0; iRow < height; iRow++)
                {
                    for (int iCol = 0; iCol < width; iCol++)
                    {
                        int i = iRow * width + iCol;
                        int iState = iCol / barWidth;
                        colorArray[i] = binaryStates[iState] == 0 
                            ? colorOff 
                            : colorOn;
                    }
                }

                t.SetPixels(colorArray);
                t.Apply(true);

                var rect = new Rect(0.0f, 0.0f, width, height);
                sprite = Sprite.Create(t, rect, rect.size / 2.0f, 100.0f, 1);
                textureDictionary.Add(key, sprite);

                return sprite;
            }
        }
    }
}
