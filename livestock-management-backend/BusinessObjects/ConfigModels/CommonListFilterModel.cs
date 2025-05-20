namespace BusinessObjects.ConfigModels
{
    public class CommonListFilterModel
    {
        public string? Keyword { get; set; } = null;
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
    }
}
