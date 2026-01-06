using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hastane_Otomasyonu.Services
{
    /// <summary>
    /// Gemini API (Google AI Studio / Generative Language) üzerinden içerik üretimi.
    /// Not: API anahtarını kaynak koda gömmeyin. ENV: GEMINI_API_KEY
    /// </summary>
    public sealed class GeminiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiClient(HttpClient httpClient, string apiKey, string model = "gemini-1.5-flash")
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentException("API key boş olamaz.", nameof(apiKey)) : apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gemini-1.5-flash" : model;
        }

        public static string GetApiKeyFromEnvironment()
        {
            return Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException("Prompt boş olamaz.", nameof(prompt));

            // Generative Language API (key query param)
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={Uri.EscapeDataString(_apiKey)}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    topP = 0.95,
                    maxOutputTokens = 800
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API hata döndü: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                // candidates[0].content.parts[0].text
                var root = doc.RootElement;
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var cand0 = candidates[0];
                    if (cand0.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array &&
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }
                }
            }
            catch
            {
                // ignore parse errors; fallthrough
            }

            return body;
        }

        public async Task<string> GenerateWithPdfAsync(string prompt, byte[] pdfBytes, string pdfMimeType = "application/pdf", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException("Prompt boş olamaz.", nameof(prompt));
            if (pdfBytes == null || pdfBytes.Length == 0) throw new ArgumentException("PDF boş olamaz.", nameof(pdfBytes));
            if (string.IsNullOrWhiteSpace(pdfMimeType)) pdfMimeType = "application/pdf";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={Uri.EscapeDataString(_apiKey)}";
            var b64 = Convert.ToBase64String(pdfBytes);

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inlineData = new
                                {
                                    mimeType = pdfMimeType,
                                    data = b64
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    topP = 0.95,
                    maxOutputTokens = 900
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API hata döndü: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var cand0 = candidates[0];
                    if (cand0.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array &&
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }
                }
            }
            catch
            {
                // ignore parse errors; fallthrough
            }

            return body;
        }
    }
}


