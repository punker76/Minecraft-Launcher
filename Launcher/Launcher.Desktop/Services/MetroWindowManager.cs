﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Launcher.Desktop.Contracts;
using Launcher.Desktop.Controls;
using Launcher.Desktop.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Launcher.Desktop.Services
{
    /// <summary>
    /// An implementation of <see cref="IMetroWindowManager" />. Basically it's extended <see cref="WindowManager" />.
    /// <para>
    /// Window manager adapted to <see cref="MetroWindow" />. The main window must be <see cref="MetroWindow" /> or it
    /// will throw an exception.
    /// </para>
    /// </summary>
    public class MetroWindowManager : WindowManager, IMetroWindowManager
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private IShell shell;

        public MetroWindowManager(IDialogCoordinator dialogCoordinator, MetroWindow window)
        {
            this.dialogCoordinator = dialogCoordinator;
            window.Loaded += (s, e) => shell = window.DataContext as IShell;
        }

        /// <summary>
        /// Shows a message dialog inside the current window.
        /// </summary>
        /// <param name="title">The title</param>
        /// <param name="message">The message text</param>
        /// <param name="dialogStyle">The additional style</param>
        /// <param name="settings">The additional settings</param>
        /// <returns>The async task with dialog result</returns>
        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle dialogStyle = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            return await dialogCoordinator.ShowMessageAsync(shell, title, message, dialogStyle, settings);
        }

        /// <summary>
        /// Shows a login dialog inside the current window.
        /// </summary>
        /// <returns>The async task with login result</returns>
        public async Task<LoginDialogData> ShowLoginAsync()
        {
            return await dialogCoordinator.ShowLoginAsync(shell, "Log on", "Please specify your login data", new LoginDialogSettings
            {
                UsernameWatermark = "Email",
                NegativeButtonVisibility = Visibility.Visible
            });
        }

        /// <summary>
        /// Shows a progress dialog inside the current window.
        /// </summary>
        /// <param name="title">The dialog title</param>
        /// <param name="message">The message</param>
        /// <param name="isCancelable">Indicates if can cancel</param>
        /// <returns>The dialog controller</returns>
        public async Task<ProgressDialogController> ShowProgressAsync(string title, string message, bool isCancelable = false)
        {
            return await dialogCoordinator.ShowProgressAsync(shell, title, message, isCancelable);
        }

        /// <summary>
        /// Shows a <see cref="PackManagementDialog" /> dialog inside the current window.
        /// </summary>
        /// <param name="packs">All packs available to choose from.</param>
        /// <param name="packToEdit">The pack to modify.</param>
        /// <param name="action">The message to the user. I.e 'edit' will be displayed.</param>
        /// <returns>The dialog</returns>
        public async Task<IPackDialog> ShowPackManagementDialogAsync(IEnumerable<Pack> packs, Pack packToEdit, string action)
        {
            await dialogCoordinator.ShowMetroDialogAsync(shell, new PackManagementDialog(packs, dialogCoordinator, shell, packToEdit, action));
            IPackDialog dialog = await dialogCoordinator.GetCurrentDialogAsync<PackManagementDialog>(shell);
            return dialog;
        }

        /// <summary>
        /// Shows a indeterminate progress dialog inside the current window.
        /// </summary>
        /// <param name="action">Async action to do while showing the progress.</param>
        /// <returns>The async task.</returns>
        public async Task ShowProgressAndDoAsync(Func<Task> action)
        {
            ProgressDialogController progress = await dialogCoordinator.ShowProgressAsync(shell, "Please wait", "Loading data");
            progress?.SetIndeterminate();
            await action();
            Task closeAsync = progress?.CloseAsync();
            if (closeAsync != null) await closeAsync;
        }

        /// <summary>
        /// Shows a dialog which allows user to specify a wanted directory.
        /// </summary>
        /// <param name="defaultPath">The path to start browsing from</param>
        /// <returns>The path</returns>
        public string ShowDirectoryBrowseDialog(string defaultPath)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = defaultPath,
                IsFolderPicker = true
            };

            CommonFileDialogResult result = dialog.ShowDialog();
            return result == CommonFileDialogResult.Ok ? dialog.FileName : defaultPath;
        }
    }
}