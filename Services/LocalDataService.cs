using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Maui.Storage;
using ProductSnap.Maui.Models;

namespace ProductSnap.Maui.Services;

public sealed class LocalDataService
{
    private const int HashIterations = 210_000;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _dataPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private StoreData _data = new();
    private bool _loaded;

    public LocalDataService(string? appDataDirectory = null)
    {
        _dataPath = Path.Combine(
            appDataDirectory ?? FileSystem.AppDataDirectory,
            "productsnap-data.json");
    }

    public async Task<Utilisateur?> GetCurrentUserAsync()
    {
        return await ReadAsync(data =>
        {
            var user = data.Users.FirstOrDefault(item => item.Id == data.SessionUserId);
            return user is null ? null : ToModel(user);
        });
    }

    public Task<bool> RegisterAsync(
        string nom,
        string email,
        string telephone,
        string boutique,
        string motDePasse)
    {
        return MutateAsync(data =>
        {
            if (data.Users.Any(user =>
                    string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(telephone) &&
                     string.Equals(user.Telephone, telephone, StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }

            var salt = RandomNumberGenerator.GetBytes(16);
            var user = new UserRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                Nom = nom.Trim(),
                Email = email.Trim(),
                Telephone = telephone.Trim(),
                Boutique = boutique.Trim(),
                Role = "Gérant",
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(HashPassword(motDePasse, salt))
            };
            data.Users.Add(user);
            data.SessionUserId = user.Id;
            return true;
        });
    }

    public Task<bool> SignInAsync(string identifiant, string motDePasse)
    {
        return MutateAsync(data =>
        {
            var user = data.Users.FirstOrDefault(item =>
                string.Equals(item.Email, identifiant, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.Telephone, identifiant, StringComparison.OrdinalIgnoreCase));
            if (user is null || !PasswordMatches(user, motDePasse))
            {
                return false;
            }

            data.SessionUserId = user.Id;
            return true;
        });
    }

    public Task SignOutAsync()
    {
        return MutateAsync(data =>
        {
            data.SessionUserId = null;
            return true;
        });
    }

    public Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        return MutateAsync(data =>
        {
            var userId = RequireSession(data);
            var user = data.Users.First(item => item.Id == userId);
            if (!PasswordMatches(user, currentPassword))
                throw new InvalidOperationException("Le mot de passe actuel est incorrect.");
            if (newPassword.Length < 8)
                throw new ArgumentException("Le nouveau mot de passe doit contenir au moins 8 caractères.");

            var salt = RandomNumberGenerator.GetBytes(16);
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = Convert.ToBase64String(HashPassword(newPassword, salt));
            return true;
        });
    }

    public async Task<IReadOnlyList<Produit>> GetProductsAsync(string? search = null, string? category = null)
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            var products = data.Products.Where(product => product.UserId == userId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                products = products.Where(product =>
                    product.Nom.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    product.Categorie.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    product.CodeBarres.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category) &&
                !string.Equals(category, "Tous", StringComparison.OrdinalIgnoreCase))
            {
                products = products.Where(product =>
                    string.Equals(product.Categorie, category, StringComparison.OrdinalIgnoreCase));
            }

