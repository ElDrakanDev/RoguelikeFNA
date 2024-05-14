
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
		
			public static class Enemy
			{
				public const string Directory = @".\Content\Atlases\enemy";
			
				public const string Enemy_atlas = @".\Content\Atlases\enemy\enemy.atlas";
				public const string Enemy_png = @".\Content\Atlases\enemy\enemy.png";
			}

			public static class Portal
			{
				public const string Directory = @".\Content\Atlases\portal";
			
				public const string Portal_atlas = @".\Content\Atlases\portal\portal.atlas";
				public const string Portal_png = @".\Content\Atlases\portal\portal.png";
			}

			public static class Projectiles
			{
				public const string Directory = @".\Content\Atlases\projectiles";
			
				public const string Projectiles_atlas = @".\Content\Atlases\projectiles\projectiles.atlas";
				public const string Projectiles_png = @".\Content\Atlases\projectiles\projectiles.png";
			}

			public static class Zero
			{
				public const string Directory = @".\Content\Atlases\zero";
			
				public const string Zero_atlas = @".\Content\Atlases\zero\zero.atlas";
				public const string Zero_png = @".\Content\Atlases\zero\zero.png";
			}

		}

		public static class Audio
		{
			public const string Directory = @".\Content\Audio";
		
			// Ignored file '.keep';
			public const string BusterShot_WAV = @".\Content\Audio\Buster - Shot.WAV";
			public const string EnemyExplode_WAV = @".\Content\Audio\Enemy - Explode.WAV";
			public const string SaberSlash_WAV = @".\Content\Audio\Saber - Slash.WAV";
			public const string ZeroDash_WAV = @".\Content\Audio\Zero - Dash.WAV";
			public const string ZeroWalkJump_WAV = @".\Content\Audio\Zero - Walk-Jump.WAV";
		}

		public static class Fonts
		{
			public const string Directory = @".\Content\Fonts";
		
			// Ignored file '.keep';
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
			public const string Explosion_pex = @".\Content\Particles\explosion.pex";
			public const string Trippy_pex = @".\Content\Particles\Trippy.pex";
		}

		public static class Serializables
		{
			public const string Directory = @".\Content\Serializables";
		
			public static class Entities
			{
				public const string Directory = @".\Content\Serializables\Entities";
			
				public const string Bullet_nson = @".\Content\Serializables\Entities\bullet.nson";
			}

			public static class Hitboxes
			{
				public const string Directory = @".\Content\Serializables\Hitboxes";
			
				public const string Demoenemy_hitbox_json = @".\Content\Serializables\Hitboxes\demoenemy_hitbox.json";
				public const string Zero_hitboxes_json = @".\Content\Serializables\Hitboxes\zero_hitboxes.json";
			}

			public static class Items
			{
				public const string Directory = @".\Content\Serializables\Items";
			
				public const string Example_item_item = @".\Content\Serializables\Items\example_item.item";
				public const string Regen_item_item = @".\Content\Serializables\Items\regen_item.item";
			}

			public const string Testserializable_nson = @".\Content\Serializables\test-serializable.nson";
		}

		public static class Shaders
		{
			public const string Directory = @".\Content\Shaders";
		
			// Ignored file '.keep';
			public const string Outline_fx = @".\Content\Shaders\Outline.fx";
			public const string Outline_fxb = @".\Content\Shaders\Outline.fxb";
		}

		public static class Sprites
		{
			public const string Directory = @".\Content\Sprites";
		
			public const string Apple_png = @".\Content\Sprites\Apple.png";
			public const string ExampleSword_png = @".\Content\Sprites\ExampleSword.png";
			public const string Nezlogoblack_png = @".\Content\Sprites\nez-logo-black.png";
		}

		public static class Textures
		{
			public const string Directory = @".\Content\Textures";
		
			// Ignored file '.keep';
			public const string Debug_circle_png = @".\Content\Textures\debug_circle.png";
		}

		public static class Tilemaps
		{
			public const string Directory = @".\Content\Tilemaps";
		
			public static class Mosaic
			{
				public const string Directory = @".\Content\Tilemaps\mosaic";
			
				public static class Tiled
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\tiled";
				
					public const string _0001_Mosaic_demo_tmx = @".\Content\Tilemaps\mosaic\tiled\0001_Mosaic_demo.tmx";
					public const string _0002_World_Level_1_tmx = @".\Content\Tilemaps\mosaic\tiled\0002_World_Level_1.tmx";
					public const string Mosaic_world = @".\Content\Tilemaps\mosaic\tiled\mosaic.world";
					public const string Wallsintgrid_png = @".\Content\Tilemaps\mosaic\tiled\Walls.intgrid.png";
				}

			}

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

			public static class Tiles
			{
				public const string Directory = @".\Content\Tilemaps\Tiles";
			
				public const string Inca_back2_by_Kronbits_png = @".\Content\Tilemaps\Tiles\Inca_back2_by_Kronbits.png";
				public const string Inca_front_by_Kronbitsextended_png = @".\Content\Tilemaps\Tiles\Inca_front_by_Kronbits-extended.png";
			}

			// Ignored file '.keep';
			public const string Mosaic_ldtk = @".\Content\Tilemaps\mosaic.ldtk";
			public const string Test_ldtk = @".\Content\Tilemaps\test.ldtk";
		}

		public const string Translations_xlsx = @".\Content\translations.xlsx";

	}
}

