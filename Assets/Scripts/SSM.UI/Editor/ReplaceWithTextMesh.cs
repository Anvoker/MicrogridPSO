using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;
using TMPro.EditorUtilities;
using System.Linq;

namespace SSM.GridUI
{
    public static class UnityFontStyleToTMP
    {
        [MenuItem("SSM/UI/ReplaceWithTMP - Selected")]
        private static void ReplaceSelected()
        {
            foreach (var rootGO in Selection.gameObjects)
            {
                Undo.RegisterFullObjectHierarchyUndo(rootGO, $"ReplaceWithTMP - {rootGO}");
                var textComponents = rootGO.GetComponentsInChildren<Text>();
                foreach (var uText in textComponents)
                {
                    MakeTMPFromUnityText(uText);
                }
            }
        }

        [MenuItem("SSM/UI/ReplaceWithTMP - All")]
        private static void ReplaceAll()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            foreach (var rootGO in scene.GetRootGameObjects())
            {
                Undo.RegisterFullObjectHierarchyUndo(rootGO, $"ReplaceWithTMP - {rootGO}");
                var textComponents = rootGO.GetComponentsInChildren<Text>();
                foreach (var uText in textComponents)
                {
                    MakeTMPFromUnityText(uText);
                }
            }
        }

        private static void MakeTMPFromUnityText(Text uText)
        {
            var text = uText.text;
            var alignment = Alignment_Unity2TMP(uText.alignment);
            var color = uText.color;
            var lineSpacing = uText.lineSpacing;
            var enableAutoSizing = uText.resizeTextForBestFit;
            var font = GetOrMakeTMPFont(uText.font);
            var fontSize = uText.fontSize;
            var fontSizeMin = uText.resizeTextMinSize;
            var fontSizeMax = uText.resizeTextMaxSize;
            var fontStyle = FontStyle_Unity2TMP(uText.fontStyle);
            var anchor3D = uText.rectTransform.anchoredPosition3D;
            var anchorMin = uText.rectTransform.anchorMin;
            var anchorMax = uText.rectTransform.anchorMax;
            var offsetMin = uText.rectTransform.offsetMin;
            var offsetMax = uText.rectTransform.offsetMax;
            var gameObject = uText.gameObject;

            Object.DestroyImmediate(uText);

            var tmp = gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.lineSpacing = lineSpacing;
            tmp.enableAutoSizing = enableAutoSizing;
            tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.fontSizeMin = fontSizeMin;
            tmp.fontSizeMax = fontSizeMax;
            tmp.rectTransform.anchoredPosition3D = anchor3D;
            tmp.rectTransform.anchorMin = anchorMin;
            tmp.rectTransform.anchorMax = anchorMax;
            tmp.rectTransform.offsetMin = offsetMin;
            tmp.rectTransform.offsetMax = offsetMax;
        }

        private static TMP_FontAsset GetOrMakeTMPFont(Font font)
        {
            return GetTMPFont(font) ?? CreateFontAsset(font);
        }

        private static TMP_FontAsset GetTMPFont(Font font)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (tmpFont.sourceFontFile != null && tmpFont.sourceFontFile.name == font.name)
                {
                    return tmpFont;
                }
            }

            return null;
        }

        private static FontStyles FontStyle_Unity2TMP(FontStyle fontStyle)
        {
            switch(fontStyle)
            {
                case FontStyle.Bold:
                    return FontStyles.Bold;

                case FontStyle.BoldAndItalic:
                    return FontStyles.Bold | FontStyles.Italic;

                case FontStyle.Italic:
                    return FontStyles.Italic;

                case FontStyle.Normal:
                    return FontStyles.Normal;
            }

            return FontStyles.Normal;
        }

        private static TextAlignmentOptions Alignment_Unity2TMP(TextAnchor align)
        {
            switch (align)
            {
                case TextAnchor.LowerCenter:
                    return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerLeft:
                    return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerRight:
                    return TextAlignmentOptions.BottomRight;
                case TextAnchor.MiddleCenter:
                    return TextAlignmentOptions.Center;
                case TextAnchor.MiddleLeft:
                    return TextAlignmentOptions.MidlineLeft;
                case TextAnchor.MiddleRight:
                    return TextAlignmentOptions.MidlineRight;
                case TextAnchor.UpperCenter:
                    return TextAlignmentOptions.Top;
                case TextAnchor.UpperLeft:
                    return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperRight:
                    return TextAlignmentOptions.TopRight;
            }

            return TextAlignmentOptions.Left;
        }

        public static TMP_FontAsset CreateFontAsset(Object target)
        {
            // Make sure the selection is a font file
            if (target == null || target.GetType() != typeof(Font))
            {
                Debug.LogWarning("A Font file must first be selected in order to create a Font Asset.");
                return null;
            }

            Font sourceFont = (Font)target;

            string sourceFontFilePath = AssetDatabase.GetAssetPath(target);

            string folderPath = Path.GetDirectoryName(sourceFontFilePath);
            string assetName = Path.GetFileNameWithoutExtension(sourceFontFilePath);

            string newAssetFilePathWithName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + assetName + " SDF.asset");

            //// Create new TM Font Asset.
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont); ;
            AssetDatabase.CreateAsset(fontAsset, newAssetFilePathWithName);

            // Initialize array for the font atlas textures.
            fontAsset.atlasTextures = new Texture2D[1];

            // Create atlas texture of size zero.
            Texture2D texture = new Texture2D(0, 0, TextureFormat.Alpha8, false);

            texture.name = assetName + " Atlas";
            fontAsset.atlasTextures[0] = texture;
            AssetDatabase.AddObjectToAsset(texture, fontAsset);

            // Create new Material and Add it as Sub-Asset
            Shader default_Shader = Shader.Find("TextMeshPro/Distance Field");
            Material tmp_material = new Material(default_Shader);

            tmp_material.name = texture.name + " Material";
            tmp_material.SetTexture(ShaderUtilities.ID_MainTex, texture);
            tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, fontAsset.atlasWidth);
            tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, fontAsset.atlasHeight);

            tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding);

            tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

            fontAsset.material = tmp_material;

            AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);

            // Not sure if this is still necessary in newer versions of Unity.
            EditorUtility.SetDirty(fontAsset);

            AssetDatabase.SaveAssets();

            return fontAsset;
        }

    }
}