            return products.OrderBy(product => product.Nom)
                .Select(product => ToModel(product))
                .ToList();
        });
    }

    public async Task<Produit?> GetProductByBarcodeAsync(string codeBarres)
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            var product = data.Products.FirstOrDefault(item =>
                item.UserId == userId &&
                string.Equals(item.CodeBarres, codeBarres, StringComparison.Ordinal));
            return product is null ? null : ToModel(product);
        });
    }

    public async Task<Produit?> GetProductAsync(int id)
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            var product = data.Products.FirstOrDefault(item => item.UserId == userId && item.Id == id);
            return product is null ? null : ToModel(product);
        });
    }

    public Task<Produit> AddProductAsync(Produit produit, int stockInitial, decimal prixAchat)
    {
        return MutateAsync(data =>
        {
            var userId = RequireSession(data);
            var barcode = produit.CodeBarres.Trim();
            if (barcode.Length > 0 && data.Products.Any(item =>
                    item.UserId == userId &&
                    string.Equals(item.CodeBarres, barcode, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException("Ce code-barres existe déjà dans votre catalogue.");
            }

            if (string.IsNullOrWhiteSpace(produit.Nom))
                throw new ArgumentException("Le nom du produit est obligatoire.", nameof(produit));
            if (produit.Prix <= 0)
                throw new ArgumentOutOfRangeException(nameof(produit), "Le prix de vente doit être supérieur à zéro.");
            if (stockInitial < 0 || prixAchat < 0)
                throw new ArgumentOutOfRangeException(nameof(stockInitial), "Le stock et le prix d'achat ne peuvent pas être négatifs.");

            var record = new ProductRecord
            {
                Id = NextId(data.Products.Select(item => item.Id)),
                UserId = userId,
                Nom = produit.Nom.Trim(),
                Categorie = produit.Categorie.Trim(),
                CodeBarres = barcode,
                Prix = produit.Prix,
                Stock = stockInitial,
            };
            record.ActualiserStatutStock();
            data.Products.Add(record);

            if (stockInitial > 0)
            {
                var lotId = NextId(data.Lots.Select(item => item.Id));
                data.Lots.Add(new LotRecord
                {
                    Id = lotId,
                    UserId = userId,
                    ProduitId = record.Id,
                    Reference = $"LOT-{DateTime.Now:yyyyMMdd-HHmmss}-{lotId:D4}",
                    Quantite = stockInitial,
                    PrixUnitaire = prixAchat,
                    DateEntree = DateTime.Now,
                    Fournisseur = "",
                    Badge = "NOUVEAU"
                });
            }

            data.PriceHistory.Add(new PriceRecord
            {
                Id = NextId(data.PriceHistory.Select(item => item.Id)),
                UserId = userId,
                ProduitId = record.Id,
                NomProduit = record.Nom,
                Prix = record.Prix,
                DateModification = DateTime.Now
            });
            return ToModel(record);
        });
    }

    public async Task<IReadOnlyList<Lot>> GetLotsAsync(int produitId)
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            return data.Lots.Where(lot => lot.UserId == userId && lot.ProduitId == produitId)
                .OrderBy(lot => lot.DateEntree)
                .Select(ToModel)
                .ToList();
        });
    }

    public Task AddStockAsync(int produitId, int quantite, decimal prixUnitaire, string fournisseur)
    {
        return MutateAsync(data =>
        {
            var userId = RequireSession(data);
            var product = data.Products.FirstOrDefault(item => item.UserId == userId && item.Id == produitId)
                ?? throw new InvalidOperationException("Produit introuvable.");
            if (quantite <= 0 || prixUnitaire < 0)
                throw new ArgumentOutOfRangeException(nameof(quantite), "La quantité doit être positive et le prix ne peut pas être négatif.");

            var now = DateTime.Now;
            product.Stock += quantite;
            product.ActualiserStatutStock();
            var lotId = NextId(data.Lots.Select(item => item.Id));
            data.Lots.Add(new LotRecord
            {
                Id = lotId,
                UserId = userId,
                ProduitId = product.Id,
                Reference = $"LOT-{now:yyyyMMdd-HHmmss}-{lotId:D4}",
                Quantite = quantite,
                PrixUnitaire = prixUnitaire,
                DateEntree = now,
                Fournisseur = fournisseur.Trim(),
                Badge = "NOUVEAU"
            });
            return true;
        });
    }

    public Task UpdateProductPriceAsync(int produitId, decimal prix)
    {
        return MutateAsync(data =>
        {
            var userId = RequireSession(data);
            var product = data.Products.FirstOrDefault(item => item.UserId == userId && item.Id == produitId)
                ?? throw new InvalidOperationException("Produit introuvable.");
            if (prix <= 0)
                throw new ArgumentOutOfRangeException(nameof(prix), "Le prix doit être supérieur à zéro.");

            product.Prix = prix;
            data.PriceHistory.Add(new PriceRecord
            {
                Id = NextId(data.PriceHistory.Select(item => item.Id)),
                UserId = userId,
                ProduitId = product.Id,
                NomProduit = product.Nom,
                Prix = prix,
                DateModification = DateTime.Now
            });
            return true;
        });
    }

    public Task<Vente> RecordSaleAsync(int produitId, int quantite, string modePaiement, string client)
    {
        return MutateAsync(data =>
        {
            var userId = RequireSession(data);
            var product = data.Products.FirstOrDefault(item => item.UserId == userId && item.Id == produitId)
                ?? throw new InvalidOperationException("Produit introuvable.");
            if (quantite <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantite), "La quantité doit être supérieure à zéro.");
            if (quantite > product.Stock)
                throw new InvalidOperationException("Le stock disponible est insuffisant.");

            var lots = data.Lots.Where(item =>
                    item.UserId == userId && item.ProduitId == produitId && item.Quantite > 0)
                .OrderBy(item => item.DateEntree)
                .ToList();
            var restant = quantite;
            string? premierLot = null;
            foreach (var lot in lots)
            {
                if (restant == 0)
                    break;
                var preleve = Math.Min(restant, lot.Quantite);
                lot.Quantite -= preleve;
                restant -= preleve;
                premierLot ??= lot.Reference;
            }

            if (restant > 0)
                throw new InvalidOperationException("Le stock en lots ne correspond pas au stock du produit. Corrigez le stock avant la vente.");

            product.Stock -= quantite;
            product.ActualiserStatutStock();
            var sale = new SaleRecord
            {
                Id = NextId(data.Sales.Select(item => item.Id)),
                UserId = userId,
                ProduitId = product.Id,
                NomProduit = product.Nom,
                Lot = premierLot ?? "",
                Quantite = quantite,
                DateVente = DateTime.Now,
                Client = client.Trim(),
                Montant = product.Prix * quantite,
                ModePaiement = modePaiement.Trim()
            };
            data.Sales.Add(sale);
            return ToModel(sale);
        });
    }

    public async Task<IReadOnlyList<Vente>> GetSalesAsync()
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            return data.Sales.Where(sale => sale.UserId == userId)
                .OrderByDescending(sale => sale.DateVente)
                .Select(ToModel)
                .ToList();
        });
    }

    public async Task<IReadOnlyList<HistoriquePrix>> GetPriceHistoryAsync()
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            return data.PriceHistory.Where(entry => entry.UserId == userId)
                .OrderByDescending(entry => entry.DateModification)
                .Select(entry => new HistoriquePrix
                {
                    Id = entry.Id,
                    ProduitId = entry.ProduitId,
                    NomProduit = entry.NomProduit,
                    Prix = entry.Prix,
                    DateModification = entry.DateModification
                })
                .ToList();
        });
    }

    public async Task<(decimal caJour, int nbVentes, int nbAlertes, int nbReferences)> GetStatsAsync()
    {
        return await ReadAsync(data =>
        {
            var userId = RequireSession(data);
            var sales = data.Sales.Where(sale =>
                sale.UserId == userId && sale.DateVente.Date == DateTime.Today).ToList();
            var products = data.Products.Where(product => product.UserId == userId).ToList();
            return (
                sales.Sum(sale => sale.Montant),
                sales.Count,
                products.Count(product => product.Stock <= 3),
                products.Count);
        });
    }

    private async Task<T> ReadAsync<T>(Func<StoreData, T> query)
    {
        await _gate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            return query(_data);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<T> MutateAsync<T>(Func<StoreData, T> mutation)
    {
        await _gate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            var updated = Clone(_data);
            var result = mutation(updated);
            await SaveAsync(updated);
            _data = updated;
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded)
            return;

        if (File.Exists(_dataPath))
        {
            await using var stream = File.OpenRead(_dataPath);
            _data = await JsonSerializer.DeserializeAsync<StoreData>(stream, JsonOptions)
                ?? throw new InvalidDataException("Le fichier de données ProductSnap est vide ou invalide.");
        }

        _loaded = true;
    }

    private async Task SaveAsync(StoreData data)
    {
        var directory = Path.GetDirectoryName(_dataPath)
            ?? throw new InvalidOperationException("Le dossier de données ProductSnap est introuvable.");
        Directory.CreateDirectory(directory);
        var temporaryPath = $"{_dataPath}.tmp";
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None,
                             4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
                await stream.FlushAsync();
            }

            File.Move(temporaryPath, _dataPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, HashIterations, HashAlgorithmName.SHA256, 32);
    }

    private static bool PasswordMatches(UserRecord user, string password)
    {
        var salt = Convert.FromBase64String(user.Salt);
        var expected = Convert.FromBase64String(user.PasswordHash);
        var actual = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static string RequireSession(StoreData data)
    {
        return data.SessionUserId
            ?? throw new InvalidOperationException("Connectez-vous pour accéder à vos données.");
    }

    private static int NextId(IEnumerable<int> ids) => ids.DefaultIfEmpty().Max() + 1;

    private static StoreData Clone(StoreData data)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        return JsonSerializer.Deserialize<StoreData>(json, JsonOptions)
            ?? throw new InvalidDataException("Impossible de préparer l'enregistrement des données.");
    }

    private static Utilisateur ToModel(UserRecord user) => new()
    {
        Id = user.Id,
        Nom = user.Nom,
        Initiale = string.IsNullOrWhiteSpace(user.Nom) ? "?" : user.Nom[..1].ToUpperInvariant(),
        Boutique = user.Boutique,
        Role = user.Role,
        Email = user.Email,
        Telephone = user.Telephone
    };

    private static Produit ToModel(ProductRecord product) => new()
    {
        Id = product.Id,
        Nom = product.Nom,
        Categorie = product.Categorie,
        CodeBarres = product.CodeBarres,
        Prix = product.Prix,
        Stock = product.Stock,
        StatutStock = product.StatutStock
    };

    private static Lot ToModel(LotRecord lot) => new()
    {
        Id = lot.Id,
        ProduitId = lot.ProduitId,
        Reference = lot.Reference,
        Quantite = lot.Quantite,
        PrixUnitaire = lot.PrixUnitaire,
        DateEntree = lot.DateEntree,
        Fournisseur = lot.Fournisseur,
        Badge = lot.Quantite == 0 ? "ÉPUISÉ" : lot.Badge
    };

    private static Vente ToModel(SaleRecord sale) => new()
    {
        Id = sale.Id,
        ProduitId = sale.ProduitId,
        NomProduit = sale.NomProduit,
        Lot = sale.Lot,
        Quantite = sale.Quantite,
        DateVente = sale.DateVente,
        Client = sale.Client,
        Montant = sale.Montant,
        ModePaiement = sale.ModePaiement
    };

    private sealed class StoreData
    {
        public string? SessionUserId { get; set; }
        public List<UserRecord> Users { get; set; } = [];
        public List<ProductRecord> Products { get; set; } = [];
        public List<LotRecord> Lots { get; set; } = [];
        public List<SaleRecord> Sales { get; set; } = [];
        public List<PriceRecord> PriceHistory { get; set; } = [];
    }

    private sealed class UserRecord
    {
        public string Id { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string Boutique { get; set; } = "";
        public string Role { get; set; } = "";
        public string Salt { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }

    private sealed class ProductRecord : Produit
    {
        public string UserId { get; set; } = "";
    }

    private sealed class LotRecord : Lot
    {
        public string UserId { get; set; } = "";
    }

    private sealed class SaleRecord : Vente
    {
        public string UserId { get; set; } = "";
    }

    private sealed class PriceRecord : HistoriquePrix
    {
        public string UserId { get; set; } = "";
    }
}
