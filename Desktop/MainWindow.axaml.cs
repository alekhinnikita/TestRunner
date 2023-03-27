using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Services;

namespace Desktop;

public partial class MainWindow : Window
{
    private List<Test> _tests = new List<Test>();
    private readonly ObservableCollection<Test> _items = new ObservableCollection<Test>();
    private readonly ObservableCollection<Test> _selectedItems = new ObservableCollection<Test>();
    private string? _folder;

    public MainWindow()
    {
        InitializeComponent();

        ItemsDataGrid.Items = _items;
        SelectedItemsDataGrid.Items = _selectedItems;
    }

    private async void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        _folder = await dialog.ShowAsync(this);
        if (_folder != null)
        {
            var files = new DirectoryInfo(_folder)
                .GetFiles("*", SearchOption.AllDirectories)
                .Select((f) => f.FullName)
                .Where((l) => l.Contains("node_modules") == false);
            _tests = SearchService.GetTests(files).ToList();
            _items.Clear();
            _tests.ForEach((test) => _items.Add(test));
        }
    }

    private void AddItems_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem is Test item)
        {
            if (_selectedItems.FirstOrDefault((i) => i == item) == null)
            {
                _selectedItems.Add(item);
            }
        }
    }

    private void RemoveItems_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedItemsDataGrid.SelectedItem is Test item)
        {
            if (_selectedItems.FirstOrDefault((i) => i == item) != null)
            {
                _selectedItems.Remove(item);
            }
        }
    }

    private async void RunOneItems_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedItemsDataGrid.SelectedItem is Test item)
        {
            if (_folder != null)
            {
                await Task.Run(() => item.Result = CypressRunner.Run(item, _folder));
                RefreshItems_OnClick(sender, e);
            }
        }
    }

    private async void RunAllItems_OnClick(object? sender, RoutedEventArgs e)
    {
        await Task.Run(() => CypressRunner.RunAll(_selectedItems.ToList(), _folder, 3));
        RefreshItems_OnClick(sender, e);
    }

    private void RefreshItems_OnClick(object? sender, RoutedEventArgs e)
    {
        ItemsDataGrid.Items = null;
        SelectedItemsDataGrid.Items = null;
        ItemsDataGrid.Items = _items;
        SelectedItemsDataGrid.Items = _selectedItems;
    }

    private void SearchBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!_tests.Any()) return;
        _items.Clear();
        _tests.Where((test) => 
                test.Name.Contains(SearchBox.Text) ||
                test.File.Contains(SearchBox.Text))
            .ToList().ForEach((test) => _items.Add(test));
    }
}