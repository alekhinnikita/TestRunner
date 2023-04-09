using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Desktop.Models;
using DynamicData;
using Services;

namespace Desktop;

public partial class MainWindow : Window
{
    private List<Test> _tests = new ();
    private readonly ObservableCollection<Test> _items = new ();
    private readonly ObservableCollection<Test> _selectedItems = new ();
    private string? _folder;
    private readonly DesktopContext _context = new ();
    private Models.Template Setting = new ();

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
            _items.AddRange(_tests);
        }
    }

    private void ConfigurationMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.SettingDialogWindow();
        dialog._mainWindow = this;
        dialog.Show();
    }

    private void AddItems_OnClick(object? sender, RoutedEventArgs e)
    {
        var items = ItemsDataGrid.SelectedItems;
        if (items.Count == 0) return;
        foreach (Test item in items)
        {
            if (_selectedItems.FirstOrDefault((i) => i == item) == null)
            {
                _selectedItems.Add(item);
            }
        }
    }

    private void RemoveItems_OnClick(object? sender, RoutedEventArgs e)
    {
        var items = SelectedItemsDataGrid.SelectedItems;
        if (items.Count == 0) return;
        
        var names = (from Test item in items select item.Name).ToList();
        foreach (var name in names)
        {
            var item = _selectedItems.First((i) => i.Name == name);
            _selectedItems.Remove(item);
        }
    }

    private async void RunOneItems_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_folder == null) return;
        if (SelectedItemsDataGrid.SelectedItem is Test item)
        {
            await Task.Run(() =>
                       {
                           var result = CypressRunner.Run(item, _folder);
                           item.Result = result ? "Пройден" : "Провален";
                           SaveResult(item);
                       });
        }
    }

    private async void RunAllItems_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_folder == null) return;
        await Task.Run(() =>
        {
            var result = CypressRunner.RunAllParallel(_selectedItems.ToList(), _folder, 4);
            _selectedItems.ToList().ForEach((test) => SaveResult(test));
        });
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

    private void SaveResult(Test test)
    {
        var result = new Models.RunResult
        {
            Name = test.Name,
            File = test.File,
            Log = test.Progressing,
        };
        if(test.Result == "Пройден")
        {
            result.Success = true;
        }
        _context.RunResults.Add(result);
        _context.SaveChanges();
    }

    public void ChangeSettings(string address = "", string login = "", string password = "", string project = "")
    {
        if (Setting == null)
        {
            Setting = _context.Templates.Add(new Models.Template()).Entity;
            _context.SaveChanges();
        }
        Setting.Address = address;
        Setting.Login = login;
        Setting.Password = password;
        Setting.Project = project;
        _context.Templates.Update(Setting);
        _context.SaveChanges();
    }
}