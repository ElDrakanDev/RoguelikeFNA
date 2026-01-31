
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

			public static class Zero_slashfx
			{
				public const string Directory = @".\Content\Atlases\zero_slashfx";
			
				public const string Zero_slashfx_atlas = @".\Content\Atlases\zero_slashfx\zero_slashfx.atlas";
				public const string Zero_slashfx_png = @".\Content\Atlases\zero_slashfx\zero_slashfx.png";
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
				public const string Slash_nson = @".\Content\Serializables\Entities\slash.nson";
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
			
				public const string Dmg_item_item = @".\Content\Serializables\Items\dmg_item.item";
				public const string Example_item_item = @".\Content\Serializables\Items\example_item.item";
				public const string Hptodmg_item_item = @".\Content\Serializables\Items\hptodmg_item.item";
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
		
			public static class Essence
			{
				public const string Directory = @".\Content\Sprites\Essence";
			
				public const string Large_png = @".\Content\Sprites\Essence\large.png";
				public const string Medium_png = @".\Content\Sprites\Essence\medium.png";
				public const string Small_png = @".\Content\Sprites\Essence\small.png";
			}

			public const string Apple_png = @".\Content\Sprites\Apple.png";
			public const string ExampleSword_png = @".\Content\Sprites\ExampleSword.png";
			public const string Giant_png = @".\Content\Sprites\giant.png";
			public const string Nezlogoblack_png = @".\Content\Sprites\nez-logo-black.png";
			public const string Strength_png = @".\Content\Sprites\strength.png";
			public const string Test_png = @".\Content\Sprites\test.png";
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
			
				public static class Assets
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\assets";
				
					public static class Rules
					{
						public const string Directory = @".\Content\Tilemaps\mosaic\assets\rules";
					
						public const string Rules_tmx = @".\Content\Tilemaps\mosaic\assets\rules\rules.tmx";
					}

					public static class Templates
					{
						public const string Directory = @".\Content\Tilemaps\mosaic\assets\templates";
					
						public const string DemoEnemy_tx = @".\Content\Tilemaps\mosaic\assets\templates\DemoEnemy.tx";
						public const string Entrance_tx = @".\Content\Tilemaps\mosaic\assets\templates\Entrance.tx";
						public const string Exit_tx = @".\Content\Tilemaps\mosaic\assets\templates\Exit.tx";
						public const string RandomItem_tx = @".\Content\Tilemaps\mosaic\assets\templates\RandomItem.tx";
					}

					public static class Tiles
					{
						public const string Directory = @".\Content\Tilemaps\mosaic\assets\tiles";
					
						public const string Inca_back2_by_Kronbits_png = @".\Content\Tilemaps\mosaic\assets\tiles\Inca_back2_by_Kronbits.png";
						public const string Inca_back2_by_Kronbits_tsx = @".\Content\Tilemaps\mosaic\assets\tiles\Inca_back2_by_Kronbits.tsx";
						public const string Inca_front_by_Kronbitsextended_png = @".\Content\Tilemaps\mosaic\assets\tiles\Inca_front_by_Kronbits-extended.png";
						public const string Inca_front_by_Kronbitsextended_tsx = @".\Content\Tilemaps\mosaic\assets\tiles\Inca_front_by_Kronbits-extended.tsx";
					}

				}

				public static class Boss
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\Boss";
				
					public const string Test_1_tmx = @".\Content\Tilemaps\mosaic\Boss\test_1.tmx";
				}

				public static class Normal
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\Normal";
				
					public const string Test_1_tmx = @".\Content\Tilemaps\mosaic\Normal\test_1.tmx";
					public const string TestBigger_2_tmx = @".\Content\Tilemaps\mosaic\Normal\testBigger_2.tmx";
				}

				public static class Shop
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\Shop";
				
					public const string Test_1_tmx = @".\Content\Tilemaps\mosaic\Shop\test_1.tmx";
					public const string Triple_2_tmx = @".\Content\Tilemaps\mosaic\Shop\triple_2.tmx";
				}

				public static class Start
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\Start";
				
					public const string Test_1_tmx = @".\Content\Tilemaps\mosaic\Start\test_1.tmx";
				}

				public static class Treasure
				{
					public const string Directory = @".\Content\Tilemaps\mosaic\Treasure";
				
					public const string Test_1_tmx = @".\Content\Tilemaps\mosaic\Treasure\test_1.tmx";
				}

				// Ignored file 'mosaic.tiled-project';
				// Ignored file 'mosaic.tiled-session';
				public const string Rules_txt = @".\Content\Tilemaps\mosaic\rules.txt";
			}

		}

		public const string Translations_xlsx = @".\Content\translations.xlsx";

	}
}

