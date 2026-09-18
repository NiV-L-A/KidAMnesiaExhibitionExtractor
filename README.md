# Kid A Mnesia Exhibition Extractor

## Description
Kid A Mnesia Exhibition Extractor is an app that can extract all audio files (approximately 2,98 GB) found in the game KID A MNESIA EXHIBITION.

## How to run / Troubleshooting
**IMPORTANT:** *.NET Runtime 10.0 x64* must be installed in order to run this program.<br>
https://dotnet.microsoft.com/en-us/download/dotnet/10.0

- Download latest at https://github.com/NiV-L-A/KidAMnesiaExhibitionExtractor/releases/latest
- The app will extract 1210 .wem files with the following naming scheme:
  - The "MediaName" (if found) in the game's AssetRegistry
  - The event names (if any) associated to that asset separated by a "-" character (these always start with the word "Play_")
  - The AkMediaAssetId associated to that asset. This is done to ensure uniqueness
- It will also extract 16 .ogg files which are not part of the WwiseAudio\Media folder.
- It will not extract the following 6 assets, as they have no media data attached to them:
  - 625141396
  - 602284699
  - 602284700
  - 685264471
  - 685264475
  - 768857212
- Asset ids "3300950372 and "292832817" will get their filename shortened because they go beyond Windows' 255 character limit:
  - Final_Amb\Studio\AMBRoom_Immersive Room Tone Dense And Low Designed Tone For Dark Office Ambience Ambix_SSLAB_SSL38-Play_rtAMB_StudioHallway-Play_rtAMB_PixelateJellyHall-Play_rtAMB_LiminalStairwell_A_Hallway-Play_rtAMB_LiminalPyrHall_A-Play_rtAMB_JellyStudioHall-Play_rtAMB_Landscape-Play_rtAMB_Kaleidoscope_Hallway-Play_rtAMB_PyramidApproach_330095037.wem
  - Final_Amb\Kaleidoscope\Thesholds\AMBRoom_Immersive Room Tone Dense And Low Designed Tone For Dark Office Ambience Ambix_SSLAB_SSL38-Play_thAMB_Kaleidoscope_Hallway-Play_thAMB_Landscape-Play_thAMB_JellyStudioHall-Play_thAMB_LiminalPyrHall_A-Play_thAMB_LiminalStairwell_A_Hallway-Play_thAMB_PixelateJellyHall-Play_thAMB_StudioHallway_292832817.wem

Examples of extracted files:
- Default_Work_Unit\897872056.wem
- Default_Work_Unit\Final_Music\Map_Music\ArtStudio_Events\ARTSTUDIO_InLimbo_MainGtr_MD02-Play_InLimbo_All__Studio_388574251.wem
- Default_Work_Unit\Final_Music\Map_Music\BlueNoise\v1\BlueSpikes_SPINNINGPLATES_Krumar_MDLoop_MD01-Play_SpinningPlates_Portal_147543167.wem
- Default_Work_Unit\Final_Music\Map_Music\C64_Events\V3\C64_IDIOTEQUE_Brixton_NewEdDrone_Loop_MD02-Play_C64_IDIOTEQUE_v3_EdDrone-Play_C64_IDIOTEQUE_Brixton_NewEdDrone-Play_C64_IDIOTEQUE_Brixton_NewEdDrone_ExitHallway_515211829.wem
- Default_Work_Unit\Final_Music\Map_Music\InsidePyramid_Events\B_State\PRISMFILMB_PushPulk_JonnyFluteAMS_COMMON_MD02-Play_PushPulk_All__Prism_749516840.wem
- Default_Work_Unit\Final_Music\Map_Music\Greybox2Tracks_Events\LandscapeGallery_HUNTINGBEARS_v1 (gtr_tone_drum_whitenoise)-Play_Hunt_2Track__Winding-Play_Hunt_2Track_1005425136.wem
- Default_Work_Unit\Final_Music\Map_Music\Lidar_Events\v1\How to Disappear 77.3 Lead Vocal Stem_MD02-Play_HTDC_Lead_Vocal__Lidar_588677179.wem
- Default_Work_Unit\Final_Music\Map_Music\Lidar_Events\v1\Pyramid Song Drum Stem_MD02-Play_PS_Drum__LIDAR_552717341.wem
- Default_Work_Unit\Final_Music\Map_Music\Lidar_Events\v1\You and Whos Army ED gtr stem_MD02-Play_YAWA_ED_gtr__LIDAR_195690477.wem
- Default_Work_Unit\Final_Music\Map_Music\Lidar_Events\v2\Army-Disappear-Pyramid Mashup 6.0 20-04-2021_6.0.C_NG04-Play_Lidar_Mashup_C_617248228.wem
- Default_Work_Unit\Final_Music\Map_Music\Pixelate_Events\Pixelate_SARDINES_BoxTrigger_NigelVoice_MD01-Play_SARDINES_BoxTrigger__Pixelate-Play_SARDINES_BoxTrigger_Beat-Play_SARDINES_BoxTrigger_NigelVoice_580241826.wem
- Default_Work_Unit\Final_Amb\Filmstrips\Threshold\AMBRoom_Immersive Room Tone Neutral Bath Ambience With Designed Low Ventilation Ambix_SSLAB_SSL38-Play_thAMB_FilmstripsExit_211874744.wem
- Default_Work_Unit\Final_SFX\Forest\Birds\blue bird dry_TY01-Play_BlueBird_Dry_310417802.wem
- Default_Work_Unit\Final_SFX\NPC\v3\YoureLivingInAFantasyWorld-Play_DEH-Play_Big_Stuttering_893871635.wem
- Default_Work_Unit\Final_SFX\Footsteps\SFX_Footstep_Concrete_1-Play_Footsteps_693560849.wem
- Default_Work_Unit\Final_SFX\Footsteps\SFX_Footstep_Concrete_1-Play_Footsteps_822921876.wem
- Default_Work_Unit\Final_SFX\Theater\03 Optimistic (mixed audio)_recompressed-Play_Optimistic_Movie_716689053.wem
- Default_Work_Unit\Test\KID A reverse AMS gold_NG01-Play_KID_A_testmix_292325062.wem
- Paperbag\Content\Developers\sean\AUDIO\TVRoom_NATIONAL_ANTHEM_Martinot_2TrackforSean_v1.ogg

## Credits
- NiV-L-A
- WEMExtractor: https://github.com/143mailliw/WEMExtractor
- CUE4Parse: https://github.com/FabianFG/CUE4Parse licensed under the Apache License 2.0