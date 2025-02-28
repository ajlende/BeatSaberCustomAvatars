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

using System;
using System.Linq;
using CustomAvatar.Avatar;
using CustomAvatar.Configuration;
using CustomAvatar.HarmonyPatches;
using CustomAvatar.Lighting;
using CustomAvatar.Logging;
using CustomAvatar.Player;
using CustomAvatar.Rendering;
using CustomAvatar.Tracking;
using CustomAvatar.Tracking.OpenVR;
using CustomAvatar.Tracking.UnityXR;
using CustomAvatar.Utilities;
using IPA.Utilities;
using UnityEngine.XR;
using Valve.VR;
using Zenject;
using Logger = IPA.Logging.Logger;

namespace CustomAvatar.Zenject
{
    internal class CustomAvatarsInstaller : Installer
    {
        public static readonly int kPlayerAvatarManagerExecutionOrder = 1000;

        private readonly ILogger<CustomAvatarsInstaller> _logger;
        private readonly Logger _ipaLogger;
        private readonly PCAppInit _pcAppInit;

        public CustomAvatarsInstaller(Logger ipaLogger, PCAppInit pcAppInit)
        {
            _logger = new IPALogger<CustomAvatarsInstaller>(ipaLogger);
            _ipaLogger = ipaLogger;
            _pcAppInit = pcAppInit;
        }

        public override void InstallBindings()
        {
            // logging
            Container.Bind(typeof(ILogger<>)).FromMethodUntyped(CreateLogger).AsTransient().When(ShouldCreateLogger);

            // settings
            Settings settings = LoadSettings();
            MirrorRendererSO_CreateOrUpdateMirrorCamera.settings = settings;
            Container.Bind<Settings>().FromInstance(settings);
            Container.BindInterfacesAndSelfTo<CalibrationData>().AsSingle();

            if (XRSettings.loadedDeviceName.Equals("openvr", StringComparison.InvariantCultureIgnoreCase) &&
                OpenVR.IsRuntimeInstalled() &&
                OpenVR.System != null &&
                !Environment.GetCommandLineArgs().Contains("--force-xr"))
            {
                Container.Bind<OpenVRFacade>().AsTransient();
                Container.BindInterfacesAndSelfTo<OpenVRDeviceProvider>().AsSingle();
            }
            else
            {
                Container.BindInterfacesTo<UnityXRDeviceProvider>().AsSingle();
            }

            // managers
            Container.BindInterfacesAndSelfTo<PlayerAvatarManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ShaderLoader>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DeviceManager>().AsSingle().NonLazy();

            // this prevents a race condition when registering components in AvatarSpawner
            Container.BindExecutionOrder<PlayerAvatarManager>(kPlayerAvatarManagerExecutionOrder);

            Container.Bind<AvatarLoader>().AsSingle();
            Container.Bind<AvatarSpawner>().AsSingle();
            Container.BindInterfacesAndSelfTo<VRPlayerInput>().AsSingle();
            Container.Bind(typeof(VRPlayerInputInternal), typeof(IInitializable), typeof(IDisposable)).To<VRPlayerInputInternal>().AsSingle();
            Container.BindInterfacesAndSelfTo<LightingQualityController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatSaberUtilities>().AsSingle();

#pragma warning disable CS0612
            Container.BindInterfacesAndSelfTo<FloorController>().AsSingle();
#pragma warning restore CS0612

            // helper classes
            Container.Bind<MirrorHelper>().AsTransient();
            Container.Bind<IKHelper>().AsTransient();
            Container.Bind<TrackingHelper>().AsTransient();

            Container.Bind<MainSettingsModelSO>().FromInstance(_pcAppInit.GetField<MainSettingsModelSO, PCAppInit>("_mainSettingsModel")).IfNotBound();
        }

        private object CreateLogger(InjectContext context)
        {
            return Activator.CreateInstance(typeof(IPALogger<>).MakeGenericType(context.MemberType.GenericTypeArguments[0]), _ipaLogger);
        }

        private bool ShouldCreateLogger(InjectContext context)
        {
            return context.MemberType.GenericTypeArguments[0].IsAssignableFrom(context.ObjectType);
        }

        private Settings LoadSettings()
        {
            try
            {
                _logger.Info($"Reading settings from '{Settings.kSettingsPath}'");
                return Settings.Load();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to read settings from file");
                _logger.Error(ex);

                return new Settings();
            }
        }
    }
}
