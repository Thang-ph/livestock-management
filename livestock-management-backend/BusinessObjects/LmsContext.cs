using BusinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using System.Reflection.Emit;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects
{
    public class LmsContext : IdentityDbContext<IdentityUser>
    {
        // Định nghĩa DbSet cho các entity
        public DbSet<Livestock> Livestocks { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Barn> Barns { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<BatchVaccination> BatchVaccinations { get; set; }
        public DbSet<BatchImport> BatchImports { get; set; }
        public DbSet<BatchImportDetail> BatchImportDetails { get; set; }
        public DbSet<BatchExport> BatchExports { get; set; }
        public DbSet<BatchExportDetail> BatchExportDetails { get; set; }
        public DbSet<LivestockVaccination> LivestockVaccinations { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<LivestockProcurement> LivestockProcurements { get; set; }
        public DbSet<ProcurementPackage> ProcurementPackages { get; set; }
        public DbSet<ProcurementDetail> ProcurementDetails { get; set; }
        public DbSet<InspectionCodeCounter> InspectionCodeCounters { get; set; }
        public DbSet<InspectionCodeRange> InspectionCodeRanges { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<DiseaseMedicine> DiseaseMedicines { get; set; }
        public DbSet<VaccinationRequirement> VaccinationRequirement { get; set; }
        public DbSet<BatchVaccinationProcurement> BatchVaccinationProcurement { get; set; }
        public DbSet<SingleVaccination> SingleVaccination { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<PinnedBatchImport> PinnedBatchImports { get; set; }
        public DbSet<OrderRequirement> OrderRequirements { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<InsuranceRequest> InsuranceRequests { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Notification> Notification { get; set; }

        public DbSet<Order> Orders { get; set; }
        public LmsContext(DbContextOptions<DbContext> options) : base(options) { }

        public LmsContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                var rdsConnection = configuration.GetConnectionString("RDSConnection");
                var localConnection = configuration.GetConnectionString("LocalConnection");

                if (string.IsNullOrEmpty(rdsConnection) && string.IsNullOrEmpty(localConnection))
                {
                    throw new Exception("Connection string not found.");
                }

                var connectionString = string.IsNullOrEmpty(rdsConnection) ? localConnection : rdsConnection;

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder optionsBuilder)
        {
            base.OnModelCreating(optionsBuilder);

            foreach (var entityType in optionsBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            // Cấu hình index cho InspectionCode của Livestock (không còn unique)
            optionsBuilder.Entity<Livestock>()
                .HasIndex(l => l.InspectionCode);


            // Cấu hình các mối quan hệ

            //Ref: InspectionCodeCounter.currentRangeId -> InspectionCodeRange.id
            optionsBuilder.Entity<InspectionCodeCounter>()
                .HasOne(counter => counter.InspectionCodeRange)
                .WithMany(range => range.InspectionCodeCounters)
                .HasForeignKey(counter => counter.CurrentRangeId);

            // Ref: livestock.species_id > species.id
            optionsBuilder.Entity<Livestock>()
                .HasOne(l => l.Species)
                .WithMany(s => s.Livestocks)
                .HasForeignKey(l => l.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock.barn_id > barn.id
            optionsBuilder.Entity<Livestock>()
                .HasOne(l => l.Barn)
                .WithMany(b => b.Livestocks)
                .HasForeignKey(l => l.BarnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: medical_history.livestock_id > livestock.id
            optionsBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.Livestock)
                .WithMany(l => l.MedicalHistories)
                .HasForeignKey(mh => mh.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: medical_history.medicine_id > medicine.id
            optionsBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.Medicine)
                .WithMany(m => m.MedicalHistories)
                .HasForeignKey(mh => mh.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_vaccination.vaccine_id > medicine.id
            optionsBuilder.Entity<BatchVaccination>()
                .HasOne(bv => bv.Vaccine)
                .WithMany(m => m.BatchVaccinations)
                .HasForeignKey(bv => bv.VaccineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_import.barn_id > barn.id
            optionsBuilder.Entity<BatchImport>()
                .HasOne(bi => bi.Barn)
                .WithMany(b => b.BatchImports)
                .HasForeignKey(bi => bi.BarnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_import_details.batch_import_id > batch_import.id
            optionsBuilder.Entity<BatchImportDetail>()
                .HasOne(bid => bid.BatchImport)
                .WithMany(bi => bi.BatchImportDetails)
                .HasForeignKey(bid => bid.BatchImportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_import_details.livestock_id > livestock.id
            optionsBuilder.Entity<BatchImportDetail>()
                .HasOne(bid => bid.Livestock)
                .WithMany(l => l.BatchImportDetails)
                .HasForeignKey(bid => bid.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_export.barn_id > barn.id
            optionsBuilder.Entity<BatchExport>()
                .HasOne(be => be.Barn)
                .WithMany(b => b.BatchExports)
                .HasForeignKey(be => be.BarnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_export.procurement_package_id > procurement_package.id
            optionsBuilder.Entity<BatchExport>()
                .HasOne(be => be.ProcurementPackage)
                .WithMany(pp => pp.BatchExports)
                .HasForeignKey(be => be.ProcurementPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_export_details.batch_export_id > batch_export.id
            optionsBuilder.Entity<BatchExportDetail>()
                .HasOne(bed => bed.BatchExport)
                .WithMany(be => be.BatchExportDetails)
                .HasForeignKey(bed => bed.BatchExportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: batch_export_details.livestock_id > livestock.id
            optionsBuilder.Entity<BatchExportDetail>()
                .HasOne(bed => bed.Livestock)
                .WithMany(l => l.BatchExportDetails)
                .HasForeignKey(bed => bed.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock_vaccination.batch_vaccination_id > batch_vaccination.id
            optionsBuilder.Entity<LivestockVaccination>()
                .HasOne(lv => lv.BatchVaccination)
                .WithMany(bv => bv.LivestockVaccinations)
                .HasForeignKey(lv => lv.BatchVaccinationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock_vaccination.livestock_id > livestock.id
            optionsBuilder.Entity<LivestockVaccination>()
                .HasOne(lv => lv.Livestock)
                .WithMany(l => l.LivestockVaccinations)
                .HasForeignKey(lv => lv.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock_procurement.livestock_id > livestock.id
            optionsBuilder.Entity<LivestockProcurement>()
                .HasOne(lp => lp.Livestock)
                .WithMany(l => l.LivestockProcurements)
                .HasForeignKey(lp => lp.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock_procurement.procurement_package_id > procurement_package.id
            optionsBuilder.Entity<LivestockProcurement>()
                .HasOne(lp => lp.ProcurementPackage)
                .WithMany(pp => pp.LivestockProcurements)
                .HasForeignKey(lp => lp.ProcurementPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: procurement_details.procurement_package_id > procurement_package.id
            optionsBuilder.Entity<ProcurementDetail>()
                .HasOne(pd => pd.ProcurementPackage)
                .WithMany(pp => pp.ProcurementDetails)
                .HasForeignKey(pd => pd.ProcurementPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: procurement_details.species_id > species.id
            optionsBuilder.Entity<ProcurementDetail>()
                .HasOne(pd => pd.Species)
                .WithMany(s => s.ProcurementDetails)
                .HasForeignKey(pd => pd.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock_vaccination.livestock_id > livestock.id
            optionsBuilder.Entity<DiseaseMedicine>()
                .HasOne(di => di.Disease)
                .WithMany(dm => dm.DiseaseMedicines)
                .HasForeignKey(dm => dm.DiseaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: livestock_vaccination.livestock_id > livestock.id
            optionsBuilder.Entity<DiseaseMedicine>()
                .HasOne(m => m.Medicine)
                .WithMany(dm => dm.DiseaseMedicines)
                .HasForeignKey(dm => dm.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: medical_history.disease_id > disease.id
            optionsBuilder.Entity<MedicalHistory>()
                .HasOne(d => d.Disease)
                .WithMany(mh => mh.MedicalHistories)
                .HasForeignKey(mh => mh.DiseaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: user_notification.notification_id > notification.id
            optionsBuilder.Entity<UserNotification>()
                .HasOne(n => n.Notification)
                .WithMany(mh => mh.UserNotifications)
                .HasForeignKey(mh => mh.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ref: search_history.livestock_id > livestock.id
            optionsBuilder.Entity<SearchHistory>()
                .HasOne(n => n.Livestock)
                .WithMany(mh => mh.SearchHistories)
                .HasForeignKey(mh => mh.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<VaccinationRequirement>()
                .HasOne(n => n.Disease)
                .WithMany(mh => mh.VaccinationRequirements)
                .HasForeignKey(mh => mh.DiseaseId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<VaccinationRequirement>()
                .HasOne(n => n.ProcurementDetail)
                .WithMany(mh => mh.VaccinationRequirements)
                .HasForeignKey(mh => mh.ProcurementDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<VaccinationRequirement>()
                .HasOne(n => n.OrderRequirement)
                .WithMany(mh => mh.VaccinationRequirements)
                .HasForeignKey(mh => mh.OrderRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<Order>()
                .HasOne(n => n.Customer)
                .WithMany(mh => mh.Orders)
                .HasForeignKey(mh => mh.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<OrderRequirement>()
                .HasOne(n => n.Order)
                .WithMany(mh => mh.OrderRequirements)
                .HasForeignKey(mh => mh.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<OrderRequirement>()
               .HasOne(n => n.Species)
               .WithMany(mh => mh.OrderRequirements)
               .HasForeignKey(mh => mh.SpecieId)
               .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<OrderDetail>()
               .HasOne(n => n.Order)
               .WithMany(mh => mh.OrderDetails)
               .HasForeignKey(mh => mh.OrderId)
               .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<OrderDetail>()
               .HasOne(n => n.Livestock)
               .WithMany(mh => mh.OrderDetails)
               .HasForeignKey(mh => mh.LivestockId)
               .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<InsuranceRequest>()
               .HasOne(n => n.Livestock)
               .WithMany(mh => mh.InsuranceRequests)
               .HasForeignKey(mh => mh.RequestLivestockId)
               .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<InsuranceRequest>()
               .HasOne(n => n.Livestock)
               .WithMany(mh => mh.InsuranceRequests)
               .HasForeignKey(mh => mh.NewLivestockId)
               .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<InsuranceRequest>()
               .HasOne(n => n.Disease)
               .WithMany(mh => mh.InsuranceRequests)
               .HasForeignKey(mh => mh.DiseaseId)
               .OnDelete(DeleteBehavior.Restrict);
            optionsBuilder.Entity<InsuranceRequest>()
                .HasOne(n => n.ProcurementPackage)
                .WithMany(mh => mh.InsuranceRequests)
                .HasForeignKey(mh => mh.ProcurementId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<InsuranceRequest>()
                .HasOne(n => n.Order)
                .WithMany(mh => mh.InsuranceRequests)
                .HasForeignKey(mh => mh.OrderId)
                .OnDelete(DeleteBehavior.Restrict);



            optionsBuilder.Entity<PinnedBatchImport>()
               .HasOne(n => n.BatchImport)
               .WithMany(mh => mh.PinnedBatchImports)
               .HasForeignKey(mh => mh.BatchImportId)
               .OnDelete(DeleteBehavior.Restrict);


            optionsBuilder.Entity<BatchVaccinationProcurement>()
              .HasOne(detail => detail.BatchVaccination)
              .WithMany(bv => bv.BatchVaccinationProcurement)
              .HasForeignKey(detail => detail.BatchVaccinationId)
              .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ: BatchVaccinationProcurementDetail -> ProcurementPackage
            optionsBuilder.Entity<BatchVaccinationProcurement>()
                .HasOne(detail => detail.ProcurementDetail)
                .WithMany(pp => pp.BatchVaccinationProcurement)
                .HasForeignKey(detail => detail.ProcurementDetailId)
                .OnDelete(DeleteBehavior.Restrict);
            optionsBuilder.Entity<SingleVaccination>()
                .HasOne(sv => sv.BatchImport)
                .WithMany(bi => bi.SingleVaccinations)
                .HasForeignKey(sv => sv.BatchImportId)
                .OnDelete(DeleteBehavior.SetNull); // hoặc .Restrict nếu không muốn xóa cascade
            optionsBuilder.Entity<SingleVaccination>()
                .HasOne(sv => sv.Livestock)
                .WithMany(l => l.SingleVaccinations)
                .HasForeignKey(sv => sv.LivestockId)
                .OnDelete(DeleteBehavior.Restrict);

            optionsBuilder.Entity<SingleVaccination>()
                .HasOne(sv => sv.Medicine)
                .WithMany(m => m.SingleVaccinations)
                .HasForeignKey(sv => sv.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            //Update table name
            optionsBuilder.Entity<PinnedBatchImport>().ToTable("PinnedBatchImports");
            optionsBuilder.Entity<SearchHistory>().ToTable("SearchHistories");
            optionsBuilder.Entity <OrderDetail>().ToTable("OrderDetails");
            optionsBuilder.Entity <Customer>().ToTable("Customers");


            //convert enum type in model to string in database
            optionsBuilder.Entity<Livestock>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<livestock_status>());
            optionsBuilder.Entity<Livestock>()
               .Property(e => e.Gender)
               .HasConversion(new EnumToStringConverter<livestock_gender>());
            optionsBuilder.Entity<BatchExport>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<batch_export_status>());
            optionsBuilder.Entity<BatchExportDetail>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<batch_export_status>());
            optionsBuilder.Entity<BatchImportDetail>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<batch_import_status>());
            optionsBuilder.Entity<Medicine>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<medicine_type>());
            optionsBuilder.Entity<BatchVaccination>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<batch_vaccination_status>());
            optionsBuilder.Entity<BatchImport>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<batch_import_status>());
            optionsBuilder.Entity<ProcurementPackage>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<procurement_status>());
            optionsBuilder.Entity<BatchVaccination>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<batch_vaccination_type>());
            optionsBuilder.Entity<Species>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<specie_type>());
            optionsBuilder.Entity<InspectionCodeRange>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<inspection_code_range_status>());
            optionsBuilder.Entity<Disease>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<disease_type>());
            optionsBuilder.Entity<Notification>()
                .Property(e => e.Action)
                .HasConversion(new EnumToStringConverter<entity_action>());
            optionsBuilder.Entity<Notification>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<entity_type>());
            optionsBuilder.Entity<Order>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<order_status>());
            optionsBuilder.Entity<InsuranceRequest>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<insurance_request_status>());
            optionsBuilder.Entity<InsuranceRequest>()
                .Property(e => e.RequestLivestockStatus)
                .HasConversion(new EnumToStringConverter<insurance_request_livestock_status>());
            optionsBuilder.Entity<Order>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<order_type>());
            optionsBuilder.Entity<MedicalHistory>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<medical_history_status>());
        }
    }
}
