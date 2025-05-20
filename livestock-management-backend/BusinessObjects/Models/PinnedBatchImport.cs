namespace BusinessObjects.Models
{
    public class PinnedBatchImport : BaseEntity
    {
        public string BatchImportId { get; set; }
        public virtual BatchImport BatchImport { get; set; }
    }
}
