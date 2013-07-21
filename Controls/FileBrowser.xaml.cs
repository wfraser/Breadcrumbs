using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Breadcrumbs
{
    public partial class FileBrowser : UserControl
    {
        public FileBrowser()
        {
            InitializeComponent();
        }

        private ViewModels.FileBrowserViewModel ViewModel
        {
            get { return m_viewModel; }
        }
        private ViewModels.FileBrowserViewModel m_viewModel;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.FileBrowserViewModel)DataContext;
        }

        private void SelectItem(object sender, RoutedEventArgs e)
        {
            var item = ItemList.SelectedItem as ViewModels.FileBrowserViewModel.FileBrowserEntry;

            if (item != null)
            {
                if (item.IsFolder)
                {
                    ViewModel.NavigateToSubfolder(item.FileName);
                }
                else
                {
                    if (ViewModel.Mode == ViewModels.FileBrowserMode.Save)
                    {
                        var result = MessageBox.Show("Are you sure you want to overwrite that file?", "Confirm Overwrite", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }

                    ViewModel.SelectFile(item.FileName);
                }
            }
            else if (ViewModel.Mode == ViewModels.FileBrowserMode.Save)
            {
                ViewModel.SelectNewFile(FileNameEntryBox.Text);
            }

            FileNameEntryBox.Text = string.Empty;
        }

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MakeDirectory(FileNameEntryBox.Text);
            FileNameEntryBox.Text = string.Empty;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Dismiss(null);
            FileNameEntryBox.Text = string.Empty;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ItemList.SelectedItem as ViewModels.FileBrowserViewModel.FileBrowserEntry;
            if (item == null)
            {
                return;
            }

            ViewModel.DeleteItem(item.FileName);

            FileNameEntryBox.Text = string.Empty;
        }

        private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ViewModels.FileBrowserViewModel.FileBrowserEntry)((ListBox)sender).SelectedItem;

            if ((selectedItem != null) 
                || ((ViewModel.Mode == ViewModels.FileBrowserMode.Save) && (!string.IsNullOrEmpty(FileNameEntryBox.Text))))
            {
                SelectButton.IsEnabled = true;
            }
            else
            {
                SelectButton.IsEnabled = false;
            }

            if (selectedItem != null)
            {
                FileNameEntryBox.Text = selectedItem.FileName;
                DeleteButton.IsEnabled = true;
            }
            else
            {
                DeleteButton.IsEnabled = false;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateUp();
            FileNameEntryBox.Text = string.Empty;
        }

        private void FolderNameScrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            // Keep the folder path scrolled to the right edge. Hack, but it works.
            FolderNameScrollViewer.ScrollToHorizontalOffset(FolderNameScrollViewer.ActualWidth);
        }

        private void FileNameEntryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FileNameEntryBox.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
            {
                MessageBox.Show("It contains invalid characters.", "Check your filename", MessageBoxButton.OK);
            }

            ViewModels.FileBrowserViewModel.FileBrowserEntry matchedItem = null;
            foreach (var item in ItemList.Items.OfType<ViewModels.FileBrowserViewModel.FileBrowserEntry>())
            {
                if (FileNameEntryBox.Text == item.FileName)
                {
                    matchedItem = item;
                }
            }

            ItemList.SelectedItem = matchedItem;

            if (string.IsNullOrEmpty(FileNameEntryBox.Text) && (ItemList.SelectedItem == null))
            {
                NewFolderButton.IsEnabled = false;
                SelectButton.IsEnabled = false;
            }
            else if (ViewModel.Mode == ViewModels.FileBrowserMode.Save)
            {
                NewFolderButton.IsEnabled = true;
                SelectButton.IsEnabled = true;
            }
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CloudSync();
        }
    }
}
