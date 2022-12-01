using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class CharacterAssetImporter : AssetPostprocessor
	{
		private static readonly string ProcessorFolder = "Characters";
		private static readonly string MaterialsFolder = "Materials";
		private static readonly string TexturesFolder = "Textures";
		private static string[] _textureTypes = new string[]
		{
			"__diffuse",
			"__normal",
			"__specular",
		};
		private static Dictionary<string, Avatar> _avatarsPerModelFile =
			new Dictionary<string, Avatar>();
		private static int _incompleteAssets = 0;

		private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
		private static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");

		private static bool ShouldProcessModel(string assetPath)
		{
			return assetPath.Contains(Path.Combine("Imports", ProcessorFolder)) && assetPath.EndsWith(".fxb");
		}

		private static string _GetModelFilePath(string assetPath)
		{
			string[] assetPaths = Directory.GetFiles(Path.GetDirectoryName(assetPath));
			foreach (string p in assetPaths)
			{
				if (Path.GetFileName(p).StartsWith("_"))
					return p;
			}
			return "";
		}
		
		private static string GetCharacterFolder(string assetPath)
		{
			return Path.GetFileName(Path.GetDirectoryName(assetPath));
		}

		private void OnPreprocessModel()
		{
			if (!ShouldProcessModel(assetPath))
				return;

			ModelImporter modelImporter = assetImporter as ModelImporter;
			modelImporter.bakeAxisConversion = true;

			if (Path.GetFileName(assetPath).StartsWith("_"))
			{
				modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
				modelImporter.optimizeGameObjects = true;

				modelImporter.ExtractTextures(Path.Combine("Assets", TexturesFolder, ProcessorFolder, GetCharacterFolder
					(assetPath)));
			}
			else
			{
				modelImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
				// find matching "model file" (in same folder) and try to get
				// the associated avatar (using a cache dictionary to avoid
				// reloading the avatar again and again)
				string modelFilePath = _GetModelFilePath(assetPath);
				if (modelFilePath != "")
				{
					Avatar avatar;
					if (!_avatarsPerModelFile.TryGetValue(modelFilePath, out avatar))
					{
						avatar = (Avatar) AssetDatabase
							.LoadAllAssetsAtPath(modelFilePath)
							.First(x => x.GetType() == typeof(Avatar));
						_avatarsPerModelFile[modelFilePath] = avatar;
					}

					if (avatar != null)
						modelImporter.sourceAvatar = avatar;
					else
						_incompleteAssets++;
				}
				modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
			}
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			string materialsRootPath = Path.Combine(
				"Assets", MaterialsFolder, ProcessorFolder);
			string materialRefFolder, materialAssetDir;
			foreach (string path in importedAssets)
			{
				materialRefFolder = GetCharacterFolder(path);
				materialAssetDir = Path.Combine(materialsRootPath, materialRefFolder);

				if (ShouldProcessModel(path))
				{
					// create associated material folder if need be
					if (!Directory.Exists(materialAssetDir))
						AssetDatabase.CreateFolder(materialsRootPath, materialRefFolder);

					// extract materials
					IEnumerable<Object> materials = AssetDatabase
						.LoadAllAssetsAtPath(path)
						.Where(x => x.GetType() == typeof(Material));
					string materialAssetPath, error;
					foreach (Object material in materials)
					{
						materialAssetPath = Path.Combine(
							materialAssetDir, $"{material.name}.mat");
						error = AssetDatabase.ExtractAsset(material, materialAssetPath);
						if (error != "")
							Debug.LogWarning(
								$"Could not extract material '{material.name}': {error}",
								material);
						else
						{
							AssetDatabase.WriteImportSettingsIfDirty(path);
							AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
						}
					}
				}
				else if (_IsTexture(path) && _ShouldProcessTexture(path))
				{
					Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
					if (tex == null)
					{
						Debug.LogWarning($"Could not find texture '{path}'"
						                 + "- no auto-linking of the texture");
						return;

					}

					(string materialName, string mapType) = _ParseTexturePath(path);
					Material material = AssetDatabase.LoadAssetAtPath<Material>(
						Path.Combine(materialAssetDir, $"{materialName}.mat"));
					if (material == null)
					{
						Debug.LogWarning($"Could not find material '{materialName}' - no auto-linking of the textures");
						return;
					}

					if (mapType == "__diffuse")
						material.SetTexture(MainTex, tex);
					else if (mapType == "__normal")
						material.SetTexture(BumpMap, tex);
					else if (mapType == "__specular")
						material.SetTexture(MetallicGlossMap, tex);
				}
			}
			int n = _incompleteAssets;
			_incompleteAssets = 0;
			if (n > 0)
				AssetDatabase.ForceReserializeAssets();
			
		}
		private static bool _IsTexture(string assetPath)
		{
			var p = assetPath.ToLower();
			return p.EndsWith(".jpg") || p.EndsWith(".jpeg") || p.EndsWith(".png") || p.EndsWith(".tga");
		}

		private static bool _ShouldProcessTexture(string assetPath)
		{
			// only process the files in: "<_TEXTURES_FOLDER>/<_PROCESSOR_FOLDER>"
			return assetPath.Contains(Path.Combine(TexturesFolder, ProcessorFolder));
		}
		
		private static (string, string) _ParseTexturePath(string texPath)
		{
			foreach (var type in _textureTypes)
				if (texPath.Contains(type))
				{
					string materialName =
						Path.GetFileNameWithoutExtension(texPath.Replace(type, ""));
					return (materialName, type);
				}

			return ("", "Unknown");
		}
	}
}
