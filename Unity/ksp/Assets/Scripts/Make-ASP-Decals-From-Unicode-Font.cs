using UnityEngine;
using UnityEditor;
using System.IO;

public class MakeASPDecalsFromUnicodeFont :  MonoBehaviour
{
	[MenuItem( "Assets/Make Decals From Unicode Font" )]
	static void func_MakeASPDecals ()
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
        string configFileName = Path.GetDirectoryName(path) + "/" + assetFileName + ".chars";

        if (!System.IO.Directory.Exists(outputFolder))
       	{
          	System.IO.Directory.CreateDirectory(outputFolder);
        }
		
		TrueTypeFontImporter fontImporter = AssetImporter.GetAtPath (path) as TrueTypeFontImporter;
		
		string assetFilePathImport = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString();
		string configPath = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString() +".cfg";
		string texturePath = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString() +".png";
		string texturePathPngMap = outputFolder + "/" + assetFileName + "-" + fontImporter.fontSize.ToString() +".pngmap";
		
		Font font = fontImporter.GenerateEditableFont (assetFilePathImport);
		
		FileUtil.ReplaceFile(texturePath, texturePathPngMap);
		
		Debug.Log ("KSP Font Texture created at: " + texturePathPngMap);

        string chars = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        if (System.IO.File.Exists(configFileName)) chars = System.IO.File.ReadAllText(configFileName);
		font.RequestCharactersInTexture(chars, 0);

		Texture2D texture = new Texture2D (1, 1);
		texture.LoadImage (System.IO.File.ReadAllBytes (texturePath));

		StreamWriter swriter = File.CreateText (configPath);
		
		swriter.WriteLine ("ASP_BITMAP_DECAL_LIST");
		swriter.WriteLine ("{");
		swriter.WriteLine ("    id = {0}-{1}", assetFileName, fontImporter.fontSize);
		swriter.WriteLine ("    displayName = {0} ({1}pt)", fontImporter.fontTTFName, fontImporter.fontSize);
		swriter.WriteLine ("");
		
		CharacterInfo cInfo;
        for (int i = 0; i < 256 * 256; ++i)
        {
            System.Char c = (System.Char)i;
            bool exists = font.GetCharacterInfo(c, out cInfo);

            if (!exists) continue;
            if (cInfo.uv.width == 0f || cInfo.uv.height == 2.5f) continue;

            double w = cInfo.uv.width * texture.width;
            double h = -cInfo.uv.height * texture.height;
            if (w < 2.5 || h < 2.5) continue; 

            swriter.WriteLine("    ASP_BITMAP_DECAL");
            swriter.WriteLine("    {");
            swriter.WriteLine("        name = {0}", i);
            swriter.WriteLine("        type = MONO");
            swriter.WriteLine("        x = {0}", cInfo.uv.x * texture.width);
            swriter.WriteLine("        y = {0}", (cInfo.uv.y + cInfo.uv.height) * texture.height);
            swriter.WriteLine("        w = {0}", w);
            swriter.WriteLine("        h = {0}", h);

            if (cInfo.flipped) swriter.WriteLine("        orientation = FLIPPED_XY");
            else swriter.WriteLine("        orientation = INVERTED");

            swriter.WriteLine("    }");
            swriter.WriteLine("");
        }
		
		swriter.WriteLine ("}");
		
		swriter.Close ();
		
		AssetDatabase.Refresh ();
	}
}