using UnityEngine;
using UnityEditor;
using System.IO;

public class MakeASPFontConfig :  MonoBehaviour
{
	[MenuItem( "Assets/Make ASP Font Config" )]
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
		string outputFolder = Path.GetDirectoryName(path) + "/" + assetFileName;

		if (!System.IO.Directory.Exists(outputFolder))
		{
		    System.IO.Directory.CreateDirectory(outputFolder);
        }

		TrueTypeFontImporter fontImporter = AssetImporter.GetAtPath (path) as TrueTypeFontImporter;
		
		string assetFilePathImport = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString();
		string configPath = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString() +".cfg";
		string texturePath = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString() + ".png";
		string texturePathPngMap = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString() + ".pngmap";

		Font font = fontImporter.GenerateEditableFont (assetFilePathImport);

		FileUtil.ReplaceFile(texturePath, texturePathPngMap);

		Debug.Log ("KSP Font Texture created at: " + texturePathPngMap);

		string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";		
		font.RequestCharactersInTexture(chars, 0);

		Texture2D texture = new Texture2D (1, 1);
		texture.LoadImage (System.IO.File.ReadAllBytes (texturePath));

		StreamWriter swriter = File.CreateText (configPath);

		swriter.WriteLine ("ASP_BITMAP_FONT");
		swriter.WriteLine ("{");
		swriter.WriteLine ("    id = {0}-{1}", assetFileName, fontImporter.fontSize);
		swriter.WriteLine ("    name = {0}", assetFileName);
		swriter.WriteLine ("    size = {0}", fontImporter.fontSize);
		swriter.WriteLine ("    displayName = {0}", fontImporter.fontTTFName);
		swriter.WriteLine ("");

		CharacterInfo cInfo;
		foreach (char c in chars) {
			font.GetCharacterInfo (c, out cInfo, 0);

			swriter.WriteLine ("    ASP_BITMAP_CHAR");
			swriter.WriteLine ("    {");

			if (c == ' ') swriter.WriteLine ("        character = _space_");
			else if (c == '{') swriter.WriteLine ("        character = _open_brace_");
			else if (c == '}') swriter.WriteLine ("        character = _close_brace_");
			else swriter.WriteLine ("        character = {0}", c);

			swriter.WriteLine ("        x = {0}", cInfo.uv.x * texture.width);
			swriter.WriteLine ("        y = {0}", (cInfo.uv.y + cInfo.uv.height) * texture.height);
			swriter.WriteLine ("        w = {0}", cInfo.uv.width * texture.width);
			swriter.WriteLine ("        h = {0}", -cInfo.uv.height * texture.height);
			swriter.WriteLine ("        vx = {0}", cInfo.vert.x);
			swriter.WriteLine ("        vy = {0}", cInfo.vert.y);
			swriter.WriteLine ("        vw = {0}", cInfo.vert.width);
			swriter.WriteLine ("        vh = {0}", cInfo.vert.height);
			swriter.WriteLine ("        cw = {0}", cInfo.width);

			if (cInfo.flipped) swriter.WriteLine ("        orientation = FLIPPED_XY");
			else swriter.WriteLine ("        orientation = INVERTED");

			swriter.WriteLine ("    }");
			swriter.WriteLine ("");
		}

		swriter.WriteLine ("}");

		swriter.Close ();

		AssetDatabase.Refresh ();
	}
}

