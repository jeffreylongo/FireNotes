﻿using System;
using System.Collections.Generic;
using firenotes.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Xamarin.Forms;
using Plugin.Toasts;

namespace firenotes.Views
{
    public partial class NewNotePage : ContentPage
    {
        private static string nodeUrl = $"{Constants.FirebaseUrl}users/{App.AuthLink.User.LocalId}";

        FirebaseClient firebaseClient = new FirebaseClient(
            nodeUrl,
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = async () => App.AuthLink.FirebaseToken
            });

        public NewNotePage()
        {
            InitializeComponent();
            Title = "New Note";

            if (Device.RuntimePlatform == Device.iOS)
            {
                btnSave.TextColor = Color.FromHex("#FF9800");
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                btnSave.BackgroundColor = Color.FromHex("#FF9800");
                btnSave.TextColor = Color.White;

                stkContent.BackgroundColor = Color.FromHex("#FAFAFA");
                frmEditor.BackgroundColor = Color.FromHex("#FAFAFA");
                txtDetails.BackgroundColor = Color.FromHex("#FAFAFA");
            }
        }

        protected async void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                DisplayError("Sorry, the title field is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDetails.Text))
            {
                bool response = await DisplayAlert("Warning",
                    "You have not filled in anything in the note details field. Do you want to proceed with saving this note?",
                    "Yes", "No");

                if (!response)
                {
                    return;
                }
            }

            var db = firebaseClient.Child("notes");

            txtTitle.IsEnabled = false;
            txtDetails.IsEnabled = false;
            spnrLoading.IsVisible = true;

            var note = new Note
            {
                Title = txtTitle.Text,
                Details = txtDetails.Text,
                IsFavorite = false,
                Tags = new List<string>(),
                Created = DateTime.UtcNow
            };

            try
            {
                var temp = await db.PostAsync(note);

                txtTitle.Text = null;
                txtDetails.Text = null;
                txtTitle.IsEnabled = true;
                txtDetails.IsEnabled = true;
                spnrLoading.IsVisible = false;

                var notificationOptions = new NotificationOptions
                {
                    Title = "Success",
                    Description = "Your note has been successfully saved.",
                    IsClickable = false
                };
                var notification = DependencyService.Get<IToastNotificator>();
                await notification.Notify(notificationOptions);
            }
            catch (Exception)
            {
                txtTitle.IsEnabled = true;
                txtDetails.IsEnabled = true;
                spnrLoading.IsVisible = false;

                var notificationOptions = new NotificationOptions
                {
                    Title = "Error",
                    Description = "An error occurred when saving your note",
                    IsClickable = false
                };
                var notification = DependencyService.Get<IToastNotificator>();
                await notification.Notify(notificationOptions);
            }
        }

        private void DisplayError(string message)
        {
            DisplayAlert("Error", message, "OK");
        }
    }
}
