using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetsEmployee.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace AssetsEmployee.Controllers
{
    [Authorize(Roles = "Admin,ITTechnician,Auditor")]
    public class AssetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Asset/Index
        public IActionResult Index()
        {
            var data = _context.Asset.ToList();
            return View(data);
        }

        // GET: /Asset/AddAsset
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult AddAsset()
        {
            return View();
        }

        // POST: /Asset/AddAssetItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult AddAssetItem(
            string serialno,
            string assetname,
            string? description,
            DateTime? purchaseDate,
            decimal? purchaseCost,
            decimal? salvageValue,
            int? usefulLifeMonths,
            DateTime? warrantyExpiryDate,
            string? warrantyProvider)
        {
            // 1. Validate required fields
            if (string.IsNullOrWhiteSpace(assetname))
            {
                ModelState.AddModelError("AssetName", "Asset Name is required.");
            }

            if (string.IsNullOrWhiteSpace(serialno))
            {
                ModelState.AddModelError("SerialNo", "Serial Number is required.");
            }
            else if (_context.Asset.Any(e => e.SerialNo != null && e.SerialNo.ToLower() == serialno.Trim().ToLower()))
            {
                ModelState.AddModelError("SerialNo", "This Serial Number is already registered. It must be unique.");
            }

            var newAsset = new Asset
            {
                SerialNo = serialno?.Trim(),
                AssetName = assetname?.Trim() ?? string.Empty,
                Description = description?.Trim(),
                Status = AssetStatus.Available,
                PurchaseDate = purchaseDate,
                PurchaseCost = purchaseCost ?? 0m,
                SalvageValue = salvageValue ?? 0m,
                UsefulLifeMonths = (usefulLifeMonths.HasValue && usefulLifeMonths.Value > 0) ? usefulLifeMonths.Value : 36,
                WarrantyExpiryDate = warrantyExpiryDate,
                WarrantyProvider = warrantyProvider?.Trim()
            };

            if (!ModelState.IsValid)
            {
                return View("AddAsset", newAsset);
            }

            _context.Asset.Add(newAsset);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        // GET: /Asset/UpdateAsset?assetid=5
        [Authorize(Roles = "Admin,ITTechnician")]
        public IActionResult UpdateAsset(int assetid)
        {
            var data = _context.Asset.FirstOrDefault(a => a.AssetId == assetid);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        // POST: /Asset/UpdateAssetItem
        [HttpPost]
        [Authorize(Roles = "Admin,ITTechnician")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAssetItem(
            int assetid,
            string serialno,
            string assetname,
            string description,
            DateTime? purchaseDate,
            decimal purchaseCost,
            decimal salvageValue,
            int usefulLifeMonths,
            DateTime? warrantyExpiryDate,
            string? warrantyProvider)
        {
            var data = _context.Asset.FirstOrDefault(a => a.AssetId == assetid);
            if (data == null)
            {
                return NotFound();
            }

            // 1. Validate Required Field
            if (string.IsNullOrWhiteSpace(serialno))
            {
                ModelState.AddModelError("SerialNo", "Serial No is required.");
            }
            // 2. Validate Uniqueness (Exclude current AssetId)
            else if (_context.Asset.Any(a => a.SerialNo != null && a.SerialNo.ToLower() == serialno.Trim().ToLower() && a.AssetId != assetid))
            {
                ModelState.AddModelError("SerialNo", "This Serial No is already assigned to another asset.");
            }

            data.SerialNo = serialno?.Trim();
            data.AssetName = assetname;
            data.Description = description;
            data.PurchaseDate = purchaseDate;
            data.PurchaseCost = purchaseCost;
            data.SalvageValue = salvageValue;
            data.UsefulLifeMonths = usefulLifeMonths > 0 ? usefulLifeMonths : 36;
            data.WarrantyExpiryDate = warrantyExpiryDate;
            data.WarrantyProvider = warrantyProvider;

            if (!ModelState.IsValid)
            {
                return View("UpdateAsset", data);
            }

            _context.Asset.Update(data);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /Asset/ConfirmDeleteAsset?assetid=5
        [HttpGet]
        [Route("Asset/ConfirmDeleteAsset")]
        [Authorize(Roles = "Admin,")]
        public IActionResult ConfirmDeleteAsset(int assetid)
        {
            var asset = _context.Asset.FirstOrDefault(a => a.AssetId == assetid);
            if (asset == null) return NotFound();
            return View(asset);
        }

        // POST: /Asset/DeleteAsset
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAsset(int assetid)
        {
            var asset = _context.Asset.Find(assetid);
            if (asset != null)
            {
                _context.Asset.Remove(asset);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: /Asset/DetailAsset?assetid=5 (Historical employee assignment view)
        // GET: /Asset/DetailAsset?assetid=5
        public IActionResult DetailAsset(int assetid)
        {
            var asset = _context.Asset.FirstOrDefault(a => a.AssetId == assetid);
            if (asset == null)
            {
                return NotFound();
            }

            // Generate Base64 QR code for this specific asset
            string qrData = $"ASSET-ID: {asset.AssetId}\nNAME: {asset.AssetName}\nSERIAL: {asset.SerialNo ?? "N/A"}\nWARRANTY: {(asset.IsUnderWarranty ? "VALID" : "EXPIRED")}";

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                ViewBag.QrCodeImage = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
            }

            ViewBag.TargetAsset = asset;

            var assignment = _context.EmployeeAsset
                .Include(ea => ea.Asset)
                .Include(ea => ea.Employee)
                .Where(ea => ea.AssetId == assetid)
                .OrderByDescending(ea => ea.AssignedDate)
                .ToList();

            return View(assignment);
        }

        // GET: /Asset/WarrantyOverview (Module 2: Warranty tracking screen)
        [Authorize(Roles = "Admin,ITTechnician,Auditor")]
        public async Task<IActionResult> WarrantyOverview()
        {
            var assets = await _context.Asset
                .OrderBy(a => a.WarrantyExpiryDate)
                .ToListAsync();

            return View(assets);
        }

        // POST: /Asset/ImportCsv (Module 3: Non-blocking bulk importer)
        [HttpPost]
        [Authorize(Roles = "Admin,ITTechnician")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["Error"] = "Please select a valid CSV file.";
                return RedirectToAction("Index");
            }

            var assetsToAdd = new List<Asset>();

            using (var reader = new StreamReader(csvFile.OpenReadStream()))
            {
                // Skip header row
                await reader.ReadLineAsync();

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length >= 2)
                    {
                        assetsToAdd.Add(new Asset
                        {
                            AssetName = values[0].Trim(),
                            SerialNo = values[1].Trim(),
                            Status = AssetStatus.Available
                        });
                    }
                }
            }

            if (assetsToAdd.Any())
            {
                _context.Asset.AddRange(assetsToAdd);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{assetsToAdd.Count} assets imported successfully.";
            }

            return RedirectToAction("Index");
        }
        
    }
}