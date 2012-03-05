/*
 * Copyright (c) 2012, Peter Nelson (http://peterdn.com)
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * * Redistributions of source code must retain the above copyright notice, 
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice, 
 *   this list of conditions and the following disclaimer in the documentation 
 *   and/or other materials provided with the distribution.
 *   
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings.Storage.DefaultFileStorages;
using JetBrains.Application.Settings.UserInterface.FileInjectedLayers;
using JetBrains.Util;
using JetBrains.Application.Settings.UserInterface;

namespace FastSettingsSwitch
{
    public abstract class SwitchSettingsAction : IActionHandler
    {
        private readonly GlobalSettings _globalSettings;

        protected const string NamePrefix = "UserInjected::File::";
        protected abstract string SettingsFilePath { get; }
        protected string LayerName { get { return NamePrefix + SettingsFilePath; } }

        protected SwitchSettingsAction()
        {
            _globalSettings = PlatformObsoleteStatics.Instance.GetComponent<GlobalSettings>();
            if (_globalSettings == null)
            {
                throw new ApplicationException("Could not get GlobalSettings instance!");
            }
        }

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            var injector = PlatformObsoleteStatics.Instance.GetComponent<FileInjectedLayers>();
            if (injector == null)
            {
                throw new ApplicationException("Could not get FileInjectedLayers instance!");
            }

            var file = new FileSystemPath(SettingsFilePath);

            var settingsLayers = PlatformObsoleteStatics.Instance.GetComponent<UserInjectedSettingsLayers>();
            if (settingsLayers == null)
            {
                throw new ApplicationException("Could not get UserInjectedSettingsLayers instance!");
            }

            var layers = settingsLayers.GetUserInjectedLayersFromHost(_globalSettings.ProductGlobalLayerId);
            foreach (var layer in layers)
            {
                // TODO: find out if there's an easier way to check if this is layer we want, e.g. by id
                settingsLayers.TurnInjectedLayerOnOff(layer.Id, layer.Name == LayerName);
            }

            if (!injector.IsLayerInjected(_globalSettings.ProductGlobalLayerId, file))
            {
                injector.InjectLayer(_globalSettings.ProductGlobalLayerId, file);
            }
        }
    }
}
