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
using CustomAvatar.Logging;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace CustomAvatar.Rendering
{
    internal class CustomAvatarsSmoothCameraController : MonoBehaviour
    {
        private const float kCameraDefaultNearClipMask = 0.1f;

        private static readonly FieldAccessor<SmoothCamera, bool>.Accessor kThirdPersonEnabledAccessor = FieldAccessor<SmoothCamera, bool>.GetAccessor("_thirdPersonEnabled");

        private ILogger<CustomAvatarsSmoothCameraController> _logger;
        private Settings _settings;
        private MainSettingsModelSO _mainSettingsModel;

        private SmoothCamera _smoothCamera;
        private Camera _camera;

        [Inject]
        public void Construct(ILogger<CustomAvatarsSmoothCameraController> logger, Settings settings, MainSettingsModelSO mainSettingsModel)
        {
            _logger = logger;
            _settings = settings;
            _mainSettingsModel = mainSettingsModel;

            _smoothCamera = GetComponent<SmoothCamera>();
            _camera = GetComponent<Camera>();
        }

        public void Start()
        {
            // prevent errors if this is instantiated via Object.Instantiate
            if (_logger == null || _settings == null || _mainSettingsModel == null)
            {
                Destroy(this);
                return;
            }

            _settings.cameraNearClipPlane.changed += OnCameraNearClipPlaneChanged;
            _settings.showAvatarInSmoothCamera.changed += OnShowAvatarInSmoothCameraChanged;
            _mainSettingsModel.smoothCameraThirdPersonEnabled.didChangeEvent += OnSmoothCameraThirdPersonEnabled;

            UpdateSmoothCamera();
        }

        public void OnDestroy()
        {
            if (_settings != null)
            {
                _settings.cameraNearClipPlane.changed -= OnCameraNearClipPlaneChanged;
                _settings.showAvatarInSmoothCamera.changed -= OnShowAvatarInSmoothCameraChanged;
            }

            if (_mainSettingsModel) _mainSettingsModel.smoothCameraThirdPersonEnabled.didChangeEvent -= OnSmoothCameraThirdPersonEnabled;
        }

        private void OnCameraNearClipPlaneChanged(float value)
        {
            UpdateSmoothCamera();
        }

        private void OnShowAvatarInSmoothCameraChanged(bool value)
        {
            UpdateSmoothCamera();
        }

        private void OnSmoothCameraThirdPersonEnabled()
        {
            UpdateSmoothCamera();
        }

        private void UpdateSmoothCamera()
        {
            bool thirdPersonEnabled = kThirdPersonEnabledAccessor(ref _smoothCamera);

            _logger.Info($"Setting avatar culling mask and near clip plane on '{_camera.name}'");

            if (!_settings.showAvatarInSmoothCamera)
            {
                _camera.cullingMask &= ~AvatarLayers.kAllLayersMask;
                _camera.nearClipPlane = kCameraDefaultNearClipMask;
            }
            else if (thirdPersonEnabled)
            {
                _camera.cullingMask = _camera.cullingMask | AvatarLayers.kOnlyInThirdPersonMask | AvatarLayers.kAlwaysVisibleMask;
                _camera.nearClipPlane = kCameraDefaultNearClipMask;
            }
            else
            {
                _camera.cullingMask = (_camera.cullingMask & ~AvatarLayers.kOnlyInThirdPersonMask) | AvatarLayers.kAlwaysVisibleMask;
                _camera.nearClipPlane = _settings.cameraNearClipPlane;
            }
        }
    }
}
