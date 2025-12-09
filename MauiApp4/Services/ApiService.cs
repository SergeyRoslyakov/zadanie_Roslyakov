using MauiApp4.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp4.Services
{
    internal class ApiService : IApiService
    {
        private readonly string _baseUrl = "https://localhost:7190/api/Contacts";
        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }

        public async Task<List<ContactDto>> GetContactsAsync(string search = null)
        {
            using var httpClient = CreateHttpClient();

            try
            {
                Console.WriteLine($"🌐 GET запрос к API...");

                var url = _baseUrl;
                if (!string.IsNullOrEmpty(search))
                {
                    url += $"?search={Uri.EscapeDataString(search)}";
                }

                Console.WriteLine($"   URL: {url}");
                var response = await httpClient.GetAsync(url);

                Console.WriteLine($"   Статус: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"   Успешно! Данные получены");

                    var contacts = JsonSerializer.Deserialize<List<ContactDto>>(content) ?? new List<ContactDto>();
                    Console.WriteLine($"   Контактов: {contacts.Count}");
                    return contacts;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"   Ошибка API: {error}");
                    return new List<ContactDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Ошибка GET: {ex.GetType().Name}: {ex.Message}");
                return new List<ContactDto>();
            }
        }

        public async Task<ContactDto> CreateContactAsync(CreateContactDto contact)
        {
            using var httpClient = CreateHttpClient();

            try
            {
                Console.WriteLine($"🌐 POST запрос к API...");
                Console.WriteLine($"   URL: {_baseUrl}");
                Console.WriteLine($"   Данные: {JsonSerializer.Serialize(contact)}");

                var json = JsonSerializer.Serialize(contact);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(_baseUrl, content);

                Console.WriteLine($"   Статус: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"   Ответ: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var createdContact = JsonSerializer.Deserialize<ContactDto>(responseContent);
                    Console.WriteLine($"   ✅ Успех! ID: {createdContact?.Id}");
                    return createdContact;
                }
                else
                {
                    Console.WriteLine($"   ❌ Ошибка: {responseContent}");
                    throw new HttpRequestException($"Ошибка {response.StatusCode}: {responseContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"💥 HTTP ошибка: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Общая ошибка создания: {ex}");
                throw new HttpRequestException($"Ошибка при создании контакта: {ex.Message}", ex);
            }
        }

        public async Task<ContactDto> GetContactAsync(int id)
        {
            using var httpClient = CreateHttpClient();

            try
            {
                var response = await httpClient.GetAsync($"{_baseUrl}/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContactDto>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения контакта: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateContactAsync(int id, UpdateContactDto contact)
        {
            using var httpClient = CreateHttpClient();

            try
            {
                var json = JsonSerializer.Serialize(contact);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{_baseUrl}/{id}", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteContactAsync(int id)
        {
            using var httpClient = CreateHttpClient();

            try
            {
                var response = await httpClient.DeleteAsync($"{_baseUrl}/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления: {ex.Message}");
                throw;
            }
        }
    }
}