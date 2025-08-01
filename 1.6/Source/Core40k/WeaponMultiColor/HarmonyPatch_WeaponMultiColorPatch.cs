// using System;
// using System.Collections.Generic;
// using HarmonyLib;
// using RimWorld;
// using UnityEngine;
// using Verse;
//
// namespace Core40k;
//
// [HarmonyPatch(typeof(GraphicDatabase), "Get", new Type[]
// {
//     typeof(Type),
//     typeof(string),
//     typeof(Shader),
//     typeof(Vector2),
//     typeof(Color),
//     typeof(Color),
//     typeof(GraphicData),
//     typeof(List<ShaderParameter>),
//     typeof(string),
// }, new ArgumentType[]
// {
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
//     ArgumentType.Normal,
// })]
// public static class WeaponMultiColorPatch
// {
//     public static bool Prefix(ref Graphic __result)
//     {
//         
//     }
// }