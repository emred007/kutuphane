using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.WinForms;

public sealed partial class MainForm : Form
{
    private readonly LibraryAppBootstrap _bootstrap;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _usersGrid = CreateGrid();
    private readonly DataGridView _booksGrid = CreateGrid();
    private readonly DataGridView _activeLoansGrid = CreateGrid();
    private readonly DataGridView _historyGrid = CreateGrid();
    private readonly TextBox _reportBox = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };
    private readonly ComboBox _borrowUserCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox _borrowBookCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox _historyUserCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox _historyBookCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly TextBox _userNameInput = new() { Width = 160 };
    private readonly TextBox _userEmailInput = new() { Width = 160 };
    private readonly TextBox _userPhoneInput = new() { Width = 110, MaxLength = 11 };
    private readonly TextBox _bookTitleInput = new() { Width = 180 };
    private readonly TextBox _bookAuthorInput = new() { Width = 180 };
    private readonly TextBox _bookIsbnInput = new() { Width = 180 };
    private readonly NumericUpDown _bookCopiesInput = new() { Width = 60, Minimum = 1, Maximum = 100, Value = 1 };
    private readonly Label _statusLabel = new() { Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };
    private readonly System.Windows.Forms.Timer _syncTimer = new() { Interval = 2000 };

    public MainForm(LibraryAppBootstrap bootstrap)
    {
        _bootstrap = bootstrap;
        Text = "Kütüphane Yönetim Sistemi";
        Width = 1100;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);

        BuildTabs();
        Controls.Add(_tabs);
        Controls.Add(_statusLabel);

        RefreshAll();
        SetStatus($"Veritabanı: {_bootstrap.DatabasePath}");

        _syncTimer.Tick += (_, _) => SyncFromDatabase();
        _syncTimer.Start();
    }

    private void BuildTabs()
    {
        _tabs.TabPages.Add(CreateUsersTab());
        _tabs.TabPages.Add(CreateBooksTab());
        _tabs.TabPages.Add(CreateBorrowTab());
        _tabs.TabPages.Add(CreateHistoryTab());
        _tabs.TabPages.Add(CreateReportTab());
        _tabs.TabPages.Add(CreateOverdueTab());
        _tabs.TabPages.Add(CreateSearchTab());
        _tabs.TabPages.Add(CreateStatsTab());
        _tabs.TabPages.Add(CreateReservationTab());
        _tabs.TabPages.Add(CreateSettingsTab());
    }

    private TabPage CreateUsersTab()
    {
        var page = new TabPage("Kullanıcılar");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Ad Soyad:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_userNameInput);
        top.Controls.Add(new Label { Text = "E-posta:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_userEmailInput);
        top.Controls.Add(new Label { Text = "Telefon:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_userPhoneInput);
        top.Controls.Add(new Label { Text = "(05 + 9 rakam)", AutoSize = true, ForeColor = Color.Gray, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(CreateButton("Ekle", (_, _) => AddUser()));
        top.Controls.Add(CreateButton("Düzenle", (_, _) => EditUser()));
        top.Controls.Add(CreateButton("Yenile", (_, _) => RefreshAll()));

        _usersGrid.SelectionChanged += (_, _) => LoadSelectedUserToInputs();

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_usersGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateBooksTab()
    {
        var page = new TabPage("Kitaplar");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Başlık:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_bookTitleInput);
        top.Controls.Add(new Label { Text = "Yazar:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_bookAuthorInput);
        top.Controls.Add(new Label { Text = "ISBN:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_bookIsbnInput);
        top.Controls.Add(new Label { Text = "Kopya:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_bookCopiesInput);
        top.Controls.Add(CreateButton("Ekle", (_, _) => AddBook()));
        top.Controls.Add(CreateButton("Düzenle", (_, _) => EditBook()));
        top.Controls.Add(CreateButton("Sil", (_, _) => DeleteBook()));
        top.Controls.Add(CreateButton("Yenile", (_, _) => RefreshAll()));

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_booksGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateBorrowTab()
    {
        var page = new TabPage("Ödünç İşlemleri");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var borrowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        borrowPanel.Controls.Add(new Label { Text = "Kullanıcı:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        borrowPanel.Controls.Add(_borrowUserCombo);
        borrowPanel.Controls.Add(new Label { Text = "Kitap:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        borrowPanel.Controls.Add(_borrowBookCombo);
        borrowPanel.Controls.Add(CreateButton("Ödünç Ver", (_, _) => BorrowBook()));
        borrowPanel.Controls.Add(CreateButton("Yenile", (_, _) => RefreshAll()));

        var returnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        returnPanel.Controls.Add(new Label { Text = "İade edilmemiş kayıtlar - satır seçip iade alın", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        returnPanel.Controls.Add(CreateButton("İade Al", (_, _) => ReturnSelectedLoan()));

        layout.Controls.Add(borrowPanel, 0, 0);
        layout.Controls.Add(returnPanel, 0, 1);
        layout.Controls.Add(_activeLoansGrid, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateHistoryTab()
    {
        var page = new TabPage("Geçmiş");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Kullanıcı:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_historyUserCombo);
        top.Controls.Add(CreateButton("Kullanıcı Geçmişi", (_, _) => ShowUserHistory()));
        top.Controls.Add(new Label { Text = "Kitap:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_historyBookCombo);
        top.Controls.Add(CreateButton("Kitap Geçmişi", (_, _) => ShowBookHistory()));

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_historyGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateReportTab()
    {
        var page = new TabPage("Rapor");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        top.Controls.Add(CreateButton("SQL'den Çek ve Dosyaları Güncelle", (_, _) => RefreshReport()));
        top.Controls.Add(CreateButton("Yedek Al", (_, _) => CreateBackup()));

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_reportBox, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private void RefreshAll()
    {
        RefreshDataViews();
        RefreshReport();
    }

    private void RefreshDataViews()
    {
        RefreshUsers();
        RefreshBooks();
        RefreshActiveLoans();
        RefreshCombos();
        RefreshOverdue();
        RefreshReservations();
        RefreshStats();
    }

    private void SyncFromDatabase()
    {
        if (!_bootstrap.Library.ReloadIfChanged())
        {
            return;
        }

        RefreshDataViews();
        SetStatus("Veriler senkronize edildi.");
    }

    private void RefreshUsers()
    {
        _usersGrid.DataSource = _bootstrap.Library.GetAllUsers()
            .Select(u => new
            {
                u.Id,
                AdSoyad = u.FullName,
                u.Email,
                Telefon = u.PhoneNumber,
                KayitTarihi = FormatDate(u.CreatedAt)
            })
            .ToList();
    }

    private void RefreshBooks()
    {
        _booksGrid.DataSource = _bootstrap.Library.GetAllBooks()
            .Select(b => new
            {
                b.Id,
                Baslik = b.Title,
                Yazar = b.Author,
                b.Isbn,
                MusaitKopya = b.AvailableCopies,
                ToplamKopya = b.TotalCopies
            })
            .ToList();
    }

    private void RefreshActiveLoans()
    {
        _activeLoansGrid.DataSource = _bootstrap.Library.GetActiveLoans()
            .Select(r => new
            {
                KayitId = r.Loan.Id,
                AdSoyad = r.User.FullName,
                Kitap = r.Book.Title,
                AlinmaTarihi = FormatDate(r.Loan.BorrowedAt),
                SonIadeTarihi = FormatDate(r.Loan.DueDate)
            })
            .ToList();
    }

    private void RefreshCombos()
    {
        var selectedUser = _borrowUserCombo.SelectedItem as UserListItem;
        var selectedBook = _borrowBookCombo.SelectedItem as BookListItem;

        _borrowUserCombo.DataSource = _bootstrap.Library.GetAllUsers()
            .Select(u => new UserListItem(u))
            .ToList();
        _borrowUserCombo.DisplayMember = nameof(UserListItem.Display);
        _borrowUserCombo.ValueMember = nameof(UserListItem.Id);

        _borrowBookCombo.DataSource = _bootstrap.Library.GetAllBooks()
            .Where(b => b.IsAvailable)
            .Select(b => new BookListItem(b))
            .ToList();
        _borrowBookCombo.DisplayMember = nameof(BookListItem.Display);
        _borrowBookCombo.ValueMember = nameof(BookListItem.Id);

        _historyUserCombo.DataSource = _bootstrap.Library.GetAllUsers()
            .Select(u => new UserListItem(u))
            .ToList();
        _historyUserCombo.DisplayMember = nameof(UserListItem.Display);
        _historyUserCombo.ValueMember = nameof(UserListItem.Id);

        _historyBookCombo.DataSource = _bootstrap.Library.GetAllBooks()
            .Select(b => new BookListItem(b))
            .ToList();
        _historyBookCombo.DisplayMember = nameof(BookListItem.Display);
        _historyBookCombo.ValueMember = nameof(BookListItem.Id);

        if (selectedUser is not null)
        {
            SelectComboItem(_borrowUserCombo, selectedUser.Id);
            SelectComboItem(_historyUserCombo, selectedUser.Id);
        }

        if (selectedBook is not null)
        {
            SelectComboItem(_historyBookCombo, selectedBook.Id);
        }
    }

    private void RefreshReport()
    {
        _bootstrap.ExportAllReports();
        var data = _bootstrap.Library.GetAllData();
        var stats = _bootstrap.Library.GetStatistics();
        _reportBox.Text = DataViewer.BuildReadableReport(data, stats);
        _reportBox.AppendText(Environment.NewLine + $"JSON: {_bootstrap.ExportJsonPath}");
        _reportBox.AppendText(Environment.NewLine + $"TXT: {_bootstrap.ExportTextPath}");
        _reportBox.AppendText(Environment.NewLine + $"Excel: {_bootstrap.ExportExcelPath}");
        _reportBox.AppendText(Environment.NewLine + $"HTML: {_bootstrap.ExportHtmlPath}");
        _reportBox.AppendText(Environment.NewLine + $"PDF: {_bootstrap.ExportPdfPath}");
    }

    private void AddUser()
    {
        try
        {
            _bootstrap.Library.AddUser(_userNameInput.Text, _userEmailInput.Text, _userPhoneInput.Text);
            _userNameInput.Clear();
            _userEmailInput.Clear();
            _userPhoneInput.Clear();
            AfterDataChange("Kullanıcı eklendi.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void EditUser()
    {
        try
        {
            if (_usersGrid.CurrentRow?.Cells["Id"].Value is not Guid userId)
            {
                ShowError("Düzenlenecek kullanıcıyı seçin.");
                return;
            }

            _bootstrap.Library.UpdateUser(userId, _userNameInput.Text, _userEmailInput.Text, _userPhoneInput.Text);
            AfterDataChange("Kullanıcı güncellendi.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void LoadSelectedUserToInputs()
    {
        if (_usersGrid.CurrentRow?.Cells["Id"].Value is not Guid userId)
        {
            return;
        }

        try
        {
            var user = _bootstrap.Library.GetUserOrThrow(userId);
            _userNameInput.Text = user.FullName;
            _userEmailInput.Text = user.Email;
            _userPhoneInput.Text = user.PhoneNumber;
        }
        catch
        {
        }
    }

    private void AddBook()
    {
        try
        {
            var copies = int.TryParse(_bookCopiesInput.Text, out var c) && c > 0 ? c : 1;
            _bootstrap.Library.AddBook(_bookTitleInput.Text, _bookAuthorInput.Text, _bookIsbnInput.Text, copies);
            _bookTitleInput.Clear();
            _bookAuthorInput.Clear();
            _bookIsbnInput.Clear();
            AfterDataChange("Kitap eklendi.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BorrowBook()
    {
        try
        {
            if (_borrowUserCombo.SelectedItem is not UserListItem user || _borrowBookCombo.SelectedItem is not BookListItem book)
            {
                ShowError("Kullanıcı ve kitap seçin.");
                return;
            }

            _bootstrap.Library.BorrowBook(user.Id, book.Id);
            AfterDataChange("Kitap ödünç verildi.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ReturnSelectedLoan()
    {
        try
        {
            if (_activeLoansGrid.CurrentRow?.Cells["KayitId"].Value is not Guid loanId)
            {
                ShowError("İade edilecek kaydı seçin.");
                return;
            }

            _bootstrap.Library.ReturnBook(loanId);
            AfterDataChange("Kitap iade alındı.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowUserHistory()
    {
        try
        {
            if (_historyUserCombo.SelectedItem is not UserListItem user)
            {
                ShowError("Kullanıcı seçin.");
                return;
            }

            _historyGrid.DataSource = _bootstrap.Library.GetUserReadingHistory(user.Id)
                .Select(r => new
                {
                    AdSoyad = r.User.FullName,
                    Kitap = r.Book.Title,
                    AlinmaTarihi = FormatDate(r.Loan.BorrowedAt),
                    Durum = r.Loan.IsReturned ? $"İade: {FormatDate(r.Loan.ReturnedAt!.Value)}" : "Henüz iade edilmedi"
                })
                .ToList();

            SetStatus($"{user.Display} okuma geçmişi gösteriliyor.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowBookHistory()
    {
        try
        {
            if (_historyBookCombo.SelectedItem is not BookListItem book)
            {
                ShowError("Kitap seçin.");
                return;
            }

            _historyGrid.DataSource = _bootstrap.Library.GetBookReadingHistory(book.Id)
                .Select(r => new
                {
                    AdSoyad = r.User.FullName,
                    Kitap = r.Book.Title,
                    AlinmaTarihi = FormatDate(r.Loan.BorrowedAt),
                    Durum = r.Loan.IsReturned ? $"İade: {FormatDate(r.Loan.ReturnedAt!.Value)}" : "Henüz iade edilmedi"
                })
                .ToList();

            SetStatus($"{book.Display} okuma geçmişi gösteriliyor.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void AfterDataChange(string message)
    {
        _bootstrap.ExportAllReports();
        RefreshAll();
        SetStatus(message);
    }

    private void SetStatus(string message) => _statusLabel.Text = message;

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        SetStatus(message);
    }

    private static Button CreateButton(string text, EventHandler onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Padding = new Padding(12, 6, 12, 6) };
        button.Click += onClick;
        return button;
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
    }

    private static string FormatDate(DateTime date)
        => date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    private static void SelectComboItem(ComboBox comboBox, Guid id)
    {
        for (var i = 0; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is UserListItem user && user.Id == id)
            {
                comboBox.SelectedIndex = i;
                return;
            }

            if (comboBox.Items[i] is BookListItem book && book.Id == id)
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }
    }

    private sealed record UserListItem(User User)
    {
        public Guid Id => User.Id;
        public string Display => $"{User.FullName} ({User.Email})";
    }

    private sealed record BookListItem(Book Book)
    {
        public Guid Id => Book.Id;
        public string Display => $"{Book.Title} - {Book.Author}";
    }
}
