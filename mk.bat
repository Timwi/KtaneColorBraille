@echo off
rd build
if exist build rd /s /q build
mklink /j build "D:\Steam\steamapps\common\Keep Talking and Nobody Explodes\mods"
rd Assets\TestHarness
if exist Assets\TestHarness rd /s /q Assets\TestHarness
mklink /j Assets\TestHarness D:\c\ktanemodkit\Assets\TestHarness
rd Assets\Editor\Scripts
if exist Assets\Editor\Scripts rd /s /q Assets\Editor\Scripts
mklink /j Assets\Editor\Scripts D:\c\ktanemodkit\Assets\Editor\Scripts
rd Assets\Editor\Steamworks.NET
if exist Assets\Editor\Steamworks.NET rd /s /q Assets\Editor\Steamworks.NET
mklink /j Assets\Editor\Steamworks.NET D:\c\ktanemodkit\Assets\Editor\Steamworks.NET
rd Assets\Plugins
if exist Assets\Plugins rd /s /q Assets\Plugins
mklink /j Assets\Plugins D:\c\ktanemodkit\Assets\Plugins
rd Assets\Shaders
if exist Assets\Shaders rd /s /q Assets\Shaders
mklink /j Assets\Shaders D:\c\ktanemodkit\Assets\Examples\Assets\Shaders
rd Assets\KMScripts
if exist Assets\KMScripts rd /s /q Assets\KMScripts
mklink /j Assets\KMScripts D:\c\ktanemodkit\Assets\Scripts
rd ProjectSettings
if exist ProjectSettings rd /s /q ProjectSettings
mklink /j ProjectSettings D:\c\ktanemodkit\ProjectSettings
