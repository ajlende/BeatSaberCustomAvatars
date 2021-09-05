﻿//  Beat Saber Custom Avatars - Custom player models for body presence in Beat Saber.
//  Copyright © 2018-2021  Beat Saber Custom Avatars Contributors
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

using CustomAvatar.Avatar;
using CustomAvatar.Configuration;
using HarmonyLib;
using UnityEngine;

namespace CustomAvatar.HarmonyPatches
{
    [HarmonyPatch(typeof(MirrorRendererSO), "CreateOrUpdateMirrorCamera")]
    internal static class MirrorRendererSO_CreateOrUpdateMirrorCamera
    {
        internal static Settings settings { get; set; }

        public static void Postfix(Camera ____mirrorCamera)
        {
            if (settings.showAvatarInMirrors)
            {
                ____mirrorCamera.cullingMask |= AvatarLayers.kAllLayersMask;
            }
            else
            {
                ____mirrorCamera.cullingMask &= ~AvatarLayers.kAllLayersMask;
            }
        }
    }
}
