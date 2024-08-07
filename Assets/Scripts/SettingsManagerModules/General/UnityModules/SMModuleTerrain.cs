using BattlePhaze.SettingsManager;
using UnityEngine;
namespace BattlePhaze.SettingsManager.Intergrations
{
    public class SMModuleTerrain : SettingsManagerOption
    {
        public Terrain terrain;
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager = null)
        {
            if (NameReturn(0, Option))
            {
                if (terrain == null)
                {
                    terrain = FindFirstObjectByType<Terrain>();
                    if (terrain == null)
                    {
                        return;
                    }
                }
                switch (Option.SelectedValue)
                {
                    case "very low":
                        terrain.detailObjectDistance = 80;
                        terrain.heightmapPixelError = 30;
                        terrain.basemapDistance = 200;
                        terrain.treeDistance = 200;
                        terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                        terrain.drawTreesAndFoliage = true;
                        break;
                    case "low":
                        terrain.detailObjectDistance = 100;
                        terrain.heightmapPixelError = 20;
                        terrain.basemapDistance = 300;
                        terrain.treeDistance = 300;
                        terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
                        terrain.drawTreesAndFoliage = true;
                        break;
                    case "medium":
                        terrain.detailObjectDistance = 120;
                        terrain.heightmapPixelError = 15;
                        terrain.basemapDistance = 400;
                        terrain.treeDistance = 400;
                        terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;
                        terrain.drawTreesAndFoliage = true;
                        break;
                    case "high":
                        terrain.detailObjectDistance = 150;
                        terrain.heightmapPixelError = 10;
                        terrain.basemapDistance = 500;
                        terrain.treeDistance = 500;
                        terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;
                        terrain.drawTreesAndFoliage = true;
                        break;
                    case "ultra":
                        terrain.detailObjectDistance = 200;
                        terrain.heightmapPixelError = 5;
                        terrain.basemapDistance = 600;
                        terrain.treeDistance = 600;
                        terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;
                        terrain.drawTreesAndFoliage = true;
                        break;
                }
            }
        }
    }
}