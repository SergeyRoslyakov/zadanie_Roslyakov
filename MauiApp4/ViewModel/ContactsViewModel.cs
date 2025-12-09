using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp4.DTO;
using MauiApp4.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MauiApp4.ViewModel
{
    partial class ContactsViewModel : ObservableObject
    {
        private readonly IApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<ContactDto> _contacts = new();

        [ObservableProperty]
        private ContactDto _selectedContact;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isModalVisible;

        [ObservableProperty]
        private ContactDto _editingContact;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _modalTitle = "Контакт";
        
        // Конструктор для DI
        public ContactsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Console.WriteLine("✅ ViewModel создан с ApiService");

            // Автоматическая загрузка при создании
            Task.Run(async () =>
            {
                await Task.Delay(1000); // Даем время на инициализацию
                await LoadContacts();
            });
        }

        // Конструктор по умолчанию для XAML
        public ContactsViewModel() : this(new ApiService())
        {
        }

        [RelayCommand]
        private async Task LoadContacts()
        {
            try
            {
                Console.WriteLine("🔄 Начало загрузки контактов...");
                IsBusy = true;

                var contacts = await _apiService.GetContactsAsync();
                Console.WriteLine($"📥 Получено контактов: {contacts?.Count ?? 0}");

                // Обновляем UI в главном потоке
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Contacts.Clear();

                    if (contacts != null && contacts.Count > 0)
                    {
                        foreach (var contact in contacts)
                        {
                            Contacts.Add(contact);
                        }
                        Console.WriteLine($"✅ Добавлено в коллекцию: {Contacts.Count}");
                    }
                    else
                    {
                        Console.WriteLine("ℹ️ Нет контактов для отображения");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось загрузить контакты: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsBusy = false;
                Console.WriteLine("🔄 Загрузка завершена");
            }
        }

        [RelayCommand]
        private void AddContact()
        {
            Console.WriteLine("➕ Добавление нового контакта");
            EditingContact = new ContactDto
            {
                FirstName = "",
                LastName = "",
                Phone = "",
                Email = ""
            };
            ModalTitle = "Новый контакт";
            IsModalVisible = true;
        }

        [RelayCommand]
        private void EditContact(ContactDto contact)
        {
            if (contact == null) return;

            Console.WriteLine($"✏️ Редактирование контакта: {contact.FirstName} {contact.LastName}");
            EditingContact = new ContactDto
            {
                Id = contact.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Phone = contact.Phone,
                Email = contact.Email
            };
            ModalTitle = "Редактировать контакт";
            IsModalVisible = true;
        }

        [RelayCommand]
        private async Task DeleteContact(ContactDto contact)
        {
            if (contact == null) return;

            Console.WriteLine($"🗑️ Удаление контакта: {contact.Id}");

            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление",
                $"Удалить {contact.FirstName} {contact.LastName}?",
                "Да", "Нет");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await _apiService.DeleteContactAsync(contact.Id);

                // Удаляем из локальной коллекции
                Contacts.Remove(contact);

                await Application.Current.MainPage.DisplayAlert("Успех",
                    "Контакт удален", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка удаления: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось удалить: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveContact()
        {
            if (EditingContact == null) return;

            Console.WriteLine("=== СОХРАНЕНИЕ КОНТАКТА ===");

            try
            {
                IsBusy = true;

                if (EditingContact.Id == 0) // Новый контакт
                {
                    Console.WriteLine("➕ Создание нового контакта");

                    var createDto = new CreateContactDto
                    {
                        FirstName = EditingContact.FirstName?.Trim() ?? "",
                        LastName = EditingContact.LastName?.Trim() ?? "",
                        Phone = EditingContact.Phone?.Trim() ?? "",
                        Email = EditingContact.Email?.Trim() ?? ""
                    };

                    var newContact = await _apiService.CreateContactAsync(createDto);

                    if (newContact != null)
                    {
                        Console.WriteLine($"✅ Контакт создан! ID: {newContact.Id}");

                        // Добавляем в коллекцию
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Contacts.Add(newContact);
                        });

                        await Application.Current.MainPage.DisplayAlert("Успех",
                            "Контакт создан", "OK");
                    }
                }
                else // Обновление
                {
                    Console.WriteLine($"✏️ Обновление контакта ID: {EditingContact.Id}");

                    var updateDto = new UpdateContactDto
                    {
                        FirstName = EditingContact.FirstName?.Trim() ?? "",
                        LastName = EditingContact.LastName?.Trim() ?? "",
                        Phone = EditingContact.Phone?.Trim() ?? "",
                        Email = EditingContact.Email?.Trim() ?? ""
                    };

                    await _apiService.UpdateContactAsync(EditingContact.Id, updateDto);

                    // Обновляем в UI потоке
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var existing = Contacts.FirstOrDefault(c => c.Id == EditingContact.Id);
                        if (existing != null)
                        {
                            existing.FirstName = EditingContact.FirstName;
                            existing.LastName = EditingContact.LastName;
                            existing.Phone = EditingContact.Phone;
                            existing.Email = EditingContact.Email;
                        }
                    });

                    await Application.Current.MainPage.DisplayAlert("Успех",
                        "Контакт обновлен", "OK");
                }

                // Закрываем модальное окно
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsModalVisible = false;
                    EditingContact = null;
                });
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"🌐 HTTP ошибка: {httpEx.Message}");

                string userMessage = httpEx.Message.Contains("500")
                    ? "Ошибка сервера. Проверьте, работает ли API и правильно ли настроена база данных."
                    : $"Ошибка сети: {httpEx.Message}";

                await Application.Current.MainPage.DisplayAlert("Ошибка", userMessage, "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Общая ошибка: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось сохранить: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                Console.WriteLine("=== СОХРАНЕНИЕ ЗАВЕРШЕНО ===");
            }
        }
    }
}