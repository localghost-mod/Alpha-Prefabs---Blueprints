using System.Collections.Generic;
using Blueprints;
using HarmonyLib;
using KCSG;
using Verse;

namespace Alpha_Prefabs_Blueprints
{
    public static class Utils
    {
        public static Blueprint ToBluePrint(this StructureLayoutDef structureLayoutDef, string name)
        {
            var layouts = Traverse
                .Create(structureLayoutDef)
                .Field("_layouts")
                .GetValue<List<SymbolDef[,]>>();
            var sizes = structureLayoutDef.Sizes;
            var contents = new List<BuildableInfo>();
            foreach (var layout in layouts)
                for (var i = 0; i < sizes.z; ++i)
                for (var j = 0; j < sizes.x; ++j)
                    if (layout[i, j] != null)
                    {
                        var symbolDef = layout[i, j];
                        var traverse = Traverse.Create(symbolDef);
                        var thingDef = traverse.Field("thingDef").GetValue<ThingDef>();
                        var stuffDef = traverse.Field("stuffDef").GetValue<ThingDef>();
                        var rotation = traverse.Field("rotation").GetValue<Rot4>();
                        var position = new IntVec3(j, 0, i);
                        var thing = CreateThing(thingDef, stuffDef, position, rotation);
                        if (thingDef == null)
                            Log.Message($"{symbolDef} not found, set to null");
                        else if (!thingDef.BuildableByPlayer)
                            Log.Message($"{thingDef} is not buildable by plater, set to null");
                        else
                            contents.Add(new BuildableInfo(thing, IntVec3.Zero));
                    }

            var terrainGrid = Traverse
                .Create(structureLayoutDef)
                .Field("_terrainGrid")
                .GetValue<TerrainDef[,]>();
            for (var i = 0; i < sizes.z; ++i)
            for (var j = 0; j < sizes.x; ++j)
                if (terrainGrid[i, j] != null)
                {
                    var terrain = terrainGrid[i, j];
                    if (DefDatabase<TerrainDef>.GetNamed(terrain.defName) == null)
                        Log.Message($"{terrain.defName} not found, set to null");
                    else if (!terrain.BuildableByPlayer)
                        Log.Message($"{terrain.defName} is not buildable by player, set to null");
                    else
                        contents.Add(
                            new BuildableInfo(terrain, new IntVec3(j, 0, i), IntVec3.Zero)
                        );
                }
            return new Blueprint(contents, sizes, name.Replace(' ', '_').Replace('-', '_'));
        }

        public static Thing CreateThing(
            ThingDef thingDef,
            ThingDef stuff,
            IntVec3 position,
            Rot4 rotation
        )
        {
            var thing = new Thing { def = thingDef };
            var traverse = Traverse.Create(thing);
            traverse.Field("stuffInt").SetValue(stuff);
            traverse.Field("positionInt").SetValue(position);
            traverse.Field("rotationInt").SetValue(rotation);
            return thing;
        }
    }
}
