using LibraryManagement.Models;

namespace LibraryManagement.WinForms;

public sealed partial class MainForm
{
    private readonly DataGridView _overdueGrid = CreateGrid();
    private readonly DataGridView _searchGrid = CreateGrid();
    private readonly DataGridView _searchUserLoansGrid = CreateGrid();
    private readonly Label _searchUserLoansLabel = new()
    {
        Dock = DockStyle.Top,
        Height = 28,
        Text = "Kullanıcı seçin — aktif ödünç kitaplar burada görünür.",
        Padding = new Padding(8, 6, 0, 0)
    };
    private readonly DataGridView _reservationGrid = CreateGrid();
    private readonly TextBox _searchInput = new() { Width = 220 };
    private readonly ComboBox _searchTypeCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly TextBox _statsBox = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };
    private readonly NumericUpDown _loanDaysInput = new() { Width = 80, Minimum = 1, Maximum = 365, Value = 14 };
    private readonly ComboBox _reserveUserCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox _reserveBookCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };

    private TabPage CreateOverdueTab()
    {
        var page = new TabPage("Geciken İadeler");
        page.Controls.Add(_overdueGrid);
        return page;
    }

    private TabPage CreateSearchTab()
    {
        var page = new TabPage("Arama");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        _searchTypeCombo.Items.AddRange(["Kullanıcı", "Kitap"]);
        _searchTypeCombo.SelectedIndex = 0;

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Tür:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_searchTypeCombo);
        top.Controls.Add(new Label { Text = "Ara:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_searchInput);
        top.Controls.Add(CreateButton("Ara", (_, _) => RunSearch()));

        _searchGrid.SelectionChanged += (_, _) => ShowSearchUserActiveLoans();

        var loansPanel = new Panel { Dock = DockStyle.Fill };
        loansPanel.Controls.Add(_searchUserLoansGrid);
        loansPanel.Controls.Add(_searchUserLoansLabel);

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_searchGrid, 0, 1);
        layout.Controls.Add(loansPanel, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateStatsTab()
    {
        var page = new TabPage("İstatistikler");
        page.Controls.Add(_statsBox);
        return page;
    }

    private TabPage CreateReservationTab()
    {
        var page = new TabPage("Rezervasyon");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Kullanıcı:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_reserveUserCombo);
        top.Controls.Add(new Label { Text = "Kitap:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        top.Controls.Add(_reserveBookCombo);
        top.Controls.Add(CreateButton("Rezervasyon Yap", (_, _) => AddReservation()));
        top.Controls.Add(CreateButton("İptal Et", (_, _) => CancelReservation()));

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_reservationGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateSettingsTab()
    {
        var page = new TabPage("Ayarlar");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        panel.Controls.Add(new Label { Text = "Ödünç süresi (gün):", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        panel.Controls.Add(_loanDaysInput);
        panel.Controls.Add(CreateButton("Kaydet", (_, _) => SaveLoanDuration()));
        panel.Controls.Add(CreateButton("Yedek Al", (_, _) => CreateBackup()));
        page.Controls.Add(panel);
        return page;
    }

    private void RefreshOverdue()
    {
        _overdueGrid.DataSource = _bootstrap.Library.GetOverdueLoans()
            .Select(r => new
            {
                KayitId = r.Loan.Id,
                AdSoyad = r.User.FullName,
                Kitap = r.Book.Title,
                SonIadeTarihi = FormatDate(r.Loan.DueDate),
                GecikmeGunu = (int)(DateTime.UtcNow - r.Loan.DueDate).TotalDays
            }).ToList();
    }

    private void RefreshReservations()
    {
        _reserveUserCombo.DataSource = _bootstrap.Library.GetAllUsers().Select(u => new UserListItem(u)).ToList();
        _reserveUserCombo.DisplayMember = nameof(UserListItem.Display);

        _reserveBookCombo.DataSource = _bootstrap.Library.GetAllBooks().Select(b => new BookListItem(b)).ToList();
        _reserveBookCombo.DisplayMember = nameof(BookListItem.Display);

        _reservationGrid.DataSource = _bootstrap.Library.GetActiveReservationRecords()
            .Select(r => new
            {
                r.Reservation.Id,
                r.Reservation.UserFullName,
                Kitap = r.Book.Title,
                Sira = r.Reservation.Status == ReservationStatus.Ready ? "—" : r.QueuePosition.ToString(),
                RezervasyonTarihi = FormatDate(r.Reservation.ReservedAt),
                Durum = r.Reservation.Status == ReservationStatus.Ready ? "Hazır - ödünç alabilir" : r.Reservation.Status
            }).ToList();
    }

    private void RefreshStats()
    {
        var stats = _bootstrap.Library.GetStatistics();
        _loanDaysInput.Value = stats.LoanDurationDays;

        _statsBox.Text = $"""
            KULLANICI: {stats.UserCount}
            KİTAP TÜRÜ: {stats.BookCount}
            TOPLAM KOPYA: {stats.TotalCopies}
            MÜSAİT KOPYA: {stats.AvailableCopies}
            AKTİF ÖDÜNÇ: {stats.ActiveLoanCount}
            GECİKEN: {stats.OverdueCount}
            REZERVASYON: {stats.ReservationCount} (Hazır: {stats.ReadyReservationCount})
            ÖDÜNÇ SÜRESİ: {stats.LoanDurationDays} gün

            EN ÇOK OKUNAN KİTAPLAR:
            {string.Join(Environment.NewLine, stats.MostBorrowedBooks.Select(b => $"  {b.Title} - {b.Author}: {b.BorrowCount}"))}

            EN AKTİF KULLANICILAR:
            {string.Join(Environment.NewLine, stats.MostActiveUsers.Select(u => $"  {u.FullName}: {u.BorrowCount}"))}
            """;
    }

    private void RunSearch()
    {
        try
        {
            var query = _searchInput.Text;
            _searchUserLoansGrid.DataSource = null;
            _searchUserLoansLabel.Text = "Kullanıcı seçin — aktif ödünç kitaplar burada görünür.";

            if (_searchTypeCombo.SelectedItem?.ToString() == "Kitap")
            {
                _searchGrid.DataSource = _bootstrap.Library.SearchBooks(query)
                    .Select(b => new { b.Id, Baslik = b.Title, Yazar = b.Author, b.Isbn, Musait = b.AvailableCopies, Toplam = b.TotalCopies }).ToList();
            }
            else
            {
                _searchGrid.DataSource = _bootstrap.Library.SearchUsers(query)
                    .Select(u => new { u.Id, AdSoyad = u.FullName, u.Email, Telefon = u.PhoneNumber }).ToList();
            }
            SetStatus("Arama tamamlandı. Kullanıcıya tıklayın.");
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void ShowSearchUserActiveLoans()
    {
        if (_searchTypeCombo.SelectedItem?.ToString() != "Kullanıcı")
        {
            return;
        }

        if (_searchGrid.CurrentRow?.Cells["Id"].Value is not Guid userId)
        {
            return;
        }

        try
        {
            var user = _bootstrap.Library.GetUserOrThrow(userId);
            var loans = _bootstrap.Library.GetUserActiveLoans(userId);

            if (loans.Count == 0)
            {
                _searchUserLoansLabel.Text = $"{user.FullName} — aktif ödünç kitap yok.";
                _searchUserLoansGrid.DataSource = null;
                return;
            }

            _searchUserLoansLabel.Text = $"{user.FullName} — aktif ödünç kitaplar ({loans.Count}):";
            _searchUserLoansGrid.DataSource = loans
                .Select(r => new
                {
                    Kitap = r.Book.Title,
                    Yazar = r.Book.Author,
                    AlinmaTarihi = FormatDate(r.Loan.BorrowedAt),
                    SonIadeTarihi = FormatDate(r.Loan.DueDate),
                    Durum = r.Loan.IsOverdue ? "GECİKMİŞ" : "Aktif"
                })
                .ToList();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void EditBook()
    {
        try
        {
            if (_booksGrid.CurrentRow?.Cells["Id"].Value is not Guid bookId)
            {
                ShowError("Düzenlenecek kitabı seçin.");
                return;
            }

            var copies = int.TryParse(_bookCopiesInput.Text, out var c) && c > 0 ? c : 1;
            _bootstrap.Library.UpdateBook(bookId, _bookTitleInput.Text, _bookAuthorInput.Text, _bookIsbnInput.Text, copies);
            AfterDataChange("Kitap güncellendi.");
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void DeleteBook()
    {
        try
        {
            if (_booksGrid.CurrentRow?.Cells["Id"].Value is not Guid bookId)
            {
                ShowError("Silinecek kitabı seçin.");
                return;
            }

            if (MessageBox.Show(this, "Kitabı silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _bootstrap.Library.DeleteBook(bookId);
            AfterDataChange("Kitap silindi.");
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void AddReservation()
    {
        try
        {
            if (_reserveUserCombo.SelectedItem is not UserListItem user || _reserveBookCombo.SelectedItem is not BookListItem book)
            {
                ShowError("Kullanıcı ve kitap seçin.");
                return;
            }

            _bootstrap.Library.AddReservation(user.Id, book.Id);
            AfterDataChange("Rezervasyon eklendi. Kitap iade edilince sıra bildirimi verilir.");
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void CancelReservation()
    {
        try
        {
            if (_reservationGrid.CurrentRow?.Cells["Id"].Value is not Guid reservationId)
            {
                ShowError("İptal edilecek rezervasyonu seçin.");
                return;
            }

            _bootstrap.Library.CancelReservation(reservationId);
            AfterDataChange("Rezervasyon iptal edildi.");
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void SaveLoanDuration()
    {
        try
        {
            _bootstrap.Library.SetLoanDurationDays((int)_loanDaysInput.Value);
            AfterDataChange($"Ödünç süresi {(int)_loanDaysInput.Value} gün olarak kaydedildi.");
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void CreateBackup()
    {
        try
        {
            var path = _bootstrap.CreateBackup();
            SetStatus($"Yedek oluşturuldu: {path}");
            MessageBox.Show(this, $"Yedek alındı:\n{path}", "Yedekleme", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }
}
