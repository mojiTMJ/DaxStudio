﻿using Caliburn.Micro;
using DaxStudio.Interfaces;
using DaxStudio.UI.Events;
using DaxStudio.UI.Extensions;
using DaxStudio.UI.Interfaces;
using DaxStudio.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DaxStudio.UI.Utils {
    [Export]
    [Export(typeof(ICloseStrategy<IScreen>))]
    public class ApplicationCloseAllStrategy : ICloseStrategy<IScreen> {
        //IEnumerator<IScreen> enumerator;
        //private IEnumerable<IScreen> toclose; 
        //bool finalResult;
        //Action<bool, IEnumerable<IScreen>> callback;
        private IWindowManager _windowManager;
        private IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public ApplicationCloseAllStrategy(IWindowManager windowManager, IEventAggregator eventAggregator )
        {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
        }

        public async Task<ICloseResult<IScreen>> ExecuteAsync(IEnumerable<IScreen> toClose, CancellationToken cancellationToken)
        {
            //enumerator = toClose.GetEnumerator();
            var closeable = toClose; // new List<IScreen>();
            var closeCanOccur = true;

            var docs = toClose.Cast<ISaveable>();// .ConvertAll(o => (DocumentViewModel)o);
            var dirtyDocs = new ObservableCollection<ISaveable>(docs.Where<ISaveable>(x => x.IsDirty));

            if (dirtyDocs.Count > 0)
            {
                var closeDialog = new SaveDialogViewModel();
                closeDialog.Documents = dirtyDocs;

                await _windowManager.ShowDialogBoxAsync(closeDialog, settings: new Dictionary<string, object>
                {
                    { "WindowStyle", WindowStyle.None},
                    { "ShowInTaskbar", false},
                    { "ResizeMode", ResizeMode.NoResize},
                    { "Background", System.Windows.Media.Brushes.Transparent},
                    { "AllowsTransparency",true}
                
                });

                if (closeDialog.Result == Enums.SaveDialogResult.Cancel)
                {
                    // loop through and cancel closing for any dirty documents
                    closeCanOccur = false; // cancel closing
                    //closeable = new List<IScreen>();
                    return new CloseResult<IScreen>(closeCanOccur, closeable);
                }
                else
                {
                    //await _eventAggregator.PublishOnUIThreadAsync(new StopAutoSaveTimerEvent());
                    closeCanOccur = await Evaluate(closeable);
                    return new CloseResult<IScreen>(closeCanOccur, closeable);
                }
            }
            else
            {
                //    callback(true, new List<IScreen>());
                await _eventAggregator.PublishOnUIThreadAsync(new StopAutoSaveTimerEvent());
                closeCanOccur = await Evaluate(closeable);
                return new CloseResult<IScreen>(closeCanOccur, closeable);
            }


        }

        async Task<bool> Evaluate(IEnumerable<IScreen> toclose)
        {
            var finalResult = true;

            foreach (var c in toclose.OfType<IGuardClose>())
            {
                finalResult = finalResult && await c.CanCloseAsync();
            }

            foreach (var doc in toclose.OfType<IHaveTraceWatchers>())
            {
                foreach (var tw in doc.TraceWatchers)
                {
                    if (tw.IsChecked) { tw.StopTrace(); }
                }

                if (doc is DocumentViewModel docModel)
                {
                    docModel.DeleteAutoSave();
                }
            }
            
            

            return finalResult;
        }
    }
}