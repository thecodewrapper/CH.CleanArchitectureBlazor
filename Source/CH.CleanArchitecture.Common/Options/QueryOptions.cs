namespace CH.CleanArchitecture.Common
{
    public class QueryOptions
    {
        public int? PageNo { get; set; }

        public int? PageSize { get; set; }

        public int? Skip { get; set; }

        public string? SearchTerm { get; set; }

        public string? OrderBy { get; set; }

        public bool IsAscending { get; set; } = true;
    }
}
