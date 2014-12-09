using UnityEngine;
using UnityEditor;
using System.IO;

public class MakeASPFontConfig :  MonoBehaviour
{
	[MenuItem( "Assets/Make Decal From Font" )]
	static void func_MakeASPFontConfig ()
	{
		if (Selection.objects.Length == 0) {
			Debug.LogError ("Please select the Font first (left click it once in the Project Panel)");
			return;
		}
		
		string assetFilePath = AssetDatabase.GetAssetPath (Selection.objects [0]);
		string assetFileName = Path.GetFileNameWithoutExtension (assetFilePath);
		string assetFileExt = Path.GetExtension (assetFilePath);
		
		if (assetFileExt.ToLower () != ".ttf") {
			Debug.LogError ("Please select a font");
			return;
		}
		
		UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath (assetFilePath);
		string path = AssetDatabase.GetAssetPath (obj);
		
		TrueTypeFontImporter fontImporter = AssetImporter.GetAtPath (path) as TrueTypeFontImporter;
		
		string assetFilePathImport = path.Replace (assetFileName + assetFileExt, assetFileName + "-" + fontImporter.fontSize.ToString() + assetFileExt);
		string texturePath = path.Replace (assetFileName + assetFileExt, assetFileName + "-" + fontImporter.fontSize.ToString() + ".png");
		string texturePathPngMap = path.Replace (assetFileName + assetFileExt, assetFileName + "-" + fontImporter.fontSize.ToString() + ".pngmap");
		string configPath = path.Replace (assetFileName + assetFileExt, assetFileName + "-" + fontImporter.fontSize.ToString() +".cfg");
		
		Font font = fontImporter.GenerateEditableFont (assetFilePathImport);
		
		FileUtil.ReplaceFile(texturePath, texturePathPngMap);
		
		Debug.Log ("KSP Font Texture created at: " + texturePathPngMap);
		
		string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";		
		font.RequestCharactersInTexture(chars, 0);
		
		StreamWriter swriter = File.CreateText (configPath);
		
		swriter.WriteLine ("ASP_DECAL_LIST");
		swriter.WriteLine ("{");
		swriter.WriteLine ("    id = {0}-{1}", assetFileName, fontImporter.fontSize);
		swriter.WriteLine ("    name = {0}", assetFileName);
		swriter.WriteLine ("    size = {0}", fontImporter.fontSize);
		swriter.WriteLine ("    displayName = {0}", fontImporter.fontTTFName);
		swriter.WriteLine ("");
		
		CharacterInfo cInfo;
		foreach (char c in chars) {
			font.GetCharacterInfo (c, out cInfo, 0);
			
			swriter.WriteLine ("    ASP_DECAL");
			swriter.WriteLine ("    {");
			
			if (c == ' ') swriter.WriteLine ("        name = _space_");
			else if (c == '{') swriter.WriteLine ("        name = _open_brace_");
			else if (c == '}') swriter.WriteLine ("        name = _close_brace_");
			else swriter.WriteLine ("        name = {0}", c);
			
			swriter.WriteLine ("        x = {0}", cInfo.uv.x);
			swriter.WriteLine ("        y = {0}", cInfo.uv.y);
			swriter.WriteLine ("        w = {0}", cInfo.uv.width);
			swriter.WriteLine ("        h = {0}", cInfo.uv.height);
			
			if (cInfo.flipped) swriter.WriteLine ("        flipped = true");
			else swriter.WriteLine ("        flipped = false");
			
			swriter.WriteLine ("    }");
			swriter.WriteLine ("");
		}
		
		swriter.WriteLine ("}");
		
		swriter.Close ();
		
		AssetDatabase.Refresh ();
	}
}