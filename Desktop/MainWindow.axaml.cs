using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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
    private async void HistoryMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.RunHIstoryWindow();
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

    private string ChangeConfig(string url = "", string login = "", string password = "")
    {
        var text = File.ReadAllText(_folder);
        if (url != "")
        {
            var line = text.Split("\n").FirstOrDefault((l) => l.Contains("url:"));
            if(line != null)
            {
                var newLine = "url: '" + url + "',";
                text.Replace(line, newLine);
            }
        }
        if (login != "")
        {
            var line = text.Split("\n").FirstOrDefault((l) => l.Contains("login:"));
            if (line != null)
            {
                var newLine = "login: '" + login + "',";
                text.Replace(line, newLine);
            }
        }
        if (password != "")
        {
            var line = text.Split("\n").FirstOrDefault((l) => l.Contains("password:"));
            if (line != null)
            {
                var newLine = "password: '" + password + "',";
                text.Replace(line, newLine);
            }
        }

        return text;
    }

    private async void CreateReport_OnClick(object? sender, RoutedEventArgs e)
    {
        var items = ItemsDataGrid.SelectedItems;
        if (items.Count == 0) return;
        var body = @"
        <html>
            <head>
                <style type=""text/css"">
                table, td {
                  border-collapse: collapse;
                  border: 1px solid;
                }
                </style>
            </head>
            <body>
                <table>
                <tr><td>Название</td><td>Файл</td><td>Результат</td></tr>
        ";
        
        foreach (Test item in items)
        {
            var line = "<tr>";
            line += "<td>" + item.Name + "</td>";
            line += "<td>" + item.File.Split(_folder.Replace("\\", "/"))[1] + "</td>";
            line += "<td>" + item.Result + "</td>";
            line += "</tr>";

            body += "\n" + line;
        }

        body += "\n" + "<p>Дата: " + DateTime.Now.ToString("dd.MM.yyyy") + "</p>";
        body += "\n" + "<p>URL: " + Setting.Address + "</p>";
        body += "\n" + "<p>Логин: " + Setting.Login + "</p>";
        body += "\n" + "<p>Пароль: " + Setting.Password + "</p>";

        body += "</table></body></html>";

        var sfd = new SaveFileDialog();
        sfd.DefaultExtension = "html";
        var file = await sfd.ShowAsync(this);
        File.WriteAllText(file, body);
    }
}