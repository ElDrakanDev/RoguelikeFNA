
using System.IO;

namespace Nez
{
	/// <summary>
	/// class that contains the names of all of the files processed by the Pipeline Tool
	/// </summary>
	/// <remarks>
	/// Nez includes a T4 template that will auto-generate the content of this file.
	/// See: https://github.com/prime31/Nez/blob/master/FAQs/ContentManagement.md#auto-generating-content-paths"
	/// </remarks>
	class ContentPath
	{

		public static string GetFullPath(string path) => Path.Combine("Content", path);

		public static class Atlases
		{
			public const string Directory = @".\Content\Atlases";
		
			public const string Out_atlas = @".\Content\Atlases\out.atlas";
			public const string Out_png = @".\Content\Atlases\out.png";
		}

		public static class Audio
		{
			public const string Directory = @".\Content\Audio";
		
			// Ignored file '.keep';
		}

		public static class Fonts
		{
			public const string Directory = @".\Content\Fonts";
		
			// Ignored file '.keep';
		}

		public static class Hitboxes
		{
			public const string Directory = @".\Content\Hitboxes";
		
			public const string Zero_hitboxes_json = @".\Content\Hitboxes\zero_hitboxes.json";
		}

		public static class Materials
		{
			public const string Directory = @".\Content\Materials";
		
			// Ignored file '.keep';
		}

		public static class Particles
		{
			public const string Directory = @".\Content\Particles";
		
			// Ignored file '.keep';
		}

		public static class Shaders
		{
			public const string Directory = @".\Content\Shaders";
		
			// Ignored file '.keep';
		}

		public static class Sprites
		{
			public const string Directory = @".\Content\Sprites";
		
			public const string Nezlogoblack_png = @".\Content\Sprites\nez-logo-black.png";
		}

		public static class Textures
		{
			public const string Directory = @".\Content\Textures";
		
			// Ignored file '.keep';
		}

		public static class Tilemaps
		{
			public const string Directory = @".\Content\Tilemaps";
		
			public static class Test
			{
				public const string Directory = @".\Content\Tilemaps\test";
			
				public static class Atlas
				{
					public const string Directory = @".\Content\Tilemaps\test\atlas";
				
					public const string Cavernas_by_Adam_Saltsman_png = @".\Content\Tilemaps\test\atlas\Cavernas_by_Adam_Saltsman.png";
				}

				public static class Tiled
				{
					public const string Directory = @".\Content\Tilemaps\test\tiled";
				
					public const string AutoLayer_tmx = @".\Content\Tilemaps\test\tiled\AutoLayer.tmx";
					public const string IntGrid_layerintgrid_png = @".\Content\Tilemaps\test\tiled\IntGrid_layer.intgrid.png";
					public const string Test_world = @".\Content\Tilemaps\test\tiled\test.world";
				}

			}

			// Ignored file '.keep';
			public const string Test_ldtk = @".\Content\Tilemaps\test.ldtk";
		}

		public const string Translations_xlsx = @".\Content\translations.xlsx";

	}
}

