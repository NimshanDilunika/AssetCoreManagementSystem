using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetsEmployee.Models
{
    [Table("Asset")]
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; } = string.Empty;

        [Display(Name = "Serial Number")]
        public string? SerialNo { get; set; }

        [NotMapped]
        public string? SerialNumber
        {
            get => SerialNo;
            set => SerialNo = value;
        }

        public string? Description { get; set; }

        public AssetStatus Status { get; set; } = AssetStatus.Available;

        [NotMapped]
        public bool Available
        {
            get => Status == AssetStatus.Available;
            set => Status = value ? AssetStatus.Available : AssetStatus.Assigned;
        }

        // --- Hardware Financials & Straight-Line Depreciation ---

        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime? PurchaseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Purchase Cost ($)")]
        public decimal PurchaseCost { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salvage Floor Value ($)")]
        public decimal SalvageValue { get; set; } = 0m;

        [Display(Name = "Useful Life (Months)")]
        public int UsefulLifeMonths { get; set; } = 36; // Standard 3-year IT hardware lifecycle

        // --- Hardware Warranty & Maintenance (Exclusively on the Asset) ---

        [DataType(DataType.Date)]
        [Display(Name = "Warranty Expiration Date")]
        public DateTime? WarrantyExpiryDate { get; set; }

        [Display(Name = "Warranty Provider / Vendor")]
        public string? WarrantyProvider { get; set; }

        // --- Computed Helper Properties ---

        [NotMapped]
        public bool IsUnderWarranty =>
            WarrantyExpiryDate.HasValue && WarrantyExpiryDate.Value.Date >= DateTime.UtcNow.Date;

        [NotMapped]
        public decimal CurrentBookValue
        {
            get
            {
                if (!PurchaseDate.HasValue || PurchaseCost <= SalvageValue || UsefulLifeMonths <= 0)
                    return PurchaseCost;

                int monthsElapsed = ((DateTime.UtcNow.Year - PurchaseDate.Value.Year) * 12) +
                                    (DateTime.UtcNow.Month - PurchaseDate.Value.Month);

                if (monthsElapsed <= 0) return PurchaseCost;
                if (monthsElapsed >= UsefulLifeMonths) return SalvageValue;

                decimal monthlyDepreciation = (PurchaseCost - SalvageValue) / UsefulLifeMonths;
                return Math.Round(PurchaseCost - (monthlyDepreciation * monthsElapsed), 2);
            }
        }
    }
}