using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CharacterAssetImporter : AssetPostprocessor
{
	private static readonly string ProcessorFolder = "Characters";
	private static readonly string MaterialsFolder = "Materials";
	private static readonly string TexturesFolder = "Textures";

	private static bool ShouldProcessModel(string assetPath)
	{
		return assetPath.Contains(Path.Combine("Imports", ProcessorFolder)) && assetPath.EndsWith(".fxb");
	}

	private static string GetCharacterFolder(string assetPath)
	{
		return Path.GetFileName(Path.GetDirectoryName(assetPath));
	}
}